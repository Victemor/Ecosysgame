using System;
using UnityEngine;

/// <summary>
/// Tipo de uso del objeto en la generación del mundo.
/// </summary>
public enum ObjectPlacementType
{
    BoundariesOnly, // Solo en los bordes del mapa
    InteriorOnly,   // Solo en el interior del mapa
    Both            // En ambos contextos
}

/// <summary>
/// Configuración individual de un prefab para el generador de mundo.
/// </summary>
[Serializable]
public struct WorldObjectEntry
{
    [Tooltip("Prefab del objeto a instanciar.")]
    public GameObject prefab;

    [Tooltip("Tamaño del objeto en X y Z. Se usa para calcular separación y evitar solapamientos.")]
    public Vector2 size;

    [Tooltip("Define si este objeto aparece en bordes, interior o ambos.")]
    public ObjectPlacementType placementType;

    [Tooltip("Peso de aparición relativo a otros objetos de la misma lista. Mayor = más frecuente.")]
    [Range(1, 10)]
    public int weight;
}

/// <summary>
/// Configuración de espacio vacío en el centro de un borde del mapa.
/// </summary>
[Serializable]
public struct BoundaryGapConfig
{
    [Tooltip("Si está activo, se deja un hueco en el centro de este borde.")]
    public bool enabled;

    [Tooltip("Ancho del hueco en unidades de mundo.")]
    public float gapWidth;
}