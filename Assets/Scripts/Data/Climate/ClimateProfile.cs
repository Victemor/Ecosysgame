using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define cómo se comporta el clima dentro de un nivel.
/// </summary>
[CreateAssetMenu(menuName = "Game/Climate/Climate Profile")]
public class ClimateProfile : ScriptableObject
{
    [Header("Climate Events")]

    [SerializeField, Tooltip("Lista de eventos posibles en este nivel.")]
    private List<ClimateEventData> possibleEvents;

    [SerializeField, Tooltip("Probabilidad de activación de eventos.")]
    private float eventProbability;

    [SerializeField, Tooltip("Tiempo entre eventos (segundos).")]
    private float timeBetweenEvents;

    public IReadOnlyList<ClimateEventData> PossibleEvents => possibleEvents;
    public float EventProbability => eventProbability;
    public float TimeBetweenEvents => timeBetweenEvents;
}