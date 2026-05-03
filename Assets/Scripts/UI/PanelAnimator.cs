using DG.Tweening;
using UnityEngine;

/// <summary>
/// Anima la aparición y desaparición de un panel con DOTween.
/// Guarda la escala original en Awake para respetarla siempre.
/// Usa SetUpdate(true) para funcionar aunque Time.timeScale sea 0.
/// </summary>
public class PanelAnimator : MonoBehaviour
{
    [Header("Animation")]

    [SerializeField, Tooltip("Duración de la animación de entrada.")]
    private float showDuration = 0.3f;

    [SerializeField, Tooltip("Duración de la animación de salida.")]
    private float hideDuration = 0.2f;

    [SerializeField, Tooltip("Escala de overshoot al aparecer.")]
    private float overshootScale = 1.08f;

    /// <summary>
    /// Escala original del panel definida en el Inspector.
    /// Se guarda antes de cualquier animación para restaurarla correctamente.
    /// </summary>
    private Vector3 originalScale;
    private bool    scaleRecorded;

    private void Awake()
    {
        RecordScale();
    }

    private void RecordScale()
    {
        if (scaleRecorded) return;
        originalScale  = transform.localScale;
        scaleRecorded  = true;
    }

    public void Show()
    {
        RecordScale();

        gameObject.SetActive(true);
        transform.DOKill();
        transform.localScale = Vector3.zero;

        DOTween.Sequence()
            .Append(transform
                .DOScale(originalScale * overshootScale, showDuration * 0.7f)
                .SetEase(Ease.OutQuad))
            .Append(transform
                .DOScale(originalScale, showDuration * 0.3f)
                .SetEase(Ease.InQuad))
            .SetUpdate(true);
    }

    public void Hide()
    {
        RecordScale();

        transform.DOKill();
        transform
            .DOScale(Vector3.zero, hideDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => gameObject.SetActive(false));
    }
}