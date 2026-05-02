using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera objetos del entorno automáticamente:
/// - En los 4 bordes del suelo (con huecos opcionales en cada lado).
/// - Distribuidos aleatoriamente en el interior del área.
/// Toda la configuración es editable desde el Inspector.
/// La generación puede ejecutarse en Edit Mode desde el Custom Editor.
/// </summary>
public class WorldObjectGenerator : MonoBehaviour
{
    [Header("Ground Reference")]

    [SerializeField, Tooltip("Renderer del suelo. Se usa para calcular los límites del mundo.")]
    private Renderer groundRenderer;

    [Header("Objects")]

    [SerializeField, Tooltip("Lista de prefabs con su configuración individual.")]
    private List<WorldObjectEntry> objectEntries = new List<WorldObjectEntry>();

    [Header("Boundary Settings")]

    [SerializeField, Tooltip("Separación adicional entre objetos en los bordes.")]
    private float boundarySpacing = 0.2f;

    [SerializeField, Tooltip("Configuración del hueco en el borde superior.")]
    private BoundaryGapConfig gapTop;

    [SerializeField, Tooltip("Configuración del hueco en el borde inferior.")]
    private BoundaryGapConfig gapBottom;

    [SerializeField, Tooltip("Configuración del hueco en el borde izquierdo.")]
    private BoundaryGapConfig gapLeft;

    [SerializeField, Tooltip("Configuración del hueco en el borde derecho.")]
    private BoundaryGapConfig gapRight;

    [Header("Interior Settings")]

    [SerializeField, Tooltip("Cantidad de objetos a generar en el interior del mapa.")]
    private int interiorCount = 20;

    [SerializeField, Tooltip("Separación mínima entre objetos del interior para evitar solapamientos.")]
    private float minSeparation = 1.5f;

    [SerializeField, Tooltip("Margen desde los bordes del suelo para el área interior válida.")]
    private float interiorMargin = 2f;

    [SerializeField, Tooltip("Semilla aleatoria. -1 = aleatoria cada vez.")]
    private int randomSeed = -1;

    [Header("Container")]

    [SerializeField, Tooltip("Transform padre donde se instancian los objetos. Si es null, se usa este GameObject.")]
    private Transform container;

    // ── Bounds calculados ────────────────────────────────────────────

    private Bounds worldBounds;

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Genera todos los objetos del mundo (bordes + interior).
    /// Si ya existen objetos generados, los elimina primero.
    /// </summary>
    public void Generate()
    {
        if (groundRenderer == null)
        {
            Debug.LogError("[WorldObjectGenerator] No hay un Renderer de suelo asignado.", this);
            return;
        }

        if (objectEntries == null || objectEntries.Count == 0)
        {
            Debug.LogWarning("[WorldObjectGenerator] No hay objetos configurados.", this);
            return;
        }

        ClearGenerated();

        if (randomSeed >= 0)
            Random.InitState(randomSeed);

        worldBounds = groundRenderer.bounds;

        Transform parent = container != null ? container : transform;

        GenerateBoundaries(parent);
        GenerateInterior(parent);
    }

