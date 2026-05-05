using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la presentación visual de un slot del inventario.
/// Gestiona el alpha, el punch al recibir un ítem y la escala al seleccionarse.
/// No conoce al InventorySystem — se comunica solo mediante eventos.
/// </summary>
[RequireComponent(typeof(Button))]
public class InventorySlotUI : MonoBehaviour
{
    [Header("Referencias")]

    [SerializeField, Tooltip("Imagen de fondo/marco del slot (la que tiene el alpha predefinido).")]
    private Image slotImage;

    [SerializeField, Tooltip("Imagen hija donde se mostrará el ícono del ítem.")]
    private Image itemImage;

    // ── Estado interno ───────────────────────────────────────────────

    private int       slotIndex;
    private float     baseAlpha;
    private bool      isSelected;
    private Coroutine scaleRoutine;

    // ── Constantes de animación ──────────────────────────────────────

    private const float PunchScalePeak    = 1.25f;
    private const float PunchDurationUp   = 0.08f;
    private const float PunchDurationDown = 0.14f;
    private const float SelectedScale     = 1.10f;
    private const float NormalScale       = 1.00f;
    private const float SelectDuration    = 0.12f;

    /// <summary>
    /// Se dispara cuando el jugador hace click en este slot.
    /// Proporciona el índice del slot para que InventoryUI lo reenvíe al sistema.
    /// </summary>
    public event Action<int> OnClicked;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        // Leer el alpha base del prefab en lugar de hardcodearlo.
        // Así el diseñador controla el valor desde el Inspector.
        baseAlpha = slotImage.color.a;

        // El ítem empieza invisible; el sprite se asigna al llenar el slot.
        SetAlpha(itemImage, 0f);

        GetComponent<Button>().onClick.AddListener(() => OnClicked?.Invoke(slotIndex));
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Asigna el índice a este slot. Debe llamarse desde InventoryUI al inicializar.
    /// </summary>
    public void Initialize(int index)
    {
        slotIndex = index;
    }

    /// <summary>
    /// Muestra el ítem: asigna el sprite, sube el alpha al máximo y ejecuta el punch.
    /// </summary>
    public void SetItem(ItemData item)
    {
        itemImage.sprite = item.Icon;
        SetAlpha(itemImage, 1f);
        SetAlpha(slotImage, 1f);

        PlayPunch();
    }

    /// <summary>
    /// Limpia el slot: quita el sprite y restaura el alpha base del marco.
    /// </summary>
    public void ClearItem()
    {
        itemImage.sprite = null;
        SetAlpha(itemImage, 0f);
        SetAlpha(slotImage, baseAlpha);
    }

    /// <summary>
    /// Aplica o quita el estado de selección con una animación de escala.
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        AnimateToScale(isSelected ? SelectedScale : NormalScale, SelectDuration);
    }

    // ── Animaciones ──────────────────────────────────────────────────

    /// <summary>
    /// Punch de entrada: sube a PunchScalePeak y vuelve a la escala objetivo.
    /// Usa unscaledDeltaTime para funcionar aunque Time.timeScale sea 0.
    /// Si hay una animación de escala en curso la interrumpe para no acumular coroutines.
    /// </summary>
    private void PlayPunch()
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        float targetScale = isSelected ? SelectedScale : NormalScale;

        yield return TweenScale(PunchScalePeak, PunchDurationUp);
        yield return TweenScale(targetScale,    PunchDurationDown);

        scaleRoutine = null;
    }

    private void AnimateToScale(float target, float duration)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(AnimateScaleRoutine(target, duration));
    }

    private IEnumerator AnimateScaleRoutine(float target, float duration)
    {
        yield return TweenScale(target, duration);
        scaleRoutine = null;
    }

    /// <summary>
    /// Interpola el localScale del slot hacia el target en la duración indicada.
    /// Usa unscaledDeltaTime para ser inmune a pausas del juego.
    /// </summary>
    private IEnumerator TweenScale(float target, float duration)
    {
        Vector3 from    = transform.localScale;
        Vector3 to      = Vector3.one * target;
        float   elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed             += Time.unscaledDeltaTime;
            float t              = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.localScale = to;
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a     = alpha;
        image.color = c;
    }
}