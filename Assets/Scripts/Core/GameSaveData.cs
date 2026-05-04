using System;
using System.Collections.Generic;

/// <summary>
/// Contenedor serializable con el estado completo del juego.
/// Solo contiene tipos primitivos y strings para ser compatible con JsonUtility.
/// </summary>
[Serializable]
public class GameSaveData
{
    /// <summary>Estado de los 9 slots del inventario. Índice = posición del slot.
    /// String vacío = slot vacío. String con valor = ID del ItemData.</summary>
    public List<string> inventorySlots = new List<string>();

    /// <summary>Lista de WorldCells ocupadas con su ID y el ID del ítem colocado.</summary>
    public List<WorldCellSaveEntry> occupiedCells = new List<WorldCellSaveEntry>();

    /// <summary>Lista de CollectibleItems ya recogidos (por su PersistenceId).</summary>
    public List<string> collectedItemIds = new List<string>();
}

/// <summary>
/// Entrada serializable para una WorldCell ocupada.
/// </summary>
[Serializable]
public class WorldCellSaveEntry
{
    public string cellId;
    public string itemId;
}