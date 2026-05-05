using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Herramienta de debug para el sistema de inventario.
/// Permite agregar ítems manualmente desde el Inspector en Play Mode.
/// El Custom Editor en InventoryDebuggerEditor.cs pinta el botón.
/// </summary>
public class InventoryDebugger : MonoBehaviour
{
    [Header("Ítems de prueba")]

    [SerializeField, Tooltip("Lista de ItemData disponibles para agregar al inventario.")]
    private List<ItemData> testItems = new List<ItemData>();

    private int nextItemIndex;

    /// <summary>Nombre del próximo ítem que se agregaría. Leído por el Editor.</summary>
    public string NextItemName => (testItems != null && testItems.Count > 0)
        ? testItems[nextItemIndex % testItems.Count]?.ItemName ?? "null"
        : "— lista vacía —";

    /// <summary>
    /// Agrega el siguiente ítem de la lista al inventario.
    /// Cicla de vuelta al inicio cuando llega al final de la lista.
    /// </summary>
    public void AddNextItem()
    {
        if (testItems == null || testItems.Count == 0)
        {
            Debug.LogWarning("[InventoryDebugger] No hay ítems en la lista de prueba.");
            return;
        }

        ItemData item = testItems[nextItemIndex % testItems.Count];
        nextItemIndex++;

        if (item == null)
        {
            Debug.LogWarning("[InventoryDebugger] El ítem seleccionado es null.");
            return;
        }

        bool added = InventorySystem.Instance.TryAddItem(item);

        Debug.Log(added
            ? $"[InventoryDebugger] '{item.ItemName}' agregado."
            : "[InventoryDebugger] Inventario lleno.");
    }
}