using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controlador central del sistema de diálogo.
/// Gestiona el flujo de nodos y comunica eventos a otros sistemas.
/// </summary>
public class DialogueController : MonoBehaviour
{
    private static DialogueController instance;

    /// <summary>
    /// Acceso global seguro.
    /// </summary>
    public static DialogueController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DialogueController>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("DialogueController");
                    instance = obj.AddComponent<DialogueController>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Evento al iniciar un diálogo.
    /// </summary>
    public event Action<DialogueData> OnDialogueStarted;

    /// <summary>
    /// Evento al cambiar de nodo.
    /// </summary>
    public event Action<DialogueNode> OnNodeChanged;

    /// <summary>
    /// Evento cuando hay opciones disponibles.
    /// </summary>
    public event Action<DialogueChoice[]> OnChoicesAvailable;

    /// <summary>
    /// Evento al finalizar diálogo.
    /// </summary>
    public event Action OnDialogueEnded;

    /// <summary>
    /// Evento cuando se dispara un evento de diálogo.
    /// </summary>
    public event Action<DialogueEvent> OnDialogueEventTriggered;

    private DialogueData currentDialogue;
    private DialogueNode currentNode;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    /// <summary>
    /// Inicia un diálogo.
    /// </summary>
    public void StartDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null)
            return;

        currentDialogue = dialogueData;
        currentNode = dialogueData.StartNode;

        OnDialogueStarted?.Invoke(currentDialogue);

        ProcessNode(currentNode);
    }

    /// <summary>
    /// Procesa el nodo actual.
    /// </summary>
    private void ProcessNode(DialogueNode node)
    {
        if (node == null)
        {
            EndDialogue();
            return;
        }

        currentNode = node;

        // Evento al entrar al nodo
        if (node.DialogueEvent != null)
        {
            OnDialogueEventTriggered?.Invoke(node.DialogueEvent);
        }

        OnNodeChanged?.Invoke(node);

        // Si hay opciones → UI decide
        if (node.Choices != null && node.Choices.Count > 0)
        {
            OnChoicesAvailable?.Invoke(node.Choices.ToArray());
        }
    }

    /// <summary>
    /// Continúa al siguiente nodo (flujo lineal).
    /// </summary>
    public void Continue()
    {
        if (string.IsNullOrEmpty(currentNode.NextNodeId))
        {
            EndDialogue();
            return;
        }

        DialogueNode nextNode = FindNodeById(currentNode.NextNodeId);
        ProcessNode(nextNode);
    }

    /// <summary>
    /// Selecciona una opción del jugador.
    /// </summary>
    public void SelectChoice(int index)
    {
        if (currentNode.Choices == null || index < 0 || index >= currentNode.Choices.Count)
            return;

        string nextId = currentNode.Choices[index].NextNodeId;

        DialogueNode nextNode = FindNodeById(nextId);
        ProcessNode(nextNode);
    }

    /// <summary>
    /// Finaliza el diálogo.
    /// </summary>
    private void EndDialogue()
    {
        currentDialogue = null;
        currentNode = null;

        OnDialogueEnded?.Invoke();

        GameStateController.Instance.RequestState(GameState.Gameplay);
    }

    /// <summary>
    /// Busca un nodo por ID dentro del diálogo actual.
    /// </summary>
    private DialogueNode FindNodeById(string id)
    {
        return currentDialogue.Nodes.FirstOrDefault(n => n.NodeId == id);
    }
}