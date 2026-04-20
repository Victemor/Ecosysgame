using UnityEngine;

/// <summary>
/// Representa un objeto interactuable en la escena.
/// Actúa como puente entre el mundo y la data (InteractableData).
/// </summary>
public class Interactable : MonoBehaviour, IInteractable
{
    [Header("Data")]

    [SerializeField, Tooltip("Datos del interactuable.")]
    private InteractableData interactableData;

    /// <summary>
    /// Ejecuta la interacción delegando al controlador central.
    /// </summary>
    public void Interact()
    {
        if (interactableData == null)
        {
            Debug.LogWarning("Interactable sin data asignada.", this);
            return;
        }

        InteractionController.Instance.ProcessInteraction(interactableData);
    }

    /// <summary>
    /// Permite acceso de solo lectura a la data.
    /// </summary>
    public InteractableData Data => interactableData;
}