using UnityEngine;

/// <summary>
/// Controla los efectos de iluminación y rayos en respuesta a eventos climáticos.
///
/// Responsabilidades:
///  1. Cambia la intensidad de la luz ambiental según ClimateEffect.LightIntensityModifier.
///  2. Activa un ParticleSystem de rayos cuando ClimateEffect.LightningIntensity supera el umbral.
///
/// DISEÑO — Por qué partículas en lugar de coroutine de luz:
/// El sistema anterior manipulaba directamente la intensidad de la Light con una coroutine,
/// lo que acoplaba la lógica de tiempo con la visual. El ParticleSystem delega
/// timing, variación y burst al propio sistema de partículas, lo que permite
/// ajustar el comportamiento desde el Inspector sin tocar código.
///
/// REQUISITOS DE ESCENA:
/// - lightningParticleSystem debe apuntar a un PS con emisión en Burst o Rate configurada.
/// - mainLight puede dejarse vacío; se busca automáticamente en Start().
/// </summary>
public class LightningController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Ambient Light")]

    [SerializeField, Tooltip("Luz principal del entorno. Si no se asigna, se busca automáticamente.")]
    private Light mainLight;

    [Header("Lightning Particles")]

    [SerializeField, Tooltip("ParticleSystem que emite los efectos visuales de rayo. " +
                              "Debe tener Emission desactivada en reposo (se activa por código).")]
    private ParticleSystem lightningParticleSystem;

    [SerializeField, Range(0f, 1f), Tooltip("Umbral de LightningIntensity a partir del cual se activa el sistema de rayos.")]
    private float lightningThreshold = 0.5f;

    #endregion

    #region Private Fields

    private float baseIntensity;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        ResolveMainLight();
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

    #endregion

    #region Climate Event Handlers

    /// <summary>
    /// Aplica el modificador de luz ambiental del evento.
    /// Activa los rayos por partículas si la intensidad supera el umbral configurado.
    /// </summary>
    private void HandleEventStarted(ClimateEventData eventData)
    {
        if (eventData?.Effect == null)
            return;

        ApplyAmbientLight(eventData.Effect.LightIntensityModifier);

        if (eventData.Effect.LightningIntensity >= lightningThreshold)
            StartLightning(eventData.Effect.LightningIntensity);
    }

    /// <summary>
    /// Restaura la luz ambiental base y detiene el sistema de rayos.
    /// </summary>
    private void HandleEventEnded(ClimateEventData eventData)
    {
        RestoreAmbientLight();
        StopLightning();
    }

    #endregion

    #region Ambient Light

    /// <summary>
    /// Ajusta la intensidad de la luz ambiental desde el valor base.
    /// Se establece desde baseIntensity para evitar acumulación incorrecta
    /// si el evento se inicia varias veces seguidas.
    /// </summary>
    private void ApplyAmbientLight(float modifier)
    {
        if (mainLight == null)
            return;

        mainLight.intensity = baseIntensity + modifier;
    }

    /// <summary>
    /// Restaura la intensidad guardada antes de cualquier evento.
    /// </summary>
    private void RestoreAmbientLight()
    {
        if (mainLight == null)
            return;

        mainLight.intensity = baseIntensity;
    }

    #endregion

    #region Lightning Particles

    /// <summary>
    /// Activa la emisión del ParticleSystem de rayos.
    /// Escala el multiplicador de emisión con la intensidad del evento,
    /// permitiendo rayos más frecuentes en tormentas más intensas.
    /// </summary>
    private void StartLightning(float intensity)
    {
        if (lightningParticleSystem == null)
        {
            Debug.LogWarning("[Lightning] lightningParticleSystem no asignado.", this);
            return;
        }

        var emission                   = lightningParticleSystem.emission;
        emission.enabled               = true;
        emission.rateOverTimeMultiplier = intensity;

        if (!lightningParticleSystem.isPlaying)
            lightningParticleSystem.Play();
    }

    /// <summary>
    /// Detiene y limpia el sistema de partículas de rayos.
    /// </summary>
    private void StopLightning()
    {
        if (lightningParticleSystem == null)
            return;

        var emission     = lightningParticleSystem.emission;
        emission.enabled = false;

        lightningParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Resuelve la referencia a la luz principal y guarda su intensidad base.
    /// Si no está asignada en el Inspector, busca en hijos y luego en la escena.
    /// </summary>
    private void ResolveMainLight()
    {
        if (mainLight == null)
            mainLight = GetComponentInChildren<Light>();

        if (mainLight == null)
            mainLight = FindObjectOfType<Light>();

        if (mainLight == null)
        {
            Debug.LogError("[LightningController] No se encontró ninguna Light en la escena.", this);
            return;
        }

        // Guardamos la intensidad original para restaurarla al final del evento.
        baseIntensity = mainLight.intensity;
    }

    #endregion
}