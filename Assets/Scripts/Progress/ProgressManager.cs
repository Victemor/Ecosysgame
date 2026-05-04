using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema central de progreso y persistencia.
/// Guarda: tiempo jugado, ecopuntos, progreso total, vida actual,
/// estado del inventario, WorldCells ocupadas y CollectibleItems recogidos.
/// Autosave periódico + guardado en cambio de escena y al salir.
/// </summary>
public class ProgressManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────

    private static ProgressManager instance;

    public static ProgressManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<ProgressManager>();
            return instance;
        }
    }

    // ── Campos serializados ──────────────────────────────────────────

    [Header("Settings")]

    [SerializeField, Tooltip("Nombre del archivo de guardado de progreso general.")]
    private string saveFileName = "progress.json";

    [SerializeField, Tooltip("Nombre exacto de la escena de gameplay.")]
    private string gameplaySceneName = "SampleScene";

    [SerializeField, Tooltip("Intervalo de autosave en segundos durante gameplay.")]
    private float autosaveInterval = 30f;

    [Header("Save System")]

    [SerializeField, Tooltip("Base de datos de todos los ítems del juego. " +
                             "Necesario para resolver IDs al cargar el estado del mundo.")]
    private ItemDatabase itemDatabase;

    // ── Estado ───────────────────────────────────────────────────────

    public GameProgress Progress { get; private set; } = new GameProgress();
    public event Action OnProgressChanged;

    private string SavePath    => Path.Combine(Application.persistentDataPath, saveFileName);
    private bool   isTrackingTime;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded   += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded   -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Update()
    {
        if (!isTrackingTime) return;
        Progress.tiempoJugadoSegundos += Time.deltaTime;
    }

    private void OnApplicationQuit()
    {
        SyncFromGameplay();
        Save();
    }

    // ── Escenas ──────────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplay = scene.name == gameplaySceneName;
        isTrackingTime  = isGameplay;

        if (isGameplay)
        {
            SyncToGameplay();
            StartCoroutine(AutosaveLoop());
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name != gameplaySceneName) return;

        isTrackingTime = false;
        StopAllCoroutines();
        SyncFromGameplay();
        Save();
    }

    private IEnumerator AutosaveLoop()
    {
        while (isTrackingTime)
        {
            yield return new WaitForSeconds(autosaveInterval);

            if (isTrackingTime)
            {
                SyncFromGameplay();
                Save();
                Debug.Log("[ProgressManager] Autosave ejecutado.");
            }
        }
    }

    // ── Sincronización ───────────────────────────────────────────────

    /// <summary>
    /// Al entrar en gameplay: restaura ecopuntos, vida y estado del mundo.
    /// </summary>
    private void SyncToGameplay()
    {
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
            currency.SetAmount(Progress.ecopuntos);

        StartCoroutine(RestoreHealthDelayed());
        StartCoroutine(LoadWorldDelayed());
    }

    /// <summary>
    /// Al salir de gameplay: guarda ecopuntos, vida y estado del mundo.
    /// </summary>
    private void SyncFromGameplay()
    {
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
            Progress.ecopuntos = currency.Amount;

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
            Progress.vidaActual = health.VidaActual;

        SaveWorldState();
        OnProgressChanged?.Invoke();
    }

    /// <summary>
    /// Espera un frame para que PlayerHealth exista en escena antes de restaurar.
    /// </summary>
    private IEnumerator RestoreHealthDelayed()
    {
        yield return null;

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null && Progress.vidaActual >= 0)
            health.SetVida(Progress.vidaActual);
    }

    /// <summary>
    /// Espera un frame para que todos los GameObjects de la escena estén inicializados
    /// antes de intentar restaurar el estado del mundo.
    /// </summary>
    private IEnumerator LoadWorldDelayed()
    {
        yield return null;
        LoadWorldSave();
    }

    // ── World Save / Load ────────────────────────────────────────────

    /// <summary>
    /// Guarda el estado completo del mundo: inventario, celdas ocupadas
    /// y coleccionables ya recogidos.
    /// Se llama automáticamente al salir de Gameplay y tras cada acción relevante.
    /// </summary>
    public void SaveWorldState()
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("[ProgressManager] ItemDatabase no asignado — no se puede guardar el mundo.");
            return;
        }

        GameSaveData data = new GameSaveData();

        // ── Inventario ────────────────────────────────────────────
        if (InventorySystem.Instance != null)
            data.inventorySlots = InventorySystem.Instance.ExportSaveData();

        // ── WorldCells ocupadas ───────────────────────────────────
        WorldCell[] cells = FindObjectsByType<WorldCell>(FindObjectsSortMode.None);

        foreach (WorldCell cell in cells)
        {
            if (!cell.IsOccupied) continue;

            data.occupiedCells.Add(new WorldCellSaveEntry
            {
                cellId = cell.PersistId,
                itemId = cell.PlacedItemId
            });
        }

        // ── CollectibleItems recogidos ────────────────────────────
        CollectibleItem[] collectibles = FindObjectsByType<CollectibleItem>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CollectibleItem col in collectibles)
        {
            if (!col.gameObject.activeSelf)
                data.collectedItemIds.Add(col.PersistId);
        }

        SaveSystem.Save(data);
    }

    /// <summary>
    /// Carga y aplica el estado guardado del mundo:
    /// inventario, celdas y coleccionables.
    /// </summary>
    private void LoadWorldSave()
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("[ProgressManager] ItemDatabase no asignado — no se puede cargar el mundo.");
            return;
        }

        GameSaveData data = SaveSystem.Load();
        if (data == null) return;

        // ── Inventario ────────────────────────────────────────────
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.LoadFromSaveData(data.inventorySlots, itemDatabase);

        // ── WorldCells ────────────────────────────────────────────
        WorldCell[] cells = FindObjectsByType<WorldCell>(FindObjectsSortMode.None);

        foreach (WorldCell cell in cells)
        {
            foreach (WorldCellSaveEntry entry in data.occupiedCells)
            {
                if (entry.cellId != cell.PersistId) continue;

                ItemData item = itemDatabase.GetById(entry.itemId);
                cell.RestoreFromSave(item);
                break;
            }
        }

        // ── CollectibleItems ──────────────────────────────────────
        CollectibleItem[] collectibles = FindObjectsByType<CollectibleItem>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CollectibleItem col in collectibles)
        {
            if (data.collectedItemIds.Contains(col.PersistId))
                col.RestoreAsCollected();
        }
    }

    // ── Progress API pública ─────────────────────────────────────────

    public void AddEcopuntos(int cantidad)
    {
        Progress.ecopuntos = Mathf.Max(0, Progress.ecopuntos + cantidad);
        OnProgressChanged?.Invoke();
    }

    public void SetProgresoTotal(float valor)
    {
        Progress.progresoTotal = Mathf.Clamp(valor, 0f, 100f);
        OnProgressChanged?.Invoke();
        Save();
    }

    /// <summary>
    /// Reinicia todo el progreso a cero: stats, inventario y estado del mundo.
    /// </summary>
    public void ResetProgress()
    {
        Progress = new GameProgress();
        Save();
        SaveSystem.DeleteSave();

        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
        {
            currency.SetAmount(0);
            currency.ForceNotify();
        }

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
            health.SetVida(health.VidaMax);

        OnProgressChanged?.Invoke();

        Debug.Log("[ProgressManager] Progreso reiniciado.");
    }

    // ── Progress Save / Load ─────────────────────────────────────────

    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Progress, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ProgressManager] Error al guardar progreso: {e.Message}");
        }
    }

    public void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Progress    = JsonUtility.FromJson<GameProgress>(json);
                Debug.Log("[ProgressManager] Progreso cargado.");
            }
            else
            {
                Progress = new GameProgress();
                Debug.Log("[ProgressManager] Nueva partida iniciada.");
            }

            OnProgressChanged?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[ProgressManager] Error al cargar progreso: {e.Message}");
            Progress = new GameProgress();
        }
    }

    public string GetFormattedTime()
    {
        float total   = Progress.tiempoJugadoSegundos;
        int   hours   = (int)(total / 3600);
        int   minutes = (int)(total % 3600 / 60);
        int   seconds = (int)(total % 60);
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }
}