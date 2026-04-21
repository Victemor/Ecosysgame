using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detecta objetos interactuables cercanos al jugador y los activa al presionar el botón de interacción.
/// Utiliza un OverlapSphere para buscar objetos automáticamente en cada frame,
/// eliminando la necesidad de asignar referencias manualmente en el Inspector.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]

    [SerializeField, Tooltip("Radio de detección de objetos interactuables.")]
    private float interactionRadius = 1.5f;

    [SerializeField, Tooltip("Layer mask para filtrar qué objetos pueden ser detectados.")]
    private LayerMask interactableLayer;

    private InputAction interactAction;
    private IInteractable currentTarget;

    /// <summary>
    /// Objeto interactuable actualmente en rango (puede ser null).
    /// Expuesto para la UI de prompts.
    /// </summary>
    public IInteractable CurrentTarget => currentTarget;

    /// <summary>
    /// Se dispara cuando entra o sale un interactuable del rango del jugador.
    /// Útil para mostrar/ocultar prompts en la UI sin acoplar sistemas.
    /// </summary>
    public event System.Action<IInteractable> OnTargetChanged;

    private void Awake()
    {
        interactAction = new InputAction(
            name: "Interact",
            type: InputActionType.Button
        );

        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonSouth");

        interactAction.performed += _ => TryInteract();
    }

    private void OnEnable()  => interactAction.Enable();
    private void OnDisable() => interactAction.Disable();

    private void Update()
    {
        DetectTarget();
    }

    /// <summary>
    /// Busca el interactuable más cercano en rango cada frame.
    /// Si el target cambia, dispara OnTargetChanged para notificar a la UI.
    /// </summary>
    private void DetectTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayer);

        IInteractable nearest = FindNearest(hits);

        if (nearest != currentTarget)
        {
            currentTarget = nearest;
            OnTargetChanged?.Invoke(currentTarget);
        }
    }

    /// <summary>
    /// Encuentra el IInteractable más cercano entre los colliders detectados.
    /// </summary>
    private IInteractable FindNearest(Collider[] hits)
    {
        IInteractable best         = null;
        float         bestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out IInteractable interactable))
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best         = interactable;
            }
        }

        return best;
    }

    /// <summary>
    /// Intenta interactuar con el target actual si existe.
    /// Solo funciona en estado Gameplay para respetar el flujo de estados del juego.
    /// </summary>
    private void TryInteract()
    {
        if (currentTarget == null)
            return;

        if (GameManager.Instance.CurrentState != GameState.Gameplay)
            return;

        currentTarget.Interact();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}