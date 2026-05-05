// BitacoraController.cs
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la bitácora: navegación entre páginas con animación de sprites
/// y transición de contenido con DOTween (scale out → animación → scale in).
/// Vive en GameplayManagers (siempre activo), no dentro del panel.
/// </summary>
public class BitacoraController : MonoBehaviour
{
    [Header("Páginas")]

    [SerializeField, Tooltip("Lista de páginas de la bitácora en orden.")]
    private BitacoraPageData[] paginas;

    [SerializeField, Tooltip("Índice de la página inicial al abrir la bitácora.")]
    private int paginaInicial = 0;

    [Header("UI — Contenido")]

    [SerializeField, Tooltip("Texto del nombre de la página.")]
    private TextMeshProUGUI textoNombre;

    [SerializeField, Tooltip("Texto de la primera descripción.")]
    private TextMeshProUGUI textoDescripcion1;

    [SerializeField, Tooltip("Texto de la segunda descripción.")]
    private TextMeshProUGUI textoDescripcion2;

    [SerializeField, Tooltip("Imagen de la página actual.")]
    private Image imagenPagina;

    [Header("Contenedor de contenido")]

    [SerializeField, Tooltip("Transform que agrupa textos e imagen para el scale.")]
    private Transform contenidoContainer;

    [SerializeField, Tooltip("Escala real del contenidoContainer tal como está en el Inspector.")]
    private Vector3 contenidoTargetScale = Vector3.one;

    [Header("Animación de sprites")]

    [SerializeField, Tooltip("Componente que reproduce la secuencia de sprites al pasar página.")]
    private SpriteSequenceAnimator spriteAnimator;

    [Header("DOTween Settings")]

    [SerializeField, Tooltip("Duración del scale out al cambiar página.")]
    private float scaleOutDuration = 0.2f;

    [SerializeField, Tooltip("Duración del scale in al cambiar página.")]
    private float scaleInDuration = 0.25f;

    [SerializeField, Tooltip("Overshoot del scale in.")]
    private float scaleInOvershoot = 1.08f;

    [Header("Referencias")]

    [SerializeField, Tooltip("Referencia al GameplayUIManager. Se busca automáticamente si está vacío.")]
    private GameplayUIManager gameplayUIManager;

    // ── Estado interno ───────────────────────────────────────────────

    private int  paginaActual;
    private bool isAnimating;
    private Coroutine paginaRoutine;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (gameplayUIManager == null)
            gameplayUIManager = FindFirstObjectByType<GameplayUIManager>();

