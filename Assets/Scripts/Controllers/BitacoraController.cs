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

    [SerializeField, Tooltip("Transform que agrupa textos e imagen. Es el que se escala con DOTween.")]
    private Transform contenidoContainer;

    [Header("Animación de sprites")]

    [SerializeField, Tooltip("Componente que reproduce la secuencia de sprites.")]
    private SpriteSequenceAnimator spriteAnimator;

    [Header("DOTween Settings")]

    [SerializeField, Tooltip("Duración del scale out (desaparecer contenido).")]
    private float scaleOutDuration = 0.2f;

    [SerializeField, Tooltip("Duración del scale in (aparecer contenido).")]
    private float scaleInDuration = 0.25f;

    [SerializeField, Tooltip("Escala de overshoot al hacer scale in.")]
    private float scaleInOvershoot = 1.08f;

    // ── Estado interno ───────────────────────────────────────────────

    private int     paginaActual;
    private bool    isAnimating;
    private Vector3 originalScale;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (contenidoContainer != null)
            originalScale = contenidoContainer.localScale;
    }

    private void OnEnable()
    {
        paginaActual = paginaInicial;
        MostrarPaginaInstante(paginaActual);
    }

    // ── Navegación ───────────────────────────────────────────────────

    /// <summary>
    /// Navega a la página anterior (izquierda). Con loop.
    /// Dispara animación A.
    /// </summary>
    public void IrPaginaAnterior()
    {
        if (isAnimating || paginas == null || paginas.Length == 0) return;

        int nuevaPagina = (paginaActual - 1 + paginas.Length) % paginas.Length;
        StartCoroutine(CambiarPaginaRoutine(nuevaPagina, isLeft: true));
    }

    /// <summary>
    /// Navega a la página siguiente (derecha). Con loop.
    /// Dispara animación B.
    /// </summary>
    public void IrPaginaSiguiente()
    {
        if (isAnimating || paginas == null || paginas.Length == 0) return;

        int nuevaPagina = (paginaActual + 1) % paginas.Length;
        StartCoroutine(CambiarPaginaRoutine(nuevaPagina, isLeft: false));
    }

    // ── Flujo de animación ───────────────────────────────────────────

    /// <summary>
    /// Flujo completo de cambio de página:
    /// 1. Scale out del contenido actual
    /// 2. Animación de sprites (A = izquierda, B = derecha)
    /// 3. Cargar datos de la nueva página
    /// 4. Scale in del nuevo contenido
    /// </summary>
    private IEnumerator CambiarPaginaRoutine(int nuevaPagina, bool isLeft)
    {
        isAnimating = true;

        // 1. Scale out — contenido desaparece
        yield return ScaleOut();

        // 2. Animación de sprites
        if (spriteAnimator != null)
        {
            if (isLeft)
                spriteAnimator.PlayAnimationA();
            else
                spriteAnimator.PlayAnimationB();

            // Esperar a que termine la animación de sprites
            yield return new WaitUntil(() => !spriteAnimator.IsPlaying);
        }

        // 3. Actualizar índice y cargar datos
        paginaActual = nuevaPagina;
        CargarDatosPagina(paginaActual);

        // 4. Scale in — nuevo contenido aparece
        yield return ScaleIn();

        isAnimating = false;
    }

    // ── DOTween ──────────────────────────────────────────────────────

    private IEnumerator ScaleOut()
    {
        if (contenidoContainer == null) yield break;

        bool done = false;

        contenidoContainer.DOKill();
        contenidoContainer
            .DOScale(Vector3.zero, scaleOutDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => done = true);

        yield return new WaitUntil(() => done);
    }

    private IEnumerator ScaleIn()
    {
        if (contenidoContainer == null) yield break;

        contenidoContainer.localScale = Vector3.zero;

        bool done = false;

        DOTween.Sequence()
            .Append(contenidoContainer
                .DOScale(originalScale * scaleInOvershoot, scaleInDuration * 0.7f)
                .SetEase(Ease.OutQuad))
            .Append(contenidoContainer
                .DOScale(originalScale, scaleInDuration * 0.3f)
                .SetEase(Ease.InQuad))
            .OnComplete(() => done = true);

        yield return new WaitUntil(() => done);
    }

    // ── Datos ────────────────────────────────────────────────────────

    /// <summary>
    /// Carga los datos de la página en la UI sin animación.
    /// Usado al abrir la bitácora por primera vez.
    /// </summary>
    private void MostrarPaginaInstante(int indice)
    {
        if (contenidoContainer != null)
            contenidoContainer.localScale = originalScale;

        CargarDatosPagina(indice);
    }

    private void CargarDatosPagina(int indice)
    {
        if (paginas == null || paginas.Length == 0) return;

        BitacoraPageData pagina = paginas[indice];
        if (pagina == null) return;

        if (textoNombre      != null) textoNombre.text      = pagina.nombre;
        if (textoDescripcion1 != null) textoDescripcion1.text = pagina.descripcion1;
        if (textoDescripcion2 != null) textoDescripcion2.text = pagina.descripcion2;
        if (imagenPagina     != null) imagenPagina.sprite   = pagina.imagen;
    }
}