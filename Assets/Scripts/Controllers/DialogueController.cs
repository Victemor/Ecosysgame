using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controlador central del sistema de diálogo.
/// Gestiona el flujo de nodos y comunica eventos a otros sistemas.
/// Este sistema NO conoce qué estado del juego debe aplicarse al terminar;
/// delega esa responsabilidad a quien escuche OnDialogueEnded.
/// </summary>
public class DialogueController : MonoBehaviour
{
    private static DialogueController instance;

    /// <summary>
    /// Acceso global seguro. Crea la instancia si no existe en escena.
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
    /// Evento cuando hay opciones disponibles para el jugador.
    /// </summary>
    public event Action<DialogueChoice[]> OnChoicesAvailable;

    /// <summary>
    /// Evento al finalizar diálogo.
    /// Otros sistemas (ej. GameplayController) deben suscribirse
    /// para restaurar el estado de juego correspondiente.
    /// </summary>
    public event Action OnDialogueEnded;

    /// <summary>
    /// Evento cuando se dispara un evento embebido en un nodo de diálogo.
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
    /// Inicia un diálogo desde su nodo raíz.
    /// </summary>
    public void StartDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null)
            return;

        currentDialogue = dialogueData;
        currentNode     = dialogueData.StartNode;

        OnDialogueStarted?.Invoke(currentDialogue);

        ProcessNode(currentNode);
    }

    /// <summary>
    /// Continúa al siguiente nodo en un flujo lineal (sin opciones).
    /// </summary>
    public void Continue()
    {
        if (currentNode == null)
            return;

        if (string.IsNullOrEmpty(currentNode.NextNodeId))
        {
            EndDialogue();
            return;
        }

        DialogueNode nextNode = FindNodeById(currentNode.NextNodeId);
        ProcessNode(nextNode);
    }

    /// <summary>
    /// Selecciona una opción del jugador y avanza al nodo correspondiente.
    /// </summary>
    public void SelectChoice(int index)
    {
        if (currentNode?.Choices == null || index < 0 || index >= currentNode.Choices.Count)
            return;

        string nextId = currentNode.Choices[index].NextNodeId;

        DialogueNode nextNode = FindNodeById(nextId);
        ProcessNode(nextNode);
    }

    /// <summary>
    /// Evalúa el nodo recibido: dispara eventos embebidos, notifica cambio
    /// y expone opciones si las hay.
    /// </summary>
    private void ProcessNode(DialogueNode node)
    {
        if (node == null)
        {
            EndDialogue();
            return;
        }

        currentNode = node;

        if (node.DialogueEvent != null)
            OnDialogueEventTriggered?.Invoke(node.DialogueEvent);

        OnNodeChanged?.Invoke(node);

        if (node.Choices != null && node.Choices.Count > 0)
            OnChoicesAvailable?.Invoke(node.Choices.ToArray());
    }

    /// <summary>
    /// Finaliza el diálogo limpiando el estado interno.
    /// NO cambia el GameState directamente; dispara OnDialogueEnded
    /// para que los suscriptores (ej. GameplayController) reaccionen.
    /// </summary>
    private void EndDialogue()
    {
        currentDialogue = null;
        currentNode     = null;

        OnDialogueEnded?.Invoke();
    }

    /// <summary>
    /// Busca un nodo por su ID dentro del diálogo activo.
    /// </summary>
    private DialogueNode FindNodeById(string id)
    {
        return currentDialogue?.Nodes.FirstOrDefault(n => n.NodeId == id);
    }
}