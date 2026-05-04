using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Puente entre InventorySystem (lógica) y los InventorySlotUI (vista).
/// Reacciona a eventos del sistema para actualizar la representación visual.
/// Maneja también la selección por teclado (teclas 1–9).
/// No toca directamente los datos — solo delega a los slots y al sistema.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField, Tooltip("Los 9 slots del inventario en orden de izquierda a derecha.")]
    private InventorySlotUI[] slots;

    /// <summary>
    /// Mapeo directo tecla → índice de slot (índice 0 = tecla 1, etc.).
    /// Definido como constante de clase para no recrearlo cada frame.
    /// </summary>
    private static readonly Key[] NumberKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3,
        Key.Digit4, Key.Digit5, Key.Digit6,
        Key.Digit7, Key.Digit8, Key.Digit9
    };

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (slots == null || slots.Length != InventorySystem.Instance.Capacity)
        {
            Debug.LogError($"[InventoryUI] Se requieren exactamente " +
                           $"{InventorySystem.Instance.Capacity} slots asignados.", this);
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Initialize(i);
            slots[i].OnClicked += HandleSlotClicked;
        }
    }

    private void OnEnable()
    {
        if (InventorySystem.Instance == null) return;

        InventorySystem.Instance.OnItemAdded        += HandleItemAdded;
        InventorySystem.Instance.OnItemRemoved      += HandleItemRemoved;   // ← nuevo
        InventorySystem.Instance.OnSelectionChanged += HandleSelectionChanged;
    }

    private void OnDisable()
    {
        if (InventorySystem.Instance == null) return;

        InventorySystem.Instance.OnItemAdded        -= HandleItemAdded;
        InventorySystem.Instance.OnItemRemoved      -= HandleItemRemoved;   // ← nuevo
        InventorySystem.Instance.OnSelectionChanged -= HandleSelectionChanged;
    }

    // Agrega este handler al final de la sección de handlers:

    private void HandleItemRemoved(int slotIndex)
    {
        slots[slotIndex].ClearItem();
    }

    private void Update()
    {
        ReadNumberKeyInput();
    }

    // ── Input de teclado ─────────────────────────────────────────────

    /// <summary>
    /// Detecta si el jugador presionó una tecla del 1 al 9 y selecciona
    /// el slot correspondiente. El mapeo es directo: tecla 1 → slot 0, etc.
    /// Usa wasPressedThisFrame para responder solo al frame del press,
    /// sin acumular selecciones si se mantiene la tecla.
    /// </summary>
    private void ReadNumberKeyInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        for (int i = 0; i < NumberKeys.Length; i++)
        {
            if (kb[NumberKeys[i]].wasPressedThisFrame)
            {
                InventorySystem.Instance.SelectSlot(i);
                break;
            }
        }
    }

    // ── Handlers ─────────────────────────────────────────────────────

    private void HandleSlotClicked(int index)
    {
        InventorySystem.Instance.SelectSlot(index);
    }

    private void HandleItemAdded(int slotIndex, ItemData item)
    {
        slots[slotIndex].SetItem(item);
    }

    private void HandleSelectionChanged(int previousIndex, int newIndex)
    {
        if (previousIndex >= 0 && previousIndex < slots.Length)
            slots[previousIndex].SetSelected(false);

        if (newIndex >= 0 && newIndex < slots.Length)
            slots[newIndex].SetSelected(true);
    }
}