using System.Collections;
using UnityEngine;

/// <summary>
/// Controla efectos de iluminación en respuesta a eventos climáticos.
/// Incluye cambios de intensidad y simulación básica de rayos.
/// </summary>
public class LightingController : MonoBehaviour
{
    [Header("Lighting")]

    [SerializeField, Tooltip("Luz principal del entorno.")]
    private Light mainLight;

    [SerializeField, Tooltip("Duración del destello de rayo.")]
    private float lightningFlashDuration = 0.1f;

    [SerializeField, Tooltip("Intensidad extra durante el rayo.")]
    private float lightningIntensityBoost = 2f;

    private Coroutine lightningRoutine;

    private void OnEnable()
    {
        ClimateController.Instance.OnClimateEventStarted += HandleEventStarted;
        ClimateController.Instance.OnClimateEventEnded += HandleEventEnded;
    }

    private void OnDisable()
    {
        ClimateController.Instance.OnClimateEventStarted -= HandleEventStarted;
        ClimateController.Instance.OnClimateEventEnded -= HandleEventEnded;
    }

    /// <summary>
    /// Aplica cambios al iniciar evento climático.
    /// </summary>
    private void HandleEventStarted(ClimateEventData eventData)
    {
        if (mainLight == null || eventData == null)
            return;

        float modifier = eventData.Effect.LightIntensityModifier;
        mainLight.intensity += modifier;

        // Simulación simple de rayos (si intensidad es alta)
        if (eventData.Intensity > 0.7f)
        {
            lightningRoutine = StartCoroutine(LightningRoutine());
        }
    }

    /// <summary>
    /// Revierte cambios al finalizar evento.
    /// </summary>
    private void HandleEventEnded(ClimateEventData eventData)
    {
        if (mainLight == null || eventData == null)
            return;

        float modifier = eventData.Effect.LightIntensityModifier;
        mainLight.intensity -= modifier;

        if (lightningRoutine != null)
        {
            StopCoroutine(lightningRoutine);
        }
    }

    /// <summary>
    /// Simula rayos intermitentes.
    /// </summary>
    private IEnumerator LightningRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));

            float original = mainLight.intensity;
            mainLight.intensity += lightningIntensityBoost;

            yield return new WaitForSeconds(lightningFlashDuration);

            mainLight.intensity = original;
        }
    }
}