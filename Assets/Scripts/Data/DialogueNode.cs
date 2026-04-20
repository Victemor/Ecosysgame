using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa una unidad de diálogo.
/// Puede contener texto, opciones o eventos.
/// </summary>
[System.Serializable]
public class DialogueNode
{
    [SerializeField, Tooltip("ID único del nodo.")]
    private string nodeId;

    [TextArea]
    [SerializeField, Tooltip("Texto que se mostrará al jugador.")]
    private string text;

    [SerializeField, Tooltip("Opciones disponibles en este nodo.")]
    private List<DialogueChoice> choices;

    [SerializeField, Tooltip("Evento que se dispara al entrar en este nodo.")]
    private DialogueEvent dialogueEvent;

    [SerializeField, Tooltip("Siguiente nodo si no hay opciones.")]
    private string nextNodeId;

    public string NodeId => nodeId;
    public string Text => text;
    public IReadOnlyList<DialogueChoice> Choices => choices;
    public DialogueEvent DialogueEvent => dialogueEvent;
    public string NextNodeId => nextNodeId;
}