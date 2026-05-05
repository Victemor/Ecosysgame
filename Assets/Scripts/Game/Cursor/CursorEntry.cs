using System;
using UnityEngine;

/// <summary>
/// Define el cursor asociado a un layer específico y su prioridad.
/// Mayor prioridad gana cuando el mouse está sobre múltiples objetos a la vez.
/// </summary>
[Serializable]
public class CursorEntry
{
    [SerializeField, Tooltip("Layer que activa este cursor.")]
    private LayerMask targetLayer;

    [SerializeField, Tooltip("Textura del cursor. Debe estar importada con tipo 'Cursor'.")]
    private Texture2D cursorTexture;

    [SerializeField, Tooltip("Prioridad de este cursor. Mayor número = mayor prioridad. " +
                             "Ej: WorldCell=2, Collectible=1, NPC=3.")]
    private int priority;

    public LayerMask TargetLayer   => targetLayer;
    public Texture2D CursorTexture => cursorTexture;
    public int       Priority      => priority;

    /// <summary>
    /// Retorna true si el layer del objeto pertenece a esta entrada.
    /// </summary>
    public bool MatchesLayer(int layer) => (targetLayer.value & (1 << layer)) != 0;
}