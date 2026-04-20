using UnityEngine;

/// <summary>
/// Define cómo un evento climático afecta el mundo.
/// No ejecuta lógica, solo describe efectos.
/// </summary>
[System.Serializable]
public class ClimateEffect
{
    [Header("Visual Effects")]

    [SerializeField, Tooltip("Cambio en el nivel de agua.")]
    private float waterLevelModifier;

    [SerializeField, Tooltip("Cambio en la iluminación global.")]
    private float lightIntensityModifier;

    [Header("Gameplay Effects")]

    [SerializeField, Tooltip("Modificador de recursos disponibles.")]
    private float resourceModifier;

    [SerializeField, Tooltip("Modificador de aparición de especies.")]
    private float faunaSpawnModifier;

    public float WaterLevelModifier => waterLevelModifier;
    public float LightIntensityModifier => lightIntensityModifier;

    public float ResourceModifier => resourceModifier;
    public float FaunaSpawnModifier => faunaSpawnModifier;
}