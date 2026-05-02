using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera objetos del entorno automáticamente:
/// - Bordes: llenado completo con separación mínima, offset configurable por lado.
/// - Interior: cantidad fija distribuida aleatoriamente.
/// Usa shuffle bag para garantizar distribución uniforme de todos los tipos de objeto.
/// </summary>
public class WorldObjectGenerator : MonoBehaviour
{
    [Header("Ground Reference")]

    [SerializeField, Tooltip("Renderer del suelo. Define los límites del mundo.")]
    private Renderer groundRenderer;

    [Header("Objects")]

    [SerializeField, Tooltip("Lista de prefabs con su configuración individual.")]
    private List<WorldObjectEntry> objectEntries = new List<WorldObjectEntry>();

    [Header("Boundary Settings")]

    [SerializeField, Tooltip("Gap mínimo entre objetos en los bordes.")]
    private float boundaryGap = 0.02f;

    [SerializeField, Tooltip("Borde superior.")]
    private BoundaryGapConfig gapTop;

    [SerializeField, Tooltip("Borde inferior.")]
    private BoundaryGapConfig gapBottom;

    [SerializeField, Tooltip("Borde izquierdo.")]
    private BoundaryGapConfig gapLeft;

    [SerializeField, Tooltip("Borde derecho.")]
    private BoundaryGapConfig gapRight;

    [Header("Interior Settings")]

    [SerializeField, Tooltip("Cantidad de objetos a generar en el interior del mapa.")]
    private int interiorCount = 15;

    [SerializeField, Tooltip("Separación mínima entre objetos del interior.")]
    private float minSeparation = 1.5f;

    [SerializeField, Tooltip("Margen desde los bordes para el área interior válida.")]
    private float interiorMargin = 2f;

    [Header("Generation")]

    [SerializeField, Tooltip("Semilla aleatoria. -1 = diferente cada vez.")]
    private int randomSeed = 42;

    [SerializeField, Tooltip("Transform padre donde se instancian los objetos generados.")]
    private Transform container;

    // ── Bounds ───────────────────────────────────────────────────────

    private Bounds worldBounds;

    // ── API pública ──────────────────────────────────────────────────

