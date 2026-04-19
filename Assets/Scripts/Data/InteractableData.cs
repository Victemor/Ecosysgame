using UnityEngine;

/// <summary>
/// Define los datos base de un objeto interactuable.
/// Contiene únicamente información, sin lógica de ejecución.
/// </summary>
[CreateAssetMenu(menuName = "Game/Interactable/Interactable Data")]
public class InteractableData : ScriptableObject
{
    [Header("Identification")]

    [SerializeField, Tooltip("Identificador único del interactuable.")]
    private string id;

    [SerializeField, Tooltip("Nombre visible del objeto.")]
    private string displayName;

    [TextArea]
    [SerializeField, Tooltip("Descripción educativa mostrada al jugador.")]
    private string description;

    [SerializeField, Tooltip("Icono usado en UI.")]
    private Sprite icon;

    [Header("Interaction")]

    [SerializeField, Tooltip("Tipo principal de interacción.")]
    private InteractionType interactionType;

    [SerializeField, Tooltip("Indica si puede activar un diálogo.")]
    private bool triggersDialogue;

    [SerializeField, Tooltip("Indica si puede activar un evento climático.")]
    private bool triggersClimateEvent;

    [Header("References")]

    [SerializeField, Tooltip("Referencia al diálogo asociado.")]
    private ScriptableObject dialogueData;

    [SerializeField, Tooltip("Referencia al evento climático asociado.")]
    private ScriptableObject climateEventData;

    // PROPERTIES

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    public InteractionType InteractionType => interactionType;

    public bool TriggersDialogue => triggersDialogue;
    public bool TriggersClimateEvent => triggersClimateEvent;

    public ScriptableObject DialogueData => dialogueData;
    public ScriptableObject ClimateEventData => climateEventData;
}