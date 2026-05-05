using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lista de ítems permitidos para ser colocados en un WorldCell específico.
/// Si la lista está vacía, acepta cualquier ítem.
/// </summary>
[CreateAssetMenu(menuName = "Game/Inventory/Placement Filter")]
public class PlacementFilter : ScriptableObject
{
    [SerializeField, Tooltip("Ítems que pueden colocarse en esta celda. " +
                             "Vacío = acepta cualquier ítem.")]
    private List<ItemData> allowedItems = new List<ItemData>();

    /// <summary>
    /// Retorna true si el ítem está permitido en esta celda.
    /// Una lista vacía funciona como filtro abierto (acepta todo).
    /// </summary>
    public bool Allows(ItemData item)
    {
        if (item == null)                return false;
        if (allowedItems.Count == 0)     return true;

        return allowedItems.Contains(item);
    }
}