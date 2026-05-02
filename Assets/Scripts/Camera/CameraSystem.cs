using UnityEngine;

/// <summary>
/// Sistema de cámara isométrica para mundo plano.
/// El cuadro azul (viewport proyectado en el suelo) nunca puede
/// salir del cuadro verde (límites del terreno).
/// </summary>
public class CameraSystem : MonoBehaviour
{
    [Header("Target")]

    [SerializeField, Tooltip("Transform del personaje a seguir.")]
    private Transform target;

    [Header("Smoothing")]

    [SerializeField, Tooltip("Desactivar para pixel art. Evita borrosidad.")]
    private bool useSmoothing = false;

    [SerializeField, Tooltip("Velocidad de suavizado si está activado.")]
    private float smoothSpeed = 15f;

    [Header("World Boundaries")]

    [SerializeField, Tooltip("Renderer del terreno. Define el cuadro verde.")]
    private Renderer groundRenderer;

    // ── Estado interno ───────────────────────────────────────────────

    private Camera  cam;
    private Vector3 currentVelocity;

    /// <summary>
    /// Offsets desde la posición XZ de la cámara hasta cada borde del
    /// cuadro azul proyectado en el suelo. Son constantes porque la
    /// rotación de la cámara no cambia nunca.
    /// </summary>
    private float offsetLeft;   // negativo: cuánto sobresale el viewport a la izquierda
    private float offsetRight;  // positivo: cuánto sobresale a la derecha
    private float offsetBottom; // negativo: cuánto sobresale hacia la cámara (Z-)
    private float offsetTop;    // positivo: cuánto sobresale lejos de la cámara (Z+)

    private bool offsetsReady;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        CalculateViewportOffsets();
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = CalculateCameraPosition();
        Vector3 clamped = offsetsReady ? ClampByViewportEdges(desired) : desired;

