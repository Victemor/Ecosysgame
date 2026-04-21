using System.Collections;
using UnityEngine;

/// <summary>
/// Controla efectos de iluminación en respuesta a eventos climáticos.
/// Guarda la intensidad base y la restaura al finalizar el evento,
/// evitando acumulación incorrecta si el evento se inicia varias veces.
/// </summary>
public class LightingController : MonoBehaviour
{
    [Header("Lighting")]

    [SerializeField, Tooltip("Luz principal del entorno. Si no se asigna, se busca automáticamente.")]
    private Light mainLight;

    [Header("Lightning Effect")]

    [SerializeField, Tooltip("Duración del destello de rayo en segundos.")]
    private float lightningFlashDuration = 0.1f;

    [SerializeField, Tooltip("Boost de intensidad durante el destello de rayo.")]
    private float lightningIntensityBoost = 2f;

    private float baseIntensity;
    private Coroutine lightningRoutine;

    private void Awake()
    {
        // Auto-find: evita tener que arrastrarlo en el Inspector en cada escena.
        if (mainLight == null)
            mainLight = GetComponentInChildren<Light>();

        if (mainLight == null)
            mainLight = FindObjectOfType<Light>();

        if (mainLight == null)
        {
            Debug.LogError("[LightingController] No se encontró ninguna Light en la escena.", this);
            return;
        }

        // Guardamos la intensidad original para restaurarla correctamente al final del evento.
        baseIntensity = mainLight.intensity;
    }

    private void OnEnable()
    {
        ClimateController.Instance.OnClimateEventStarted += HandleEventStarted;
        ClimateController.Instance.OnClimateEventEnded   += HandleEventEnded;
    }

    private void OnDisable()
    {
        ClimateController.Instance.OnClimateEventStarted -= HandleEventStarted;
        ClimateController.Instance.OnClimateEventEnded   -= HandleEventEnded;
    }

    /// <summary>
    /// Aplica el modificador de iluminación del evento climático.
    /// Se establece desde el valor base para evitar acumulación si el evento se repite.
    /// </summary>
    private void HandleEventStarted(ClimateEventData eventData)
    {
        if (mainLight == null || eventData == null)
            return;

        mainLight.intensity = baseIntensity + eventData.Effect.LightIntensityModifier;

        if (eventData.Intensity > 0.7f)
            lightningRoutine = StartCoroutine(LightningRoutine());
    }

    /// <summary>
    /// Restaura la intensidad base al finalizar el evento.
    /// </summary>
    private void HandleEventEnded(ClimateEventData eventData)
    {
        if (mainLight == null)
            return;

        mainLight.intensity = baseIntensity;

        if (lightningRoutine != null)
        {
            StopCoroutine(lightningRoutine);
            lightningRoutine = null;
        }
    }

    /// <summary>
    /// Simula rayos intermitentes durante eventos de alta intensidad.
    /// Opera sobre el valor de intensidad actual (post-modificador), no el base.
    /// </summary>
    private IEnumerator LightningRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));

            float currentIntensity   = mainLight.intensity;
            mainLight.intensity      = currentIntensity + lightningIntensityBoost;

            yield return new WaitForSeconds(lightningFlashDuration);

            mainLight.intensity = currentIntensity;
        }
    }
}