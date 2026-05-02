using TMPro;
using UnityEngine;

/// <summary>
/// Muestra los datos de progreso del jugador en el menú principal.
/// Se suscribe a ProgressManager.OnProgressChanged y actualiza los textos.
/// </summary>
public class ProgressDisplay : MonoBehaviour
{
    [Header("References")]

    [SerializeField, Tooltip("Texto que muestra el tiempo jugado.")]
    private TextMeshProUGUI tiempoText;

    [SerializeField, Tooltip("Texto que muestra los ecopuntos.")]
    private TextMeshProUGUI ecopuntosText;

    [SerializeField, Tooltip("Texto que muestra el progreso total.")]
    private TextMeshProUGUI progresoText;

    [Header("Formato")]

    [SerializeField, Tooltip("Prefijo del texto de tiempo.")]
    private string tiempoPrefix = "Tu tiempo jugado es: ";

    [SerializeField, Tooltip("Prefijo del texto de ecopuntos.")]
    private string ecopuntosPrefix = "La cantidad de ecopuntos que tienes actualmente es: ";

    [SerializeField, Tooltip("Prefijo del texto de progreso.")]
    private string progresoPrefix = "Tu progreso actual es: ";

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void OnEnable()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.OnProgressChanged += RefreshUI;
            RefreshUI();
        }
    }

    private void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged -= RefreshUI;
    }

    // ── Update en tiempo real para el timer ──────────────────────────

    private void Update()
    {
        // Actualizar tiempo en tiempo real si está en gameplay
        if (tiempoText != null && ProgressManager.Instance != null)
            tiempoText.text = tiempoPrefix + ProgressManager.Instance.GetFormattedTime();
    }

    // ── Refresh ──────────────────────────────────────────────────────

    private void RefreshUI()
    {
        if (ProgressManager.Instance == null) return;

        GameProgress p = ProgressManager.Instance.Progress;

        if (ecopuntosText != null)
            ecopuntosText.text = ecopuntosPrefix + p.ecopuntos;

        if (progresoText != null)
            progresoText.text = progresoPrefix + p.progresoTotal.ToString("0.0") + "%";
    }
}