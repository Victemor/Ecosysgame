using UnityEngine;

/// <summary>
/// Componente que asigna un ID persistente a un GameObject en escena.
/// Al añadirse, toma automáticamente el nombre del GameObject como ID.
/// El ID puede editarse manualmente en el Inspector en cualquier momento.
/// Usado por WorldCell y CollectibleItem para identificarse en el sistema de guardado.
/// </summary>
public class PersistenceId : MonoBehaviour
{
    [SerializeField, Tooltip("ID único de este objeto en la escena. " +
                             "Se asigna automáticamente desde el nombre del GameObject. " +
                             "Puedes editarlo manualmente si necesitas un ID específico.")]
    private string persistenceId;

    /// <summary>
    /// ID de persistencia de este objeto.
    /// </summary>
    public string Id => persistenceId;

    /// <summary>
    /// Inicializa el ID desde el nombre del GameObject solo si está vacío.
    /// Así no sobreescribe un ID ya asignado manualmente.
    /// </summary>
    private void Reset()
    {
        TryAutoAssignId();
    }

    private void OnValidate()
    {
        TryAutoAssignId();
    }

    /// <summary>
    /// Asigna el nombre del GameObject como ID solo si el campo está vacío.
    /// No sobreescribe un valor ya ingresado manualmente.
    /// </summary>
    private void TryAutoAssignId()
    {
        if (string.IsNullOrEmpty(persistenceId))
            persistenceId = gameObject.name;
    }

    /// <summary>
    /// Resetea el ID al nombre actual del GameObject.
    /// Llamado desde el Custom Editor con el botón "Resetear".
    /// </summary>
    public void ResetToGameObjectName()
    {
        persistenceId = gameObject.name;
    }
}