        if (gameplayUIManager == null)
            Debug.LogError("[BitacoraController] GameplayUIManager no encontrado.", this);
    }

    /// <summary>
    /// Este OnEnable corre al activarse GameplayManagers (inicio de escena),
    /// NO al abrir el panel. Por eso la inicialización de página se hace
    /// aquí para tener los datos listos cuando el panel aparezca.
    /// </summary>
    private void OnEnable()
    {
        ResetState();
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Reinicia la bitácora a la página inicial.
    /// Llamar desde GameplayUIManager al abrir el panel si se necesita reset.
    /// </summary>
    public void ResetState()
    {
        // Cancelar cualquier animación en curso antes de resetear
        if (paginaRoutine != null)
        {
            StopCoroutine(paginaRoutine);
            paginaRoutine = null;
        }

        isAnimating  = false;
        paginaActual = paginaInicial;
        MostrarPaginaInstante(paginaActual);
    }

    public void IrPaginaAnterior()
    {
        if (isAnimating || paginas == null || paginas.Length == 0) return;
        int nuevaPagina = (paginaActual - 1 + paginas.Length) % paginas.Length;
        paginaRoutine = StartCoroutine(CambiarPaginaRoutine(nuevaPagina, isLeft: true));
    }

    public void IrPaginaSiguiente()
    {
        if (isAnimating || paginas == null || paginas.Length == 0) return;
        int nuevaPagina = (paginaActual + 1) % paginas.Length;
        paginaRoutine = StartCoroutine(CambiarPaginaRoutine(nuevaPagina, isLeft: false));
    }

    /// <summary>
    /// Cierra la bitácora. El cierre NUNCA se bloquea por isAnimating:
    /// se cancela la animación en curso y se delega el cierre al manager.
    /// </summary>
    public void CerrarBitacora()
    {
        // Cancelar animación de página en curso si la hay
        if (paginaRoutine != null)
        {
            StopCoroutine(paginaRoutine);
            paginaRoutine = null;
        }

        isAnimating = false;

        // Restaurar escala del contenido por si quedó en 0 (scale out sin completar)
        if (contenidoContainer != null)
        {
            contenidoContainer.DOKill();
            contenidoContainer.localScale = contenidoTargetScale;
        }

        if (gameplayUIManager != null)
        {
            gameplayUIManager.ClosePanelBitacora();
            return;
        }

        // Fallback: buscar el PanelAnimator padre directamente
        PanelAnimator panelAnimator = GetComponentInParent<PanelAnimator>();
        if (panelAnimator != null)
        {
            panelAnimator.Hide();
            return;
        }

        Debug.LogWarning("[BitacoraController] No se encontró GameplayUIManager ni PanelAnimator padre.", this);
        gameObject.SetActive(false);
    }

    // ── Cambio de página ─────────────────────────────────────────────

    /// <summary>
    /// Usa WaitForSecondsRealtime para funcionar con Time.timeScale en 0.
    /// </summary>
    private IEnumerator CambiarPaginaRoutine(int nuevaPagina, bool isLeft)
    {
        isAnimating = true;

        // 1. Scale out
        ScaleOut();
        yield return new WaitForSecondsRealtime(scaleOutDuration);

        // 2. Animación de sprites (con timeout de seguridad)
        if (spriteAnimator != null)
        {
            if (isLeft) spriteAnimator.PlayAnimationA();
            else        spriteAnimator.PlayAnimationB();

            float timeout = 5f;
            float elapsed = 0f;

            while (spriteAnimator.IsPlaying && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // 3. Datos de la nueva página
        paginaActual = nuevaPagina;
        CargarDatosPagina(paginaActual);

        // 4. Scale in
        ScaleIn();
        yield return new WaitForSecondsRealtime(scaleInDuration);

        isAnimating   = false;
        paginaRoutine = null;
    }

    // ── DOTween ──────────────────────────────────────────────────────

    private void ScaleOut()
    {
        if (contenidoContainer == null) return;
        contenidoContainer.DOKill();
        contenidoContainer
            .DOScale(Vector3.zero, scaleOutDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true);
    }

    private void ScaleIn()
    {
        if (contenidoContainer == null) return;
        contenidoContainer.localScale = Vector3.zero;

        DOTween.Sequence()
            .Append(contenidoContainer
                .DOScale(contenidoTargetScale * scaleInOvershoot, scaleInDuration * 0.7f)
                .SetEase(Ease.OutQuad))
            .Append(contenidoContainer
                .DOScale(contenidoTargetScale, scaleInDuration * 0.3f)
                .SetEase(Ease.InQuad))
            .SetUpdate(true);
    }

    // ── Datos ────────────────────────────────────────────────────────

    private void MostrarPaginaInstante(int indice)
    {
        if (contenidoContainer != null)
            contenidoContainer.localScale = contenidoTargetScale;

        CargarDatosPagina(indice);
    }

    private void CargarDatosPagina(int indice)
    {
        if (paginas == null || paginas.Length == 0)
        {
            Debug.LogWarning("[BitacoraController] No hay páginas asignadas.", this);
            return;
        }

        indice = Mathf.Clamp(indice, 0, paginas.Length - 1);
        BitacoraPageData pagina = paginas[indice];

        if (pagina == null)
        {
            Debug.LogWarning($"[BitacoraController] Página {indice} es null.", this);
            return;
        }

        if (textoNombre       != null) textoNombre.text       = pagina.nombre;
        if (textoDescripcion1 != null) textoDescripcion1.text = pagina.descripcion1;
        if (textoDescripcion2 != null) textoDescripcion2.text = pagina.descripcion2;
        if (imagenPagina      != null) imagenPagina.sprite    = pagina.imagen;
    }
}