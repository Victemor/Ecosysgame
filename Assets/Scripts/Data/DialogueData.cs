using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contenedor principal de un diálogo.
/// Define una conversación completa estructurada en nodos.
/// </summary>
[CreateAssetMenu(menuName = "Game/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Info")]

    [SerializeField, Tooltip("Identificador único del diálogo.")]
    private string id;

    [SerializeField, Tooltip("Nombre del personaje que habla.")]
    private string speakerName;

    [SerializeField, Tooltip("Nodo inicial del diálogo.")]
    private DialogueNode startNode;

    [SerializeField, Tooltip("Lista de todos los nodos del diálogo.")]
    private List<DialogueNode> nodes;

    public string Id => id;
    public string SpeakerName => speakerName;
    public DialogueNode StartNode => startNode;
    public IReadOnlyList<DialogueNode> Nodes => nodes;
}