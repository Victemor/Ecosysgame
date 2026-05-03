using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la bitácora: navegación entre páginas con animación de sprites
/// y transición de contenido con DOTween (scale out → animación → scale in).
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

    [SerializeField, Tooltip("Componente que reproduce la secuencia de sprites.")]
    private SpriteSequenceAnimator spriteAnimator;

    [Header("DOTween Settings")]

    [SerializeField, Tooltip("Duración del scale out.")]
    private float scaleOutDuration = 0.2f;

    [SerializeField, Tooltip("Duración del scale in.")]
    private float scaleInDuration = 0.25f;

    [SerializeField, Tooltip("Overshoot del scale in.")]
    private float scaleInOvershoot = 1.08f;

    [Header("Referencias")]

    [SerializeField, Tooltip("Referencia al GameplayUIManager.")]
    private GameplayUIManager gameplayUIManager;

    // ── Estado interno ───────────────────────────────────────────────

    private int  paginaActual;
    private bool isAnimating;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (gameplayUIManager == null)
            gameplayUIManager = FindFirstObjectByType<GameplayUIManager>();
    }

    private void OnEnable()
    {
        isAnimating  = false; // Resetear siempre al abrir
        paginaActual = paginaInicial;
        StartCoroutine(InitDelayed());
    }

    private IEnumerator InitDelayed()
    {
        yield return null;
        MostrarPaginaInstante(paginaActual);
    }

    // ── Navegación ───────────────────────────────────────────────────

    public void IrPaginaAnterior()
    {
        if (isAnimating || paginas == null || paginas.Length == 0) return;
        int nuevaPagina = (paginaActual - 1 + paginas.Length) % paginas.Length;
        StartCoroutine(CambiarPaginaRoutine(nuevaPagina, isLeft: true));
    }

    public void IrPaginaSiguiente()
    {
        if (isAnimating || paginas == null || paginas.Length == 0) return;
        int nuevaPagina = (paginaActual + 1) % paginas.Length;
        StartCoroutine(CambiarPaginaRoutine(nuevaPagina, isLeft: false));
    }

    public void CerrarBitacora()
    {
        if (isAnimating) return;

        if (gameplayUIManager != null)
        {
            gameplayUIManager.ClosePanelBitacora();
            return;
        }

        PanelAnimator panelAnimator = GetComponentInParent<PanelAnimator>();
        if (panelAnimator != null)
        {
            panelAnimator.Hide();
            return;
        }

        gameObject.SetActive(false);
    }

    // ── Flujo de cambio de página ────────────────────────────────────

    /// <summary>
    /// Usa WaitForSecondsRealtime en lugar de WaitUntil para garantizar
    /// que la coroutine avance aunque Time.timeScale sea 0.
    /// </summary>
    private IEnumerator CambiarPaginaRoutine(int nuevaPagina, bool isLeft)
    {
        isAnimating = true;

        // 1. Scale out — espera duración real
        ScaleOut();
        yield return new WaitForSecondsRealtime(scaleOutDuration);

        // 2. Animación de sprites
        if (spriteAnimator != null)
        {
            if (isLeft)
                spriteAnimator.PlayAnimationA();
            else
                spriteAnimator.PlayAnimationB();

            float timeout = 5f;
            float elapsed = 0f;

            while (spriteAnimator.IsPlaying && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // 3. Actualizar índice y datos
        paginaActual = nuevaPagina;
        CargarDatosPagina(paginaActual);

        // 4. Scale in — espera duración real
        ScaleIn();
        yield return new WaitForSecondsRealtime(scaleInDuration);

        isAnimating = false;
    }

    // ── DOTween ──────────────────────────────────────────────────────

    /// <summary>
    /// Scale out sin coroutine — dispara la animación y retorna inmediatamente.
    /// El caller espera con WaitForSecondsRealtime.
    /// </summary>
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

        Debug.Log($"[BitacoraController] Mostrando página {indice}: {pagina.nombre}");

        if (textoNombre       != null) textoNombre.text       = pagina.nombre;
        if (textoDescripcion1 != null) textoDescripcion1.text = pagina.descripcion1;
        if (textoDescripcion2 != null) textoDescripcion2.text = pagina.descripcion2;
        if (imagenPagina      != null) imagenPagina.sprite    = pagina.imagen;
    }
}