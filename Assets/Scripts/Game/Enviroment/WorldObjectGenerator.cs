using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera objetos del entorno automáticamente:
/// - Bordes: llenado completo con separación mínima entre objetos.
/// - Interior: cantidad fija distribuida aleatoriamente.
/// El tamaño de cada objeto se detecta desde su Collider automáticamente.
/// </summary>
public class WorldObjectGenerator : MonoBehaviour
{
    [Header("Ground Reference")]

    [SerializeField, Tooltip("Renderer del suelo para calcular los límites del mundo.")]
    private Renderer groundRenderer;

    [Header("Objects")]

    [SerializeField, Tooltip("Lista de prefabs con su configuración individual.")]
    private List<WorldObjectEntry> objectEntries = new List<WorldObjectEntry>();

    [Header("Boundary Settings")]

    [SerializeField, Tooltip("Gap mínimo entre objetos en los bordes. Valor muy pequeño para que queden casi pegados.")]
    private float boundaryGap = 0.02f;

    [SerializeField, Tooltip("Hueco opcional en el centro del borde superior.")]
    private BoundaryGapConfig gapTop;

    [SerializeField, Tooltip("Hueco opcional en el centro del borde inferior.")]
    private BoundaryGapConfig gapBottom;

    [SerializeField, Tooltip("Hueco opcional en el centro del borde izquierdo.")]
    private BoundaryGapConfig gapLeft;

    [SerializeField, Tooltip("Hueco opcional en el centro del borde derecho.")]
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

    // Superior: se mueve hacia adentro restando en Z
    FillBorderLine(
        new Vector3(minX, y, maxZ - gapTop.inwardOffset),
        new Vector3(maxX, y, maxZ - gapTop.inwardOffset),
        Vector3.right, false, gapTop, parent);

    // Inferior: se mueve hacia adentro sumando en Z
    FillBorderLine(
        new Vector3(minX, y, minZ + gapBottom.inwardOffset),
        new Vector3(maxX, y, minZ + gapBottom.inwardOffset),
        Vector3.right, false, gapBottom, parent);

    // Izquierdo: se mueve hacia adentro sumando en X
    // Las esquinas se recortan para no solapar con top/bottom
    FillBorderLine(
        new Vector3(minX + gapLeft.inwardOffset, y, minZ + gapBottom.inwardOffset),
        new Vector3(minX + gapLeft.inwardOffset, y, maxZ - gapTop.inwardOffset),
        Vector3.forward, true, gapLeft, parent);

    // Derecho: se mueve hacia adentro restando en X
    // Las esquinas se recortan igual
    FillBorderLine(
        new Vector3(maxX - gapRight.inwardOffset, y, minZ + gapBottom.inwardOffset),
        new Vector3(maxX - gapRight.inwardOffset, y, maxZ - gapTop.inwardOffset),
        Vector3.forward, true, gapRight, parent);
}

    /// <summary>
    /// Llena completamente una línea de borde con objetos.
    /// El tamaño de cada objeto se detecta de su collider.
    /// Solo deja hueco si está explícitamente configurado.
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

        float lineLength  = Vector3.Distance(start, end);
        float centerCoord = useZ
            ? (start.z + end.z) / 2f
            : (start.x + end.x) / 2f;

        float travelled = 0f;

        while (travelled < lineLength)
        {
            WorldObjectEntry entry    = PickWeighted(candidates);
            Vector2          size     = entry.GetSize();
            float            objSize  = useZ ? size.y : size.x;

            // Si el tamaño es 0 (sin collider y sin sprite), usar 1 como fallback
            if (objSize <= 0f) objSize = 1f;

            float halfObj = objSize / 2f;

            // Centrar el objeto en su slot
            float posOnLine = travelled + halfObj;

            if (posOnLine > lineLength) break;

            Vector3 pos = start + axis * posOnLine;

            // Evaluar si cae en el hueco central
            float coord = useZ ? pos.z : pos.x;

            bool inGap = gap.enabled && Mathf.Abs(coord - centerCoord) < gap.gapWidth / 2f;

            if (!inGap)
                InstantiateEntry(entry, pos, parent);

            // Avanzar al siguiente slot: tamaño del objeto + gap mínimo
            travelled += objSize + boundaryGap;
        }
    }

    // ── Interior ─────────────────────────────────────────────────────

    private void GenerateInterior(Transform parent)
    {
        List<WorldObjectEntry> candidates = GetCandidates(ObjectPlacementType.InteriorOnly);
        if (candidates.Count == 0) return;

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

            WorldObjectEntry entry = PickWeighted(candidates);
            InstantiateEntry(entry, candidate, parent);
            placed.Add(candidate);
        }

        if (placed.Count < interiorCount)
            Debug.LogWarning($"[WorldObjectGenerator] Solo se colocaron {placed.Count}/{interiorCount} objetos interiores. Reduce minSeparation o interiorCount.", this);
    }

    private bool IsTooClose(Vector3 candidate, List<Vector3> placed)
    {
        foreach (Vector3 p in placed)
            if (Vector3.Distance(candidate, p) < minSeparation) return true;
        return false;
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

    private WorldObjectEntry PickWeighted(List<WorldObjectEntry> entries)
    {
        int total = 0;
        foreach (WorldObjectEntry e in entries) total += e.weight;

        int roll = Random.Range(0, total);
        int acc  = 0;

        foreach (WorldObjectEntry e in entries)
        {
            acc += e.weight;
            if (roll < acc) return e;
        }

        return entries[entries.Count - 1];
    }

    // ── Gizmos ───────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (groundRenderer == null) return;

        Bounds b = groundRenderer.bounds;

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
    }
}