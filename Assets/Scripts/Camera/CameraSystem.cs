using UnityEngine;

/// <summary>
/// Sistema de cámara con seguimiento completo del personaje (X, Y y Z).
/// Proyecta el rayo central del viewport a la Y real del personaje,
/// funcionando correctamente con cualquier rotación de cámara y terreno irregular.
/// </summary>
public class CameraSystem : MonoBehaviour
{
    [Header("Target")]

    [SerializeField, Tooltip("Transform del personaje a seguir.")]
    private Transform target;

    [Header("Smoothing")]

    [SerializeField, Tooltip("Desactivar para pixel art — evita borrosidad por subpíxel.")]
    private bool useSmoothing = false;

    [SerializeField, Tooltip("Factor de suavizado si está activado.")]
    private float smoothSpeed = 15f;

    [Header("World Boundaries")]

    [SerializeField, Tooltip("Límite mínimo del mundo en X.")]
    private float boundMinX = -50f;

    [SerializeField, Tooltip("Límite máximo del mundo en X.")]
    private float boundMaxX = 50f;

    [SerializeField, Tooltip("Límite mínimo del mundo en Z.")]
    private float boundMinZ = -50f;

    [SerializeField, Tooltip("Límite máximo del mundo en Z.")]
    private float boundMaxZ = 50f;

    // ── Estado interno ───────────────────────────────────────────────

    private Camera  cam;
    private Vector3 currentVelocity;
    private float   halfWidthOnGround;
    private float   halfHeightOnGround;

    /// <summary>
    /// Offset fijo en Y entre la cámara y el personaje.
    /// Se calcula al inicio y se mantiene constante para que la cámara
    /// suba y baje junto al personaje preservando la vista isométrica.
    /// </summary>
    private float cameraYOffset;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (target != null)
            cameraYOffset = transform.position.y - target.position.y;

        RecalculateViewportFootprint();
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = CalculateCameraPosition();
        Vector3 clamped = ApplyBoundaries(desired);

        if (useSmoothing)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, clamped, ref currentVelocity, 1f / smoothSpeed
            );
        }
        else
        {
            transform.position = clamped;
        }
    }

    // ── Cálculo de posición ──────────────────────────────────────────

    /// <summary>
    /// Calcula la posición de la cámara para centrar exactamente al personaje.
    ///
    /// Estrategia:
    /// 1. Proyectar el centro del viewport a la Y REAL del personaje (no a groundY fijo).
    /// 2. El delta XZ entre ese punto y el personaje es cuánto hay que mover la cámara.
    /// 3. La Y de la cámara sigue al personaje manteniendo el offset inicial.
    ///
    /// Esto resuelve el problema de terreno irregular: si el personaje baja o sube,
    /// la proyección y la altura de la cámara se adaptan automáticamente.
    /// </summary>
    private Vector3 CalculateCameraPosition()
    {
        // 1. Proyectar centro del viewport a la Y real del personaje
        Vector3 currentLookPoint = ProjectViewportToY(new Vector3(0.5f, 0.5f, 0f), target.position.y);

        // 2. Delta XZ para centrar al personaje
        float deltaX = target.position.x - currentLookPoint.x;
        float deltaZ = target.position.z - currentLookPoint.z;

        // 3. Y de la cámara sigue la Y del personaje manteniendo offset fijo
        float desiredY = target.position.y + cameraYOffset;

        return new Vector3(
            transform.position.x + deltaX,
            desiredY,
            transform.position.z + deltaZ
        );
    }

    private Vector3 ApplyBoundaries(Vector3 desired)
    {
        return new Vector3(
            Mathf.Clamp(desired.x, boundMinX + halfWidthOnGround,  boundMaxX - halfWidthOnGround),
            desired.y,  // Y no se clampea: sigue libremente al personaje
            Mathf.Clamp(desired.z, boundMinZ + halfHeightOnGround, boundMaxZ - halfHeightOnGround)
        );
    }

    // ── Proyección ───────────────────────────────────────────────────

    /// <summary>
    /// Proyecta un punto del viewport sobre el plano Y = targetY usando el rayo
    /// real de la cámara. Al usar targetY = personaje.Y, la intersección es exacta
    /// sin importar en qué altura esté el personaje.
    /// </summary>
    private Vector3 ProjectViewportToY(Vector3 viewportPoint, float targetY)
    {
        Ray ray = cam.ViewportPointToRay(viewportPoint);

        if (Mathf.Abs(ray.direction.y) < 0.0001f)
            return ray.origin;

        float t = (targetY - ray.origin.y) / ray.direction.y;
        return ray.origin + t * ray.direction;
    }

    private void RecalculateViewportFootprint()
    {
        if (cam == null) return;

        // Usar Y actual del target si existe, si no usar 0
        float refY = target != null ? target.position.y : 0f;

        Vector3 bl = ProjectViewportToY(new Vector3(0f, 0f, 0f), refY);
        Vector3 br = ProjectViewportToY(new Vector3(1f, 0f, 0f), refY);
        Vector3 tl = ProjectViewportToY(new Vector3(0f, 1f, 0f), refY);
        Vector3 tr = ProjectViewportToY(new Vector3(1f, 1f, 0f), refY);

        halfWidthOnGround  = (Mathf.Max(bl.x, br.x, tl.x, tr.x) - Mathf.Min(bl.x, br.x, tl.x, tr.x)) / 2f;
        halfHeightOnGround = (Mathf.Max(bl.z, br.z, tl.z, tr.z) - Mathf.Min(bl.z, br.z, tl.z, tr.z)) / 2f;
    }

    // ── API pública ──────────────────────────────────────────────────

    public void SnapToTarget()
    {
        if (target == null) return;

        currentVelocity = Vector3.zero;

        for (int i = 0; i < 2; i++)
            transform.position = ApplyBoundaries(CalculateCameraPosition());
    }

    public void SetTarget(Transform newTarget)
    {
        target      = newTarget;
        cameraYOffset = transform.position.y - target.position.y;
    }

    // ── Gizmos ───────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        float refY = target != null ? target.position.y : 0f;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.6f);
        DrawGroundRect(boundMinX, boundMaxX, boundMinZ, boundMaxZ, refY);

        if (Application.isPlaying)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            DrawGroundRect(
                boundMinX + halfWidthOnGround, boundMaxX - halfWidthOnGround,
                boundMinZ + halfHeightOnGround, boundMaxZ - halfHeightOnGround,
                refY
            );
        }

        if (cam != null)
        {
            Gizmos.color = new Color(0.4f, 0.6f, 1f, 0.7f);
            Vector3 bl = ProjectViewportToY(new Vector3(0f, 0f, 0f), refY);
            Vector3 br = ProjectViewportToY(new Vector3(1f, 0f, 0f), refY);
            Vector3 tl = ProjectViewportToY(new Vector3(0f, 1f, 0f), refY);
            Vector3 tr = ProjectViewportToY(new Vector3(1f, 1f, 0f), refY);
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);
        }
    }

    private void DrawGroundRect(float minX, float maxX, float minZ, float maxZ, float y)
    {
        y += 0.05f;
        Vector3 bl = new Vector3(minX, y, minZ);
        Vector3 br = new Vector3(maxX, y, minZ);
        Vector3 tr = new Vector3(maxX, y, maxZ);
        Vector3 tl = new Vector3(minX, y, maxZ);
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
}