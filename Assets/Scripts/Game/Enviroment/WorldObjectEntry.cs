using System;
using UnityEngine;

/// <summary>Tipo de uso del objeto en la generación del mundo.</summary>
public enum ObjectPlacementType
{
    BoundariesOnly,
    InteriorOnly,
    Both
}

/// <summary>
/// Configuración individual de un prefab para el generador de mundo.
/// El tamaño se detecta automáticamente desde el Collider del prefab.
/// </summary>
[Serializable]
public struct WorldObjectEntry
{
    [Tooltip("Prefab del objeto. Debe tener un Collider para detectar su tamaño automáticamente.")]
    public GameObject prefab;

    [Tooltip("Define si este objeto aparece en bordes, interior o ambos.")]
    public ObjectPlacementType placementType;

    [Tooltip("Peso de aparición relativo. Mayor = más frecuente.")]
    [Range(1, 10)]
    public int weight;

    /// <summary>
    /// Detecta el tamaño XZ del objeto desde su Collider en espacio local escalado.
    /// Prioriza BoxCollider → SphereCollider → CapsuleCollider → SpriteRenderer.
    /// </summary>
    public Vector2 GetSize()
    {
        if (prefab == null) return Vector2.one;

        Vector3 scale = prefab.transform.lossyScale;

        BoxCollider box = prefab.GetComponentInChildren<BoxCollider>();
        if (box != null)
        {
            return new Vector2(
                box.size.x * Mathf.Abs(scale.x),
                box.size.z * Mathf.Abs(scale.z)
            );
        }

        SphereCollider sphere = prefab.GetComponentInChildren<SphereCollider>();
        if (sphere != null)
        {
            float diameter = sphere.radius * 2f * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            return new Vector2(diameter, diameter);
        }

        CapsuleCollider capsule = prefab.GetComponentInChildren<CapsuleCollider>();
        if (capsule != null)
        {
            float diameter = capsule.radius * 2f * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            return new Vector2(diameter, diameter);
        }

        SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            Bounds b = sr.sprite.bounds;
            return new Vector2(
                b.size.x * Mathf.Abs(scale.x),
                b.size.z * Mathf.Abs(scale.z)
            );
        }

        return Vector2.one;
    }
}

/// <summary>Configuración de un borde del mapa.</summary>
[Serializable]
public struct BoundaryGapConfig
{
    [Tooltip("Si está activo, se deja un hueco en el centro de este borde.")]
    public bool enabled;

    [Tooltip("Ancho del hueco en unidades de mundo.")]
    public float gapWidth;

    [Tooltip("Distancia desde el borde del terreno hacia adentro donde se generan los objetos.")]
    public float inwardOffset;
}