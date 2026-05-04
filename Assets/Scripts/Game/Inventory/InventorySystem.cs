using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lógica pura del inventario. No conoce la UI ni la escena.
/// Mantiene un arreglo de 9 slots y garantiza que siempre se llenen en orden (0 → 8).
/// </summary>
public class InventorySystem : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────

    private static InventorySystem instance;

    public static InventorySystem Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<InventorySystem>();
            return instance;
        }
    }

    // ── Constantes ───────────────────────────────────────────────────

    private const int SlotCount = 9;

    // ── Estado ───────────────────────────────────────────────────────

    private readonly ItemData[] slots = new ItemData[SlotCount];

    /// <summary>Índice del slot actualmente seleccionado. -1 si ninguno está seleccionado.</summary>
    public int SelectedIndex { get; private set; } = -1;

    public int  Capacity => SlotCount;
    public bool IsFull   => FindFirstEmptySlot() == -1;

    // ── Eventos ──────────────────────────────────────────────────────

    /// <summary>
    /// Se dispara cuando un ítem se agrega al inventario.
    /// Proporciona el índice del slot y el ítem agregado.
    /// </summary>
    public event Action<int, ItemData> OnItemAdded;

    /// <summary>
    /// Se dispara cuando un ítem se elimina del inventario.
    /// Proporciona el índice del slot que quedó vacío.
    /// </summary>
    public event Action<int> OnItemRemoved;

    /// <summary>
    /// Se dispara cuando cambia el slot seleccionado.
    /// Proporciona el índice anterior (-1 si ninguno) y el nuevo (-1 si se deseleccionó).
    /// </summary>
    public event Action<int, int> OnSelectionChanged;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Intenta agregar un ítem al primer slot vacío en orden (0 → 8).
    /// Retorna true si se agregó exitosamente, false si el inventario está lleno.
    /// </summary>
    public bool TryAddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("[InventorySystem] Se intentó agregar un item null.");
            return false;
        }

        int slotIndex = FindFirstEmptySlot();

        if (slotIndex == -1)
        {
            Debug.LogWarning("[InventorySystem] Inventario lleno.");
            return false;
        }

        slots[slotIndex] = item;
        OnItemAdded?.Invoke(slotIndex, item);
        return true;
    }

    /// <summary>
    /// Selecciona un slot. Si ya estaba seleccionado, lo deselecciona (toggle).
    /// </summary>
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= SlotCount) return;

        int previous  = SelectedIndex;
        SelectedIndex = (SelectedIndex == index) ? -1 : index;

        if (previous != SelectedIndex)
            OnSelectionChanged?.Invoke(previous, SelectedIndex);
    }

    /// <summary>Retorna el ítem en el slot indicado, o null si está vacío.</summary>
    public ItemData GetItem(int index)
    {
        if (index < 0 || index >= SlotCount) return null;
        return slots[index];
    }

    /// <summary>
    /// Elimina el ítem en el slot indicado sin compactar los demás slots.
    /// El hueco queda vacío — los ítems adyacentes no se mueven.
    /// Usado por WorldCell al colocar un ítem en el mundo.
    /// </summary>
    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= SlotCount) return;
        if (slots[index] == null)            return;

        slots[index] = null;
        OnItemRemoved?.Invoke(index);
    }

    /// <summary>
    /// Exporta el estado actual del inventario como lista de IDs.
    /// String vacío representa un slot vacío.
    /// </summary>
    public List<string> ExportSaveData()
    {
        var data = new List<string>();

        for (int i = 0; i < SlotCount; i++)
            data.Add(slots[i] != null ? slots[i].Id : string.Empty);

        return data;
    }

    /// <summary>
    /// Restaura el inventario desde una lista de IDs.
    /// Requiere un ItemDatabase para resolver IDs → ItemData.
    /// </summary>
    public void LoadFromSaveData(List<string> savedSlots, ItemDatabase database)
    {
        if (savedSlots == null) return;

        for (int i = 0; i < SlotCount && i < savedSlots.Count; i++)
        {
            string id = savedSlots[i];

            if (string.IsNullOrEmpty(id))
            {
                slots[i] = null;
            }
            else
            {
                ItemData item = database.GetById(id);

                if (item == null)
                    Debug.LogWarning($"[InventorySystem] No se encontró ItemData con ID '{id}'.");

                slots[i] = item;
            }

            if (slots[i] != null)
                OnItemAdded?.Invoke(i, slots[i]);
        }
    }

    // ── Privados ─────────────────────────────────────────────────────

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] == null) return i;
        }
        return -1;
    }
}