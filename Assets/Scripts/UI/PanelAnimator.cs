using DG.Tweening;
using UnityEngine;

/// <summary>
/// Anima la aparición y desaparición de un panel con DOTween.
/// Respeta la escala original configurada en el Inspector.
/// </summary>
public class PanelAnimator : MonoBehaviour
{
    [Header("Animation")]

    [SerializeField, Tooltip("Duración de la animación de entrada.")]
    private float showDuration = 0.3f;

    [SerializeField, Tooltip("Duración de la animación de salida.")]
    private float hideDuration = 0.2f;

    [SerializeField, Tooltip("Escala de overshoot al aparecer (efecto rebote).")]
    private float overshootScale = 1.08f;

    // ── Estado interno ───────────────────────────────────────────────

    /// <summary>
    /// Escala original del panel definida en el Inspector.
    /// Se guarda en Awake para que Show() siempre anime hasta este valor.
    /// </summary>
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    // ── API pública ──────────────────────────────────────────────────

    public void Show()
    {
        gameObject.SetActive(true);
        transform.DOKill();
        transform.localScale = Vector3.zero;

        DOTween.Sequence()
            .Append(transform.DOScale(originalScale * overshootScale, showDuration * 0.7f)
                             .SetEase(Ease.OutQuad))
            .Append(transform.DOScale(originalScale, showDuration * 0.3f)
                             .SetEase(Ease.InQuad));
    }

    public void Hide()
    {
        transform.DOKill();

        transform.DOScale(Vector3.zero, hideDuration)
                 .SetEase(Ease.InBack)
                 .OnComplete(() => gameObject.SetActive(false));
    }
}