    /// <summary>
    /// Elimina todos los objetos hijos del container generados previamente.
    /// </summary>
    public void ClearGenerated()
    {
        Transform parent = container != null ? container : transform;

        // Iterar en reversa para destruir correctamente en Edit Mode y Play Mode
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;

#if UNITY_EDITOR
            UnityEditor.Undo.DestroyObjectImmediate(child);
#else
            Destroy(child);
#endif
        }
    }

    // ── Bordes ───────────────────────────────────────────────────────

    private void GenerateBoundaries(Transform parent)
    {
        float minX = worldBounds.min.x;
        float maxX = worldBounds.max.x;
        float minZ = worldBounds.min.z;
        float maxZ = worldBounds.max.z;
        float y    = worldBounds.center.y;

        // Top: Z = maxZ, recorre en X
        GenerateBorderLine(
            start:    new Vector3(minX, y, maxZ),
            end:      new Vector3(maxX, y, maxZ),
            axis:     Vector3.right,
            gap:      gapTop,
            parent:   parent,
            useZ:     false
        );

        // Bottom: Z = minZ, recorre en X
        GenerateBorderLine(
            start:    new Vector3(minX, y, minZ),
            end:      new Vector3(maxX, y, minZ),
            axis:     Vector3.right,
            gap:      gapBottom,
            parent:   parent,
            useZ:     false
        );

        // Left: X = minX, recorre en Z
        GenerateBorderLine(
            start:    new Vector3(minX, y, minZ),
            end:      new Vector3(minX, y, maxZ),
            axis:     Vector3.forward,
            gap:      gapLeft,
            parent:   parent,
            useZ:     true
        );

        // Right: X = maxX, recorre en Z
        GenerateBorderLine(
            start:    new Vector3(maxX, y, minZ),
            end:      new Vector3(maxX, y, maxZ),
            axis:     Vector3.forward,
            gap:      gapRight,
            parent:   parent,
            useZ:     true
        );
    }

    /// <summary>
    /// Recorre una línea del borde e instancia objetos con separación uniforme,
    /// respetando el hueco central si está configurado.
    /// </summary>
    private void GenerateBorderLine(
        Vector3           start,
        Vector3           end,
        Vector3           axis,
        BoundaryGapConfig gap,
        Transform         parent,
        bool              useZ)
    {
        List<WorldObjectEntry> candidates = GetEntriesForPlacement(ObjectPlacementType.BoundariesOnly);
        if (candidates.Count == 0) return;

        float lineLength  = Vector3.Distance(start, end);
        float centerCoord = useZ
            ? (start.z + end.z) / 2f
            : (start.x + end.x) / 2f;

        WorldObjectEntry entry     = PickWeighted(candidates);
        float            stepSize  = (useZ ? entry.size.y : entry.size.x) + boundarySpacing;
        float            travelled = stepSize / 2f;

        while (travelled < lineLength)
        {
            Vector3 pos = start + axis * travelled;

            // Evaluar si esta posición cae dentro del hueco central
            float posCoord = useZ ? pos.z : pos.x;

            if (gap.enabled && Mathf.Abs(posCoord - centerCoord) < gap.gapWidth / 2f)
            {
                travelled += stepSize;
                continue;
            }

            InstantiateEntry(PickWeighted(candidates), pos, parent);
            travelled += stepSize;
        }
    }

    // ── Interior ─────────────────────────────────────────────────────

    /// <summary>
    /// Distribuye objetos aleatoriamente dentro del área interior del mapa.
    /// Aplica separación mínima para evitar solapamientos obvios.
    /// </summary>
    private void GenerateInterior(Transform parent)
    {
        List<WorldObjectEntry> candidates = GetEntriesForPlacement(ObjectPlacementType.InteriorOnly);
        if (candidates.Count == 0) return;

        float minX = worldBounds.min.x + interiorMargin;
        float maxX = worldBounds.max.x - interiorMargin;
        float minZ = worldBounds.min.z + interiorMargin;
        float maxZ = worldBounds.max.z - interiorMargin;
        float y    = worldBounds.center.y;

        List<Vector3> placed   = new List<Vector3>();
        int           attempts = 0;
        int           maxAttempts = interiorCount * 10;

        while (placed.Count < interiorCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 candidate = new Vector3(
                Random.Range(minX, maxX),
                y,
                Random.Range(minZ, maxZ)
            );

            if (IsTooClose(candidate, placed))
                continue;

            WorldObjectEntry entry = PickWeighted(candidates);
            InstantiateEntry(entry, candidate, parent);
            placed.Add(candidate);
        }

        if (placed.Count < interiorCount)
            Debug.LogWarning($"[WorldObjectGenerator] Solo se colocaron {placed.Count}/{interiorCount} objetos interiores. Reduce minSeparation o interiorCount.");
    }

    private bool IsTooClose(Vector3 candidate, List<Vector3> placed)
    {
        foreach (Vector3 p in placed)
        {
            if (Vector3.Distance(candidate, p) < minSeparation)
                return true;
        }
        return false;
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private void InstantiateEntry(WorldObjectEntry entry, Vector3 position, Transform parent)
    {
        if (entry.prefab == null) return;

#if UNITY_EDITOR
        GameObject obj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(entry.prefab, parent);
        obj.transform.position = position;
        UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "Generate World Object");
#else
        Instantiate(entry.prefab, position, Quaternion.identity, parent);
#endif
    }

    /// <summary>
    /// Devuelve los objetos que aplican a un tipo de placement dado.
    /// Los de tipo Both se incluyen en ambos contextos.
    /// </summary>
    private List<WorldObjectEntry> GetEntriesForPlacement(ObjectPlacementType type)
    {
        List<WorldObjectEntry> result = new List<WorldObjectEntry>();

        foreach (WorldObjectEntry entry in objectEntries)
        {
            if (entry.prefab == null) continue;

            if (entry.placementType == type || entry.placementType == ObjectPlacementType.Both)
                result.Add(entry);
        }

        return result;
    }

    /// <summary>
    /// Selecciona un objeto de la lista respetando el peso de cada uno.
    /// </summary>
    private WorldObjectEntry PickWeighted(List<WorldObjectEntry> entries)
    {
        int totalWeight = 0;
        foreach (WorldObjectEntry e in entries)
            totalWeight += e.weight;

        int roll = Random.Range(0, totalWeight);
        int accumulated = 0;

        foreach (WorldObjectEntry e in entries)
        {
            accumulated += e.weight;
            if (roll < accumulated)
                return e;
        }

        return entries[entries.Count - 1];
    }

    // ── Gizmos ───────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (groundRenderer == null) return;

        Bounds b = groundRenderer.bounds;

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.4f);
        Gizmos.DrawWireCube(b.center, b.size);

        // Área interior válida
        float margin = interiorMargin;
        Vector3 innerSize = new Vector3(b.size.x - margin * 2f, 0.1f, b.size.z - margin * 2f);
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireCube(b.center, innerSize);
    }
}