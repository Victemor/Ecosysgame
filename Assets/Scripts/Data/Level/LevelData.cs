using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define todos los datos necesarios para un nivel.
/// Actúa como contenedor central de contenido jugable y educativo.
/// </summary>
[CreateAssetMenu(menuName = "Game/Level/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Identification")]

    [SerializeField, Tooltip("Identificador único del nivel. Usado para guardar progreso.")]
    private string id;

    [SerializeField, Tooltip("Nombre mostrado al jugador en menús y HUD.")]
    private string displayName;

    [TextArea]
    [SerializeField, Tooltip("Descripción narrativa o educativa del nivel.")]
    private string description;

    [Header("Scene")]

    [SerializeField, Tooltip("Nombre exacto de la escena de Unity asociada a este nivel.")]
    private string sceneName;

    [Header("Gameplay Content")]

    [SerializeField, Tooltip("Lista de interactuables disponibles en el nivel.")]
    private List<InteractableData> interactables;

    [Header("Objectives")]

    [SerializeField, Tooltip("Objetivos educativos que el jugador debe completar.")]
    private List<ObjectiveData> objectives;

    [Header("Climate")]

    [SerializeField, Tooltip("Perfil climático que define los eventos posibles en el nivel.")]
    private ClimateProfile climateProfile;

    [Header("Audio")]

    [SerializeField, Tooltip("Música de fondo del nivel.")]
    private AudioClip backgroundMusic;

    public string Id          => id;
    public string DisplayName => displayName;
    public string Description => description;
    public string SceneName   => sceneName;

    public IReadOnlyList<InteractableData> Interactables => interactables;
    public IReadOnlyList<ObjectiveData>    Objectives    => objectives;

    public ClimateProfile ClimateProfile   => climateProfile;
    public AudioClip      BackgroundMusic  => backgroundMusic;
}