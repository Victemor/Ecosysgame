using UnityEngine;

/// <summary>
/// Define cómo un evento climático afecta el mundo.
/// No ejecuta lógica, solo describe efectos.
/// Cada campo es leído directamente por el controlador visual correspondiente.
/// </summary>
[System.Serializable]
public class ClimateEffect
{
    [Header("Visual Effects — Water")]

    [SerializeField, Tooltip("Cambio en el nivel de agua.")]
    private float waterLevelModifier;

    [SerializeField, Min(0), Tooltip("Partículas de lluvia por segundo. 0 = sin lluvia, 300 = tormenta intensa.")]
    private float rainIntensity;

    [Header("Visual Effects — Lighting")]

    [SerializeField, Tooltip("Cambio en la intensidad de la luz ambiental.")]
    private float lightIntensityModifier;

    [SerializeField, Range(0f, 1f), Tooltip("Intensidad del sistema de rayos por partículas. 0 = sin rayos.")]
    private float lightningIntensity;

    [Header("Gameplay Effects")]

    [SerializeField, Tooltip("Modificador de recursos disponibles.")]
    private float resourceModifier;

    [SerializeField, Tooltip("Modificador de aparición de especies.")]
    private float faunaSpawnModifier;

    public float WaterLevelModifier    => waterLevelModifier;
    public float RainIntensity         => rainIntensity;
    public float LightIntensityModifier => lightIntensityModifier;
    public float LightningIntensity    => lightningIntensity;
    public float ResourceModifier      => resourceModifier;
    public float FaunaSpawnModifier    => faunaSpawnModifier;
}