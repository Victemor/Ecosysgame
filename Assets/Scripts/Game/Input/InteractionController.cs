using UnityEngine;

/// <summary>
/// Controlador central de interacciones.
/// Decide qué ocurre cuando el jugador interactúa con un objeto.
/// </summary>
public class InteractionController : MonoBehaviour
{
    private static InteractionController instance;

    /// <summary>
    /// Acceso global seguro al controlador.
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
                    GameObject obj = new GameObject("InteractionController");
                    instance = obj.AddComponent<InteractionController>();
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
    /// Procesa una interacción basada en los datos del objeto.
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
        }
    }

    /// <summary>
    /// Maneja interacciones de inspección.
    /// </summary>
    private void HandleInspect(InteractableData data)
    {
        if (data.TriggersDialogue && data.DialogueData != null)
        {
            GameStateController.Instance.RequestState(GameState.Dialogue);
            // Aquí luego conectamos con DialogueController
        }
    }

    /// <summary>
    /// Maneja interacciones de recolección.
    /// </summary>
    private void HandleCollect(InteractableData data)
    {
        Debug.Log($"Recolectado: {data.DisplayName}");
        // Aquí iría inventario / progreso
    }

    /// <summary>
    /// Maneja interacciones de diálogo.
    /// </summary>
    private void HandleDialogue(InteractableData data)
    {
        if (data.DialogueData == null)
            return;

        GameStateController.Instance.RequestState(GameState.Dialogue);

        // Aquí luego conectamos DialogueController
    }

    /// <summary>
    /// Maneja activación de eventos (clima, etc).
    /// </summary>
    private void HandleEvent(InteractableData data)
    {
        if (data.TriggersClimateEvent && data.ClimateEventData != null)
        {
            // Aquí luego conectamos ClimateController
            Debug.Log("Evento climático activado");
        }
    }
}