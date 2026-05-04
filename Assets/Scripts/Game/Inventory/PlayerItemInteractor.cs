using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Maneja las interacciones de ítem del jugador:
/// - Click izquierdo: coloca el ítem seleccionado en un WorldCell cercano.
/// - Click derecho:   recolecta un CollectibleItem cercano al inventario.
///
/// Usa OverlapSphere propio con layers separados del PlayerInteractor principal,
/// para que el diseñador controle con precisión qué detecta cada sistema.
/// </summary>
public class PlayerItemInteractor : MonoBehaviour
{
    [Header("Detección — WorldCell")]

    [SerializeField, Tooltip("Radio para detectar WorldCells cercanas.")]
    private float cellDetectionRadius = 1.5f;

    [SerializeField, Tooltip("Layer de WorldCell.")]
    private LayerMask worldCellLayer;

    [Header("Detección — CollectibleItem")]

    [SerializeField, Tooltip("Radio para detectar ítems recolectables.")]
    private float collectibleDetectionRadius = 1.5f;

    [SerializeField, Tooltip("Layer de CollectibleItem.")]
    private LayerMask collectibleLayer;

    // ── Estado ───────────────────────────────────────────────────────

    private WorldCell        nearestCell;
    private CollectibleItem  nearestCollectible;

    // ── Input ────────────────────────────────────────────────────────

    private InputAction placeAction;
    private InputAction collectAction;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        placeAction = new InputAction(name: "PlaceItem", type: InputActionType.Button);
        placeAction.AddBinding("<Mouse>/leftButton");

        collectAction = new InputAction(name: "CollectItem", type: InputActionType.Button);
        collectAction.AddBinding("<Mouse>/rightButton");

        placeAction.performed   += _ => TryPlaceItem();
        collectAction.performed += _ => TryCollectItem();
    }

    private void OnEnable()
    {
        placeAction.Enable();
        collectAction.Enable();
    }

    private void OnDisable()
    {
        placeAction.Disable();
        collectAction.Disable();
    }

    private void Update()
    {
        DetectNearestCell();
        DetectNearestCollectible();
    }

    // ── Detección ────────────────────────────────────────────────────

    /// <summary>
    /// Busca el WorldCell más cercano dentro del radio de detección.
    /// Solo considera celdas que aún no están ocupadas.
    /// </summary>
    private void DetectNearestCell()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, cellDetectionRadius, worldCellLayer);

        nearestCell = null;
        float bestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out WorldCell cell)) continue;
            if (cell.IsOccupied)                          continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearestCell  = cell;
            }
        }
    }

    /// <summary>
    /// Busca el CollectibleItem más cercano dentro del radio de detección.
    /// </summary>
    private void DetectNearestCollectible()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, collectibleDetectionRadius, collectibleLayer);

        nearestCollectible = null;
        float bestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out CollectibleItem collectible)) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < bestDistance)
            {
                bestDistance       = distance;
                nearestCollectible = collectible;
            }
        }
    }

    // ── Acciones ─────────────────────────────────────────────────────

    private void TryPlaceItem()
    {
        if (nearestCell == null) return;

        nearestCell.TryPlaceSelectedItem();
    }

    private void TryCollectItem()
    {
        if (nearestCollectible == null) return;

        nearestCollectible.TryCollect();
    }

    // ── Gizmos ───────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cellDetectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectibleDetectionRadius);
    }
}