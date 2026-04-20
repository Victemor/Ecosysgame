using UnityEngine;

/// <summary>
/// Define un evento que puede ser disparado durante el diálogo.
/// Permite conectar el sistema de diálogo con otros sistemas del juego.
/// </summary>
[System.Serializable]
public class DialogueEvent
{
    [SerializeField, Tooltip("Identificador del evento.")]
    private string eventId;

    [SerializeField, Tooltip("Tipo de evento a ejecutar.")]
    private DialogueEventType eventType;

    [SerializeField, Tooltip("Referencia opcional a datos externos (clima, objetivo, etc).")]
    private ScriptableObject payload;

    public string EventId => eventId;
    public DialogueEventType EventType => eventType;
    public ScriptableObject Payload => payload;
}