        transform.position = useSmoothing
            ? Vector3.SmoothDamp(transform.position, clamped, ref currentVelocity, 1f / smoothSpeed)
            : clamped;
    }

    // ── Offsets del viewport ─────────────────────────────────────────

    /// <summary>
    /// Calcula una sola vez cuánto sobresale el cuadro azul
    /// más allá de la posición de la cámara en cada dirección.
    /// Como la rotación es fija, estos valores nunca cambian.
    /// </summary>
    private void CalculateViewportOffsets()
    {
        offsetsReady = false;

        if (cam == null) return;

        Vector3 camPos = transform.position;

        Vector3 left   = ProjectViewportToY(new Vector3(0f,   0.5f, 0f), 0f);
        Vector3 right  = ProjectViewportToY(new Vector3(1f,   0.5f, 0f), 0f);
        Vector3 bottom = ProjectViewportToY(new Vector3(0.5f, 0f,   0f), 0f);
        Vector3 top    = ProjectViewportToY(new Vector3(0.5f, 1f,   0f), 0f);

        if (!IsValid(left) || !IsValid(right) || !IsValid(bottom) || !IsValid(top))
        {
            Debug.LogWarning("[CameraSystem] No se puede proyectar el viewport al suelo. " +
                             "Verifica que la cámara esté por encima de Y=0.", this);
            return;
        }

        offsetLeft   = left.x   - camPos.x;  // negativo
        offsetRight  = right.x  - camPos.x;  // positivo
        offsetBottom = bottom.z - camPos.z;   // negativo
        offsetTop    = top.z    - camPos.z;   // positivo

        offsetsReady = true;
    }

    // ── Seguimiento ──────────────────────────────────────────────────

    private Vector3 CalculateCameraPosition()
    {
        Vector3 lookPoint = ProjectViewportToY(new Vector3(0.5f, 0.5f, 0f), target.position.y);

        if (!IsValid(lookPoint))
            return transform.position;

        return new Vector3(
            transform.position.x + (target.position.x - lookPoint.x),
            transform.position.y,
            transform.position.z + (target.position.z - lookPoint.z)
        );
    }

    /// <summary>
    /// Clampea la posición de la cámara para que los BORDES del cuadro azul
    /// no salgan del cuadro verde.
    ///
    /// Razonamiento:
    /// - Borde izquierdo del azul = camPos.x + offsetLeft
    /// - Para que no salga del verde: camPos.x + offsetLeft >= greenMinX
    /// - Despejando: camPos.x >= greenMinX - offsetLeft
    ///
    /// Lo mismo para los otros tres lados.
    /// </summary>
    private Vector3 ClampByViewportEdges(Vector3 pos)
    {
        if (groundRenderer == null) return pos;

        Bounds b = groundRenderer.bounds;

        float clampMinX = b.min.x - offsetLeft;    // borde izq azul no sale por izq verde
        float clampMaxX = b.max.x - offsetRight;   // borde der azul no sale por der verde
        float clampMinZ = b.min.z - offsetBottom;  // borde inf azul no sale por inf verde
        float clampMaxZ = b.max.z - offsetTop;     // borde sup azul no sale por sup verde

        // Si el terreno es más pequeño que el viewport, centrar la cámara
        if (clampMinX > clampMaxX) { float cx = (b.min.x + b.max.x) / 2f; clampMinX = cx; clampMaxX = cx; }
        if (clampMinZ > clampMaxZ) { float cz = (b.min.z + b.max.z) / 2f; clampMinZ = cz; clampMaxZ = cz; }

        return new Vector3(
            Mathf.Clamp(pos.x, clampMinX, clampMaxX),
            pos.y,
            Mathf.Clamp(pos.z, clampMinZ, clampMaxZ)
        );
    }

    // ── Proyección ───────────────────────────────────────────────────

    private Vector3 ProjectViewportToY(Vector3 viewportPoint, float targetY)
    {
        Ray ray = cam.ViewportPointToRay(viewportPoint);

        if (Mathf.Abs(ray.direction.y) < 0.0001f)
            return Vector3.positiveInfinity;

        float t = (targetY - ray.origin.y) / ray.direction.y;

        if (t < 0f)
            return Vector3.positiveInfinity;

        return ray.origin + t * ray.direction;
    }

    private bool IsValid(Vector3 p)
    {
        return !float.IsInfinity(p.x) && !float.IsInfinity(p.z)
            && !float.IsNaN(p.x)      && !float.IsNaN(p.z);
    }

    // ── API pública ──────────────────────────────────────────────────

    public void SnapToTarget()
    {
        if (target == null) return;

        CalculateViewportOffsets();
        currentVelocity = Vector3.zero;

        for (int i = 0; i < 3; i++)
            transform.position = ClampByViewportEdges(CalculateCameraPosition());
    }

    public void SetTarget(Transform newTarget) => target = newTarget;

    // ── Gizmos ───────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (cam == null) cam = GetComponent<Camera>();
        CalculateViewportOffsets();

        // Verde — terreno (cuadro verde)
        if (groundRenderer != null)
        {
            Bounds b = groundRenderer.bounds;
            Gizmos.color = new Color(0f, 1f, 0.3f, 0.6f);
            DrawRect(b.min.x, b.max.x, b.min.z, b.max.z, 0.05f);
        }

        // Azul — viewport proyectado en el suelo (cuadro azul)
        if (cam != null)
        {
            Vector3 bl = ProjectViewportToY(new Vector3(0f, 0f, 0f), 0f);
            Vector3 br = ProjectViewportToY(new Vector3(1f, 0f, 0f), 0f);
            Vector3 tl = ProjectViewportToY(new Vector3(0f, 1f, 0f), 0f);
            Vector3 tr = ProjectViewportToY(new Vector3(1f, 1f, 0f), 0f);

            if (IsValid(bl))
            {
                Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.7f);
                Gizmos.DrawLine(bl, br); Gizmos.DrawLine(br, tr);
                Gizmos.DrawLine(tr, tl); Gizmos.DrawLine(tl, bl);
            }
        }
    }

    private void DrawRect(float x0, float x1, float z0, float z1, float y)
    {
        Gizmos.DrawLine(new Vector3(x0, y, z0), new Vector3(x1, y, z0));
        Gizmos.DrawLine(new Vector3(x1, y, z0), new Vector3(x1, y, z1));
        Gizmos.DrawLine(new Vector3(x1, y, z1), new Vector3(x0, y, z1));
        Gizmos.DrawLine(new Vector3(x0, y, z1), new Vector3(x0, y, z0));
    }
}