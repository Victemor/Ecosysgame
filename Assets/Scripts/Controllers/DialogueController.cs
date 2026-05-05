using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controlador central del sistema de diálogo.
/// Gestiona el flujo de nodos y comunica cambios a otros sistemas mediante eventos.
/// No conoce el GameState ni la UI: su única responsabilidad es el flujo lógico del diálogo.
/// </summary>
public class DialogueController : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────

    private static DialogueController instance;

    /// <summary>
    /// Acceso global al controlador de diálogo.
    /// No auto-crea el objeto — si no existe en escena retorna null.
    /// Auto-crear en un getter provoca instanciación durante el cierre
    /// de escena, generando la advertencia de GameObjects no limpiados.
    /// </summary>
    public static DialogueController Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<DialogueController>();

            return instance;
        }
    }

    // ── Eventos públicos ─────────────────────────────────────────────

    /// <summary>
    /// Se dispara al iniciar un diálogo. Proporciona los metadatos del diálogo.
    /// </summary>
    public event Action<DialogueData> OnDialogueStarted;

    /// <summary>
    /// Se dispara cada vez que se avanza a un nuevo nodo.
    /// La UI debe suscribirse aquí para actualizar el texto mostrado.
    /// </summary>
    public event Action<DialogueNode> OnNodeChanged;

    /// <summary>
    /// Se dispara cuando el nodo actual contiene opciones para el jugador.
    /// </summary>
    public event Action<DialogueChoice[]> OnChoicesAvailable;

    /// <summary>
    /// Se dispara cuando el diálogo termina (no hay más nodos).
    /// GameplayController escucha esto para restaurar el estado de juego.
    /// </summary>
    public event Action OnDialogueEnded;

    /// <summary>
    /// Se dispara cuando un nodo contiene un evento embebido (clima, objetivo, etc.).
    /// </summary>
    public event Action<DialogueEvent> OnDialogueEventTriggered;

    // ── Estado interno ───────────────────────────────────────────────

    private DialogueData currentDialogue;
    private DialogueNode currentNode;

    /// <summary>
    /// Indica si hay un diálogo activo en este momento.
    /// Útil para guards en otros sistemas.
    /// </summary>
    public bool IsDialogueActive => currentDialogue != null;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            // Destroy(this) destruye solo el componente, no el GameObject completo.
            // DialogueController no es DDOL — es específico de la escena de gameplay.
            Destroy(this);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        // Limpiar la referencia estática al destruirse para que el getter
        // no intente usar una instancia destruida en el siguiente frame.
        if (instance == this)
            instance = null;
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Inicia un diálogo desde su nodo raíz.
    /// Llamado por InteractionController al detectar una interacción de tipo Talk o Inspect.
    /// </summary>
    public void StartDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("[DialogueController] Se intentó iniciar un diálogo con data null.");
            return;
        }

        if (dialogueData.StartNode == null)
        {
            Debug.LogWarning($"[DialogueController] '{dialogueData.Id}' no tiene StartNode asignado.", this);
            return;
        }

        currentDialogue = dialogueData;
        currentNode     = dialogueData.StartNode;

        OnDialogueStarted?.Invoke(currentDialogue);
        ProcessNode(currentNode);
    }

    /// <summary>
    /// Avanza al siguiente nodo en un flujo lineal (sin opciones).
    /// Si el nodo actual tiene opciones, registra un warning: usa SelectChoice() en ese caso.
    /// Si no hay siguiente nodo, termina el diálogo.
    /// Llamado por DialogueUI cuando el jugador hace clic después de que el texto terminó.
    /// </summary>
    public void ContinueDialogue()
    {
        if (currentNode == null) return;

        if (currentNode.Choices != null && currentNode.Choices.Count > 0)
        {
            Debug.LogWarning("[DialogueController] El nodo actual tiene opciones. " +
                             "Usa SelectChoice() en lugar de ContinueDialogue().");
            return;
        }

        if (!string.IsNullOrEmpty(currentNode.NextNodeId))
        {
            DialogueNode nextNode = FindNodeById(currentNode.NextNodeId);

            if (nextNode == null)
            {
                Debug.LogWarning($"[DialogueController] Nodo '{currentNode.NextNodeId}' no encontrado. " +
                                 "Se termina el diálogo.");
                EndDialogue();
                return;
            }

            ProcessNode(nextNode);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Selecciona una opción del jugador por índice y avanza al nodo correspondiente.
    /// Llamado por DialogueUI cuando el jugador pulsa un botón de opción.
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (currentNode?.Choices == null)
        {
            Debug.LogWarning("[DialogueController] SelectChoice llamado pero el nodo actual no tiene opciones.");
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= currentNode.Choices.Count)
        {
            Debug.LogWarning($"[DialogueController] Índice de opción fuera de rango: {choiceIndex}.");
            return;
        }

        string nextId = currentNode.Choices[choiceIndex].NextNodeId;

        if (string.IsNullOrEmpty(nextId))
        {
            EndDialogue();
            return;
        }

        DialogueNode nextNode = FindNodeById(nextId);

        if (nextNode == null)
        {
            Debug.LogWarning($"[DialogueController] Nodo destino '{nextId}' de la opción {choiceIndex} no encontrado.");
            EndDialogue();
            return;
        }

        ProcessNode(nextNode);
    }

    /// <summary>
    /// Fuerza el cierre del diálogo desde un sistema externo
    /// (ej. muerte del jugador, cutscene, reset de progreso).
    /// </summary>
    public void ForceEndDialogue()
    {
        if (!IsDialogueActive) return;

        EndDialogue();
    }

    // ── Lógica interna ───────────────────────────────────────────────

    /// <summary>
    /// Evalúa un nodo: dispara su evento embebido si tiene uno,
    /// notifica el cambio y expone las opciones si las hay.
    /// </summary>
    private void ProcessNode(DialogueNode node)
    {
        if (node == null)
        {
            EndDialogue();
            return;
        }

        currentNode = node;

        if (node.DialogueEvent != null && node.DialogueEvent.EventType != DialogueEventType.None)
            OnDialogueEventTriggered?.Invoke(node.DialogueEvent);

        OnNodeChanged?.Invoke(currentNode);

        if (currentNode.Choices != null && currentNode.Choices.Count > 0)
            OnChoicesAvailable?.Invoke(currentNode.Choices.ToArray());
    }

    /// <summary>
    /// Limpia el estado interno y dispara OnDialogueEnded.
    /// No cambia el GameState: esa responsabilidad es de GameplayController.
    /// </summary>
    private void EndDialogue()
    {
        currentDialogue = null;
        currentNode     = null;

        OnDialogueEnded?.Invoke();
    }

    /// <summary>
    /// Busca un nodo por su ID dentro del diálogo activo.
    /// Retorna null si no existe, con log de error para facilitar el debug de contenido.
    /// </summary>
    private DialogueNode FindNodeById(string id)
    {
        if (currentDialogue == null) return null;

        DialogueNode node = currentDialogue.Nodes.FirstOrDefault(n => n.NodeId == id);

        if (node == null)
            Debug.LogError($"[DialogueController] Nodo '{id}' no encontrado en '{currentDialogue.Id}'. " +
                           "Verifica el DialogueData asset.");

        return node;
    }

    // ── Testing ──────────────────────────────────────────────────────

    /// <summary>
    /// Construye un diálogo temporal con texto libre y lo inicia.
    /// Usado exclusivamente por DialogueDebugger en desarrollo.
    /// Encadena las líneas en nodos lineales: línea 0 → 1 → 2 → fin.
    /// </summary>
    public void StartTestDialogue(string speaker, string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[DialogueController] StartTestDialogue: no se proporcionaron líneas.");
            return;
        }

        DialogueNode[] nodes = new DialogueNode[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            nodes[i] = new DialogueNode();

            string nextId = i < lines.Length - 1 ? $"test_{i + 1}" : string.Empty;

            SetField(nodes[i], "nodeId",     $"test_{i}");
            SetField(nodes[i], "text",       lines[i]);
            SetField(nodes[i], "nextNodeId", nextId);
        }

        DialogueData data = ScriptableObject.CreateInstance<DialogueData>();

        SetField(data, "id",          "test_dialogue");
        SetField(data, "speakerName", speaker);
        SetField(data, "startNode",   nodes[0]);
        SetField(data, "nodes",       new System.Collections.Generic.List<DialogueNode>(nodes));

        StartDialogue(data);
    }

    /// <summary>
    /// Asigna un campo privado por reflexión.
    /// Solo se usa en contexto de testing — reflexión es aceptable en este scope.
    /// </summary>
    private static void SetField(object target, string fieldName, object value)
    {
        System.Reflection.FieldInfo field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field != null)
            field.SetValue(target, value);
        else
            Debug.LogWarning($"[DialogueController] Campo '{fieldName}' no encontrado en {target.GetType().Name}.");
    }
}