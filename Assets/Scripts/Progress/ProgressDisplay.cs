using TMPro;
using UnityEngine;

/// <summary>
/// Muestra datos de progreso en tiempo real.
/// Funciona tanto en menú como en gameplay.
/// El tiempo se actualiza cada frame desde Update().
/// </summary>
public class ProgressDisplay : MonoBehaviour
{
    [Header("Textos")]

    [SerializeField, Tooltip("Texto que muestra el tiempo jugado.")]
    private TextMeshProUGUI tiempoText;

    [SerializeField, Tooltip("Texto que muestra los ecopuntos.")]
    private TextMeshProUGUI ecopuntosText;

    [SerializeField, Tooltip("Texto que muestra el progreso total.")]
    private TextMeshProUGUI progresoText;

    [Header("Prefijos")]

    [SerializeField]
    private string tiempoPrefix = "Tu tiempo jugado es: ";

    [SerializeField]
    private string ecopuntosPrefix = "La cantidad de ecopuntos que tienes actualmente es: ";

    [SerializeField]
    private string progresoPrefix = "Tu progreso actual es: ";

    private void OnEnable()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.OnProgressChanged += RefreshStatic;
            RefreshStatic();
        }
    }

    private void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged -= RefreshStatic;
    }

    /// <summary>
    /// Actualiza el tiempo en tiempo real cada frame.
    /// Así si el panel está abierto durante gameplay, el timer sigue corriendo visualmente.
    /// </summary>
    private void Update()
    {
        if (tiempoText == null || ProgressManager.Instance == null) return;
        tiempoText.text = tiempoPrefix + ProgressManager.Instance.GetFormattedTime();
    }

    /// <summary>
    /// Actualiza ecopuntos y progreso (no cambian cada frame).
    /// Se llama al suscribirse y cuando OnProgressChanged dispara.
    /// </summary>
    private void RefreshStatic()
    {
        if (ProgressManager.Instance == null) return;

        GameProgress p = ProgressManager.Instance.Progress;

        if (ecopuntosText != null)
            ecopuntosText.text = ecopuntosPrefix + p.ecopuntos;

        if (progresoText != null)
            progresoText.text = progresoPrefix + p.progresoTotal.ToString("0.0") + "%";
    }
}