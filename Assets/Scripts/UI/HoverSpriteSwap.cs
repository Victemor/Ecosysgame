using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Comportamiento de hover para botones de UI.
/// Modo A — Sprite swap: cambia imagen al entrar/salir.
/// Modo B — Scale punch: agranda con DOTween si no hay sprite hover asignado.
/// Ambos modos son mutuamente excluyentes y se detectan automáticamente.
/// </summary>
[RequireComponent(typeof(Image))]
public class HoverSpriteSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sprite Swap")]

    [SerializeField, Tooltip("Sprite base. Si está vacío se toma el sprite actual de la Image.")]
    private Sprite spriteNormal;

    [SerializeField, Tooltip("Sprite hover. Si está vacío se activa el modo Scale Punch.")]
    private Sprite spriteHover;

    [Header("Scale Punch (se usa si no hay Sprite Hover)")]

    [SerializeField, Tooltip("Escala máxima al pasar el mouse encima.")]
    private float hoverScale = 1.12f;

    [SerializeField, Tooltip("Duración de la animación de escala.")]
    private float scaleDuration = 0.15f;

    // ── Estado interno ───────────────────────────────────────────────

    private Image   image;
    private bool    useScaleMode;
    private Vector3 originalScale;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        image         = GetComponent<Image>();
        originalScale = transform.localScale;

        if (spriteNormal == null)
            spriteNormal = image.sprite;

        // Si no hay sprite hover, usar modo escala
        useScaleMode = spriteHover == null;
    }

    // ── Hover handlers ───────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (useScaleMode)
        {
            transform.DOKill();
            transform.DOScale(originalScale * hoverScale, scaleDuration)
                     .SetEase(Ease.OutBack);
        }
        else
        {
            image.sprite = spriteHover;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (useScaleMode)
        {
            transform.DOKill();
            transform.DOScale(originalScale, scaleDuration)
                     .SetEase(Ease.InBack);
        }
        else
        {
            image.sprite = spriteNormal;
        }
    }
}