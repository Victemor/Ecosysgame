using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registro central de todos los cursores del juego mapeados por layer.
/// Asignado al CursorSystem en el Inspector.
/// </summary>
[CreateAssetMenu(menuName = "Game/Cursor/Cursor Database")]
public class CursorDatabase : ScriptableObject
{
    [Header("Cursor por defecto")]

    [SerializeField, Tooltip("Cursor que se muestra cuando el mouse no está sobre " +
                             "ningún objeto con layer registrado.")]
    private Texture2D defaultCursor;

    [Header("Entradas por layer")]

    [SerializeField, Tooltip("Lista de cursores por layer. " +
                             "Cada layer debe aparecer una sola vez.")]
    private List<CursorEntry> entries = new List<CursorEntry>();

    public Texture2D             DefaultCursor => defaultCursor;
    public IReadOnlyList<CursorEntry> Entries  => entries;

    /// <summary>
    /// Construye una LayerMask combinando todos los layers registrados.
    /// Usado por CursorSystem para filtrar el RaycastAll y solo detectar
    /// objetos relevantes — sin tocar layers de física, player, etc.
    /// </summary>
    public LayerMask BuildCombinedMask()
    {
        int mask = 0;

        foreach (CursorEntry entry in entries)
            mask |= entry.TargetLayer.value;

        return mask;
    }

    /// <summary>
    /// Retorna la entrada cuyo layer coincide con el layer del objeto.
    /// Retorna null si ninguna entrada lo cubre.
    /// </summary>
    public CursorEntry GetEntryForLayer(int layer)
    {
        foreach (CursorEntry entry in entries)
        {
            if (entry.MatchesLayer(layer))
                return entry;
        }

        return null;
    }
}