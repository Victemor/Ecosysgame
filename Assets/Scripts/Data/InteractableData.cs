using UnityEngine;

/// <summary>
/// Define los datos base de un objeto interactuable.
/// Contiene únicamente información, sin lógica de ejecución.
/// </summary>
[CreateAssetMenu(menuName = "Game/Interactable/Interactable Data")]
public class InteractableData : ScriptableObject
{
    [Header("Identification")]

    [SerializeField, Tooltip("Identificador único del interactuable. Usado por el sistema de progresión.")]
    private string id;

    [SerializeField, Tooltip("Nombre visible del objeto en la UI.")]
    private string displayName;

    [TextArea]
    [SerializeField, Tooltip("Descripción educativa mostrada al jugador al inspeccionar.")]
    private string description;

    [SerializeField, Tooltip("Icono representativo usado en la UI.")]
    private Sprite icon;

    [Header("Interaction")]

    [SerializeField, Tooltip("Tipo de interacción que ejecuta este objeto.")]
    private InteractionType interactionType;

    [Header("Dialogue")]

    [SerializeField, Tooltip("Activa si este objeto inicia un diálogo al interactuar.")]
    private bool triggersDialogue;

    [SerializeField, Tooltip("Datos del diálogo asociado. Requiere que 'Triggers Dialogue' esté activo.")]
    private DialogueData dialogueData;

    [Header("Climate Event")]

    [SerializeField, Tooltip("Activa si este objeto dispara un evento climático al interactuar.")]
    private bool triggersClimateEvent;

    [SerializeField, Tooltip("Evento climático asociado. Requiere que 'Triggers Climate Event' esté activo.")]
    private ClimateEventData climateEventData;

    public string Id          => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon        => icon;

    public InteractionType InteractionType => interactionType;

    public bool         TriggersDialogue    => triggersDialogue;
    public DialogueData DialogueData        => dialogueData;

    public bool             TriggersClimateEvent => triggersClimateEvent;
    public ClimateEventData ClimateEventData     => climateEventData;
}