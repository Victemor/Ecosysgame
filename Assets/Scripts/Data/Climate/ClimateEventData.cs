using UnityEngine;

/// <summary>
/// Define un evento climático específico.
/// Contiene datos educativos y efectos en el gameplay.
/// </summary>
[CreateAssetMenu(menuName = "Game/Climate/Climate Event")]
public class ClimateEventData : ScriptableObject
{
    [Header("Identification")]

    [SerializeField, Tooltip("ID único del evento climático.")]
    private string id;

    [SerializeField, Tooltip("Nombre del evento climático.")]
    private string displayName;

    [TextArea]
    [SerializeField, Tooltip("Descripción educativa del evento.")]
    private string description;

    [Header("Behavior")]

    [SerializeField, Tooltip("Duración del evento en segundos.")]
    private float duration;

    [SerializeField, Tooltip("Intensidad del evento.")]
    private float intensity;

    [Header("Effects")]

    [SerializeField, Tooltip("Configuración de efectos aplicados durante el evento.")]
    private ClimateEffect effect;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;

    public float Duration => duration;
    public float Intensity => intensity;

    public ClimateEffect Effect => effect;
}