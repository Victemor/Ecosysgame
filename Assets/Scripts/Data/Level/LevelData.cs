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

    [SerializeField, Tooltip("Identificador único del nivel.")]
    private string id;

    [SerializeField, Tooltip("Nombre del nivel.")]
    private string displayName;

    [TextArea]
    [SerializeField, Tooltip("Descripción del nivel.")]
    private string description;

    [Header("Scene")]

    [SerializeField, Tooltip("Nombre de la escena asociada.")]
    private string sceneName;

    [Header("Gameplay Content")]

    [SerializeField, Tooltip("Lista de interactuables presentes en el nivel.")]
    private List<InteractableData> interactables;

    [SerializeField, Tooltip("Lista de NPCs presentes en el nivel.")]
    private List<ScriptableObject> npcs;

    [Header("Objectives")]

    [SerializeField, Tooltip("Objetivos educativos del nivel.")]
    private List<ObjectiveData> objectives;

    [Header("Climate")]

    [SerializeField, Tooltip("Perfil climático del nivel.")]
    private ScriptableObject climateProfile;

    [Header("Audio")]

    [SerializeField, Tooltip("Música del nivel.")]
    private AudioClip backgroundMusic;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public string SceneName => sceneName;

    public IReadOnlyList<InteractableData> Interactables => interactables;
    public IReadOnlyList<ScriptableObject> NPCs => npcs;
    public IReadOnlyList<ObjectiveData> Objectives => objectives;

    public ScriptableObject ClimateProfile => climateProfile;
    public AudioClip BackgroundMusic => backgroundMusic;
}