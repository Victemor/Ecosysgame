using UnityEngine;

/// <summary>
/// Celda del mundo que acepta un ítem del inventario.
/// Reemplaza su SpriteRenderer con el ícono del ítem colocado.
/// Una vez ocupada, queda bloqueada permanentemente.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PersistenceId))]
public class WorldCell : MonoBehaviour
{
    [Header("Configuración")]

    [SerializeField, Tooltip("Filtro de ítems aceptados. Vacío = acepta cualquier ítem.")]
    private PlacementFilter placementFilter;

    // ── Estado ───────────────────────────────────────────────────────

    private SpriteRenderer spriteRenderer;
    private PersistenceId  persistenceId;
    private string         placedItemId;

    /// <summary>True si ya tiene un ítem colocado y no puede recibir más.</summary>
    public bool   IsOccupied  { get; private set; }

    /// <summary>ID del ítem colocado. Vacío si no hay ítem.</summary>
    public string PlacedItemId => placedItemId;

    /// <summary>ID de persistencia de esta celda. Tomado del PersistenceId adjunto.</summary>
    public string PersistId    => persistenceId != null ? persistenceId.Id : gameObject.name;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        persistenceId  = GetComponent<PersistenceId>();
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Intenta colocar el ítem seleccionado del inventario en esta celda.
    /// Valida que haya un ítem seleccionado, que la celda esté libre y
    /// que el ítem pase el filtro de esta celda.
    /// </summary>
    public bool TryPlaceSelectedItem()
    {
        if (IsOccupied)
        {
            Debug.Log($"[WorldCell] '{gameObject.name}' ya está ocupada.");
            return false;
        }

        int selectedIndex = InventorySystem.Instance.SelectedIndex;

        if (selectedIndex == -1)
        {
            Debug.Log("[WorldCell] No hay ningún slot seleccionado en el inventario.");
            return false;
        }

        ItemData item = InventorySystem.Instance.GetItem(selectedIndex);

        if (item == null)
        {
            Debug.Log("[WorldCell] El slot seleccionado está vacío.");
            return false;
        }

        if (placementFilter != null && !placementFilter.Allows(item))
        {
            Debug.Log($"[WorldCell] '{item.ItemName}' no está permitido en esta celda.");
            return false;
        }

        PlaceItem(item, selectedIndex);
        return true;
    }

    /// <summary>
    /// Restaura el estado ocupado de esta celda al cargar una partida.
    /// No dispara animaciones ni modifica el inventario — es una restauración silenciosa.
    /// </summary>
    public void RestoreFromSave(ItemData item)
    {
        if (item == null) return;

        spriteRenderer.sprite = item.Icon;
        placedItemId          = item.Id;
        IsOccupied            = true;
    }

    // ── Privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Coloca el ítem: reemplaza el sprite, retira el ítem del inventario,
    /// deselecciona el slot activo, bloquea la celda y guarda el estado.
    /// </summary>
    private void PlaceItem(ItemData item, int slotIndex)
    {
        spriteRenderer.sprite = item.Icon;
        IsOccupied            = true;
        placedItemId          = item.Id;

        InventorySystem.Instance.RemoveItemAt(slotIndex);
        InventorySystem.Instance.ClearSelection();

        ProgressManager.Instance.SaveWorldState();

        Debug.Log($"[WorldCell] '{item.ItemName}' colocado en '{gameObject.name}'.");
    }
}