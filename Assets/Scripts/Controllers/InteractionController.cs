using UnityEngine;

/// <summary>
/// Controlador central de interacciones.
/// Recibe datos de un interactuable y delega la lógica al sistema correcto
/// (diálogo, clima, recolección, etc.) sin conocer los detalles de cada uno.
///
/// IMPORTANTE: Este componente debe estar presente en la escena manualmente.
/// A diferencia de GameManager o DialogueController, InteractionController
/// no se auto-crea porque depende de contexto de escena activa.
/// Si no está en escena, Interactable.Interact() lanzará un error claro.
/// </summary>
public class InteractionController : MonoBehaviour
{
    private static InteractionController instance;

    /// <summary>
    /// Acceso global seguro al controlador.
    /// Retorna null si no existe en escena y registra un error descriptivo.
    /// No se auto-crea intencionalmente: este sistema requiere configuración de escena.
    /// </summary>
    public static InteractionController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InteractionController>();

                if (instance == null)
                {
                    Debug.LogError(
                        "[InteractionController] No se encontró ninguna instancia en la escena. " +
                        "Agrega el componente InteractionController a un GameObject en la escena activa."
                    );
                }
            }

            return instance;
        }
    }

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
    /// Punto de entrada principal. Evalúa el tipo de interacción y delega al sistema correcto.
    /// </summary>
    public void ProcessInteraction(InteractableData data)
    {
        if (data == null)
            return;

        switch (data.InteractionType)
        {
            case InteractionType.Inspect:
                HandleInspect(data);
                break;

            case InteractionType.Collect:
                HandleCollect(data);
                break;

            case InteractionType.Talk:
                HandleDialogue(data);
                break;

            case InteractionType.TriggerEvent:
                HandleEvent(data);
                break;

            default:
                Debug.LogWarning($"[InteractionController] Tipo de interacción no manejado: {data.InteractionType}", this);
                break;
        }
    }

    /// <summary>
    /// Inspeccionar: puede derivar en diálogo si el objeto tiene uno asignado.
    /// Si no tiene diálogo, la UI reacciona a OnTargetChanged en PlayerInteractor.
    /// </summary>
    private void HandleInspect(InteractableData data)
    {
        if (data.TriggersDialogue && data.DialogueData != null)
        {
            StartDialogue(data.DialogueData);
            return;
        }

        Debug.Log($"[Inspect] {data.DisplayName}: {data.Description}");
    }

    /// <summary>
    /// Recolectar: placeholder hasta que exista un InventorySystem.
    /// </summary>
    private void HandleCollect(InteractableData data)
    {
        // TODO: conectar con InventorySystem cuando esté disponible
        Debug.Log($"[Collect] {data.DisplayName}");
    }

    /// <summary>
    /// Iniciar conversación directa con un NPC.
    /// </summary>
    private void HandleDialogue(InteractableData data)
    {
        if (data.DialogueData == null)
        {
            Debug.LogWarning($"[Dialogue] '{data.DisplayName}' no tiene DialogueData asignada.", this);
            return;
        }

        StartDialogue(data.DialogueData);
    }

    /// <summary>
    /// Disparar evento climático desde una interacción (p.ej. tocar un artefacto).
    /// </summary>
    private void HandleEvent(InteractableData data)
    {
        if (!data.TriggersClimateEvent || data.ClimateEventData == null)
        {
            Debug.LogWarning($"[Event] '{data.DisplayName}' no tiene ClimateEventData asignada.", this);
            return;
        }

        ClimateController.Instance.StartClimateEvent(data.ClimateEventData);
    }

    /// <summary>
    /// Centraliza el inicio de un diálogo: cambia estado y notifica al DialogueController.
    /// Separado para evitar duplicación entre HandleInspect y HandleDialogue.
    /// </summary>
    private void StartDialogue(DialogueData dialogueData)
    {
        GameStateController.Instance.RequestState(GameState.Dialogue);
        DialogueController.Instance.StartDialogue(dialogueData);
    }
}