    public void Generate()
    {
        if (groundRenderer == null)
        {
            Debug.LogError("[WorldObjectGenerator] Asigna un Ground Renderer.", this);
            return;
        }

        if (objectEntries == null || objectEntries.Count == 0)
        {
            Debug.LogWarning("[WorldObjectGenerator] No hay objetos en la lista.", this);
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

    public void ClearGenerated()
    {
        Transform parent = container != null ? container : transform;

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

        // Superior
        FillBorderLine(
            new Vector3(minX, y, maxZ - gapTop.inwardOffset),
            new Vector3(maxX, y, maxZ - gapTop.inwardOffset),
            Vector3.right, false, gapTop, parent);

        // Inferior
        FillBorderLine(
            new Vector3(minX, y, minZ + gapBottom.inwardOffset),
            new Vector3(maxX, y, minZ + gapBottom.inwardOffset),
            Vector3.right, false, gapBottom, parent);

        // Izquierdo — esquinas recortadas para no solapar con top/bottom
        FillBorderLine(
            new Vector3(minX + gapLeft.inwardOffset, y, minZ + gapBottom.inwardOffset),
            new Vector3(minX + gapLeft.inwardOffset, y, maxZ - gapTop.inwardOffset),
            Vector3.forward, true, gapLeft, parent);

        // Derecho — esquinas recortadas
        FillBorderLine(
            new Vector3(maxX - gapRight.inwardOffset, y, minZ + gapBottom.inwardOffset),
            new Vector3(maxX - gapRight.inwardOffset, y, maxZ - gapTop.inwardOffset),
            Vector3.forward, true, gapRight, parent);
    }

    /// <summary>
    /// Llena completamente una línea de borde con objetos usando shuffle bag.
    /// Solo deja hueco en el centro si está explícitamente configurado.
    /// </summary>
    private void FillBorderLine(
        Vector3           start,
        Vector3           end,
        Vector3           axis,
        bool              useZ,
        BoundaryGapConfig gap,
        Transform         parent)
    {
        List<WorldObjectEntry> candidates = GetCandidates(ObjectPlacementType.BoundariesOnly);
        if (candidates.Count == 0) return;

        List<WorldObjectEntry> bag      = BuildShuffleBag(candidates);
        int                    bagIndex = 0;

        float lineLength  = Vector3.Distance(start, end);
        float centerCoord = useZ
            ? (start.z + end.z) / 2f
            : (start.x + end.x) / 2f;

        float travelled = 0f;

        while (travelled < lineLength)
        {
            WorldObjectEntry entry   = DrawFromBag(bag, candidates, ref bagIndex);
            Vector2          size    = entry.GetSize();
            float            objSize = useZ ? size.y : size.x;

            if (objSize <= 0f) objSize = 1f;

            float halfObj   = objSize / 2f;
            float posOnLine = travelled + halfObj;

            if (posOnLine > lineLength) break;

            Vector3 pos   = start + axis * posOnLine;
            float   coord = useZ ? pos.z : pos.x;

            bool inGap = gap.enabled && Mathf.Abs(coord - centerCoord) < gap.gapWidth / 2f;

            if (!inGap)
                InstantiateEntry(entry, pos, parent);

            travelled += objSize + boundaryGap;
        }
    }

    // ── Interior ─────────────────────────────────────────────────────

    private void GenerateInterior(Transform parent)
    {
        List<WorldObjectEntry> candidates = GetCandidates(ObjectPlacementType.InteriorOnly);
        if (candidates.Count == 0) return;

        List<WorldObjectEntry> bag      = BuildShuffleBag(candidates);
        int                    bagIndex = 0;

        float minX = worldBounds.min.x + interiorMargin;
        float maxX = worldBounds.max.x - interiorMargin;
        float minZ = worldBounds.min.z + interiorMargin;
        float maxZ = worldBounds.max.z - interiorMargin;
        float y    = worldBounds.center.y;

        List<Vector3> placed      = new List<Vector3>();
        int           attempts    = 0;
        int           maxAttempts = interiorCount * 15;

        while (placed.Count < interiorCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 candidate = new Vector3(
                Random.Range(minX, maxX),
                y,
                Random.Range(minZ, maxZ)
            );

            if (IsTooClose(candidate, placed)) continue;

            WorldObjectEntry entry = DrawFromBag(bag, candidates, ref bagIndex);
            InstantiateEntry(entry, candidate, parent);
            placed.Add(candidate);
        }

        if (placed.Count < interiorCount)
            Debug.LogWarning(
                $"[WorldObjectGenerator] Solo se colocaron {placed.Count}/{interiorCount} objetos. " +
                "Reduce minSeparation o interiorCount.", this);
    }

    private bool IsTooClose(Vector3 candidate, List<Vector3> placed)
    {
        foreach (Vector3 p in placed)
            if (Vector3.Distance(candidate, p) < minSeparation) return true;
        return false;
    }

    // ── Shuffle Bag ───────────────────────────────────────────────────

    /// <summary>
    /// Construye una bolsa con todos los candidatos repetidos según su peso,
    /// la baraja y la devuelve. Garantiza distribución uniforme de todos los tipos.
    /// </summary>
    private List<WorldObjectEntry> BuildShuffleBag(List<WorldObjectEntry> candidates)
    {
        List<WorldObjectEntry> bag = new List<WorldObjectEntry>();

        foreach (WorldObjectEntry e in candidates)
        {
            int times = Mathf.Max(1, e.weight);
            for (int i = 0; i < times; i++)
                bag.Add(e);
        }

        Shuffle(bag);
        return bag;
    }

    private void Shuffle(List<WorldObjectEntry> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Saca el siguiente elemento de la bolsa.
    /// Cuando se vacía, la rellena y baraja automáticamente.
    /// </summary>
    private WorldObjectEntry DrawFromBag(
        List<WorldObjectEntry> bag,
        List<WorldObjectEntry> candidates,
        ref int                bagIndex)
    {
        if (bagIndex >= bag.Count)
        {
            bag.Clear();
            bag.AddRange(BuildShuffleBag(candidates));
            bagIndex = 0;
        }

        return bag[bagIndex++];
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private void InstantiateEntry(WorldObjectEntry entry, Vector3 position, Transform parent)
    {
        if (entry.prefab == null) return;

#if UNITY_EDITOR
        GameObject obj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(entry.prefab, parent);
        obj.transform.SetPositionAndRotation(position, Quaternion.identity);
        UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "Generate World Object");
#else
        Instantiate(entry.prefab, position, Quaternion.identity, parent);
#endif
    }

    private List<WorldObjectEntry> GetCandidates(ObjectPlacementType type)
    {
        var result = new List<WorldObjectEntry>();
        foreach (WorldObjectEntry e in objectEntries)
        {
            if (e.prefab == null) continue;
            if (e.placementType == type || e.placementType == ObjectPlacementType.Both)
                result.Add(e);
        }
        return result;
    }

    // ── Gizmos ───────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (groundRenderer == null) return;

        Bounds b = groundRenderer.bounds;
        float  y = b.center.y;

        // Amarillo — límites totales del mundo
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
        Gizmos.DrawWireCube(b.center, new Vector3(b.size.x, 0.1f, b.size.z));

        // Verde — área interior válida
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireCube(b.center, new Vector3(
            b.size.x - interiorMargin * 2f,
            0.1f,
            b.size.z - interiorMargin * 2f
        ));

        // Cyan — líneas de generación con offsets aplicados
        Gizmos.color = new Color(0f, 0.9f, 1f, 0.6f);

        // Top
        Gizmos.DrawLine(
            new Vector3(b.min.x, y, b.max.z - gapTop.inwardOffset),
            new Vector3(b.max.x, y, b.max.z - gapTop.inwardOffset));

        // Bottom
        Gizmos.DrawLine(
            new Vector3(b.min.x, y, b.min.z + gapBottom.inwardOffset),
            new Vector3(b.max.x, y, b.min.z + gapBottom.inwardOffset));

        // Left
        Gizmos.DrawLine(
            new Vector3(b.min.x + gapLeft.inwardOffset, y, b.min.z + gapBottom.inwardOffset),
            new Vector3(b.min.x + gapLeft.inwardOffset, y, b.max.z - gapTop.inwardOffset));

        // Right
        Gizmos.DrawLine(
            new Vector3(b.max.x - gapRight.inwardOffset, y, b.min.z + gapBottom.inwardOffset),
            new Vector3(b.max.x - gapRight.inwardOffset, y, b.max.z - gapTop.inwardOffset));
    }
}