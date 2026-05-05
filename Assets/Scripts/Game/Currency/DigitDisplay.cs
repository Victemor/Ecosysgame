using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla un único dígito visual.
/// Recibe un valor 0-9 y actualiza su sprite con animación DOTween.
/// </summary>
public class DigitDisplay : MonoBehaviour
{
    [Header("Sprites — asignar del 0 al 9 en orden")]

    [SerializeField, Tooltip("Sprites del 0 al 9.")]
    private Sprite[] digitSprites = new Sprite[10];

    [Header("Animación")]

    [SerializeField, Tooltip("Escala mínima al inicio del punch.")]
    private float scaleDown = 0.6f;

    [SerializeField, Tooltip("Escala máxima durante el punch.")]
    private float scaleUp = 1.25f;

    [SerializeField, Tooltip("Duración total de la animación.")]
    private float duration = 0.3f;

    private Image image;
    private int   currentDigit = 0;

    private void Awake()
    {
        image = GetComponent<Image>();
        ApplySprite(0, false);
    }

    /// <summary>
    /// Actualiza el dígito mostrado.
    /// Si el valor cambió y animate es true, lanza la animación.
    /// </summary>
    public void SetDigit(int digit, bool animate)
    {
        digit = Mathf.Clamp(digit, 0, 9);

        bool changed  = digit != currentDigit;
        currentDigit  = digit;

        ApplySprite(digit, animate && changed);
    }

    private void ApplySprite(int digit, bool animate)
    {
        if (image == null) return;

        if (digitSprites != null && digit < digitSprites.Length && digitSprites[digit] != null)
            image.sprite = digitSprites[digit];

        if (animate)
            PlayPunch();
    }

    /// <summary>
    /// Animación: reduce → crece → vuelve a normal.
    /// </summary>
    private void PlayPunch()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;

        DOTween.Sequence()
            .Append(transform.DOScale(scaleDown, duration * 0.3f).SetEase(Ease.InQuad))
            .Append(transform.DOScale(scaleUp,   duration * 0.5f).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(1f,        duration * 0.2f).SetEase(Ease.InOutQuad));
    }
}