using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registro central de todos los ItemData del juego.
/// Permite buscar un ítem por su ID string sin depender de Resources.Load.
/// Debe ser asignado al ProgressManager en el Inspector.
/// </summary>
[CreateAssetMenu(menuName = "Game/Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField, Tooltip("Lista de todos los ItemData del juego.")]
    private List<ItemData> items = new List<ItemData>();

    private Dictionary<string, ItemData> lookup;

    /// <summary>
    /// Construye el diccionario de búsqueda la primera vez que se usa.
    /// Lazy initialization para no depender del orden de carga de Unity.
    /// </summary>
    private void BuildLookup()
    {
        lookup = new Dictionary<string, ItemData>();

        foreach (ItemData item in items)
        {
            if (item == null) continue;

            if (string.IsNullOrEmpty(item.Id))
            {
                Debug.LogWarning($"[ItemDatabase] '{item.name}' no tiene ID asignado.", item);
                continue;
            }

            if (lookup.ContainsKey(item.Id))
            {
                Debug.LogWarning($"[ItemDatabase] ID duplicado: '{item.Id}'. Se ignora el segundo.", item);
                continue;
            }

            lookup[item.Id] = item;
        }
    }

    /// <summary>
    /// Busca un ItemData por su ID. Retorna null si no existe.
    /// </summary>
    public ItemData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (lookup == null) BuildLookup();

        lookup.TryGetValue(id, out ItemData result);
        return result;
    }

    /// <summary>
    /// Invalida el lookup para que se reconstruya en la próxima consulta.
    /// Llamar si se modifica la lista en runtime (solo en debug/editor).
    /// </summary>
    public void InvalidateCache() => lookup = null;
}