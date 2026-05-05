using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema central de progreso y persistencia.
/// Guarda: tiempo jugado, ecopuntos, progreso total, vida actual, nombre del jugador,
/// estado del inventario, WorldCells ocupadas y CollectibleItems recogidos.
///
/// PATRÓN isDuplicate:
/// Cuando el MainMenu se recarga, Unity crea un nuevo ProgressManager que
/// Awake detecta como duplicado. En lugar de destruirlo (lo que dejaría
/// los botones del menú apuntando a null), se marca como duplicado y
/// todas sus llamadas públicas se redirigen al singleton real.
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

    private string SavePath       => Path.Combine(Application.persistentDataPath, saveFileName);
    private bool   isTrackingTime;
    private bool   isDuplicate;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            isDuplicate = true;
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void OnEnable()
    {
        if (isDuplicate) return;

        SceneManager.sceneLoaded   += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        if (isDuplicate) return;

        SceneManager.sceneLoaded   -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Update()
    {
        if (isDuplicate || !isTrackingTime) return;
        Progress.tiempoJugadoSegundos += Time.deltaTime;
    }

    private void OnApplicationQuit()
    {
        if (isDuplicate) return;
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

    private void SyncToGameplay()
    {
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
            currency.SetAmount(Progress.ecopuntos);

        StartCoroutine(RestoreHealthDelayed());
        StartCoroutine(LoadWorldDelayed());
    }

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

    private IEnumerator RestoreHealthDelayed()
    {
        yield return null;

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null && Progress.vidaActual >= 0)
            health.SetVida(Progress.vidaActual);
    }

    private IEnumerator LoadWorldDelayed()
    {
        yield return null;
        LoadWorldSave();
    }

    // ── World Save / Load ────────────────────────────────────────────

    /// <summary>
    /// Guarda el estado completo del mundo: inventario, celdas ocupadas
    /// y coleccionables ya recogidos.
    /// </summary>
    public void SaveWorldState()
    {
        if (isDuplicate) { instance.SaveWorldState(); return; }

        if (itemDatabase == null)
        {
            Debug.LogWarning("[ProgressManager] ItemDatabase no asignado — no se puede guardar el mundo.");
            return;
        }

        GameSaveData data = new GameSaveData();

        if (InventorySystem.Instance != null)
            data.inventorySlots = InventorySystem.Instance.ExportSaveData();

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

        CollectibleItem[] collectibles = FindObjectsByType<CollectibleItem>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CollectibleItem col in collectibles)
        {
            if (!col.gameObject.activeSelf)
                data.collectedItemIds.Add(col.PersistId);
        }

        SaveSystem.Save(data);
    }

    private void LoadWorldSave()
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("[ProgressManager] ItemDatabase no asignado — no se puede cargar el mundo.");
            return;
        }

        GameSaveData data = SaveSystem.Load();
        if (data == null) return;

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.LoadFromSaveData(data.inventorySlots, itemDatabase);

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

        CollectibleItem[] collectibles = FindObjectsByType<CollectibleItem>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CollectibleItem col in collectibles)
        {
            if (data.collectedItemIds.Contains(col.PersistId))
                col.RestoreAsCollected();
        }
    }

    // ── API pública ──────────────────────────────────────────────────

    public void AddEcopuntos(int cantidad)
    {
        if (isDuplicate) { instance.AddEcopuntos(cantidad); return; }

        Progress.ecopuntos = Mathf.Max(0, Progress.ecopuntos + cantidad);
        OnProgressChanged?.Invoke();
    }

    public void SetProgresoTotal(float valor)
    {
        if (isDuplicate) { instance.SetProgresoTotal(valor); return; }

        Progress.progresoTotal = Mathf.Clamp(valor, 0f, 100f);
        OnProgressChanged?.Invoke();
        Save();
    }

    public void SetPlayerName(string validatedName)
    {
        if (isDuplicate) { instance.SetPlayerName(validatedName); return; }

        Progress.playerName = validatedName;
        Save();
        OnProgressChanged?.Invoke();
    }

    /// <summary>
    /// Actualiza y guarda la vida actual inmediatamente a disco.
    /// Llamado por GameProgressAutoSaver al recibir OnVidaChanged.
    /// Garantiza que una vida perdida persiste aunque el juego se cierre en ese frame.
    /// </summary>
    public void SaveVidaActual(int vida)
    {
        if (isDuplicate) { instance.SaveVidaActual(vida); return; }

        Progress.vidaActual = vida;
        Save();
    }

    /// <summary>
    /// Actualiza y guarda los ecopuntos inmediatamente a disco.
    /// Llamado por GameProgressAutoSaver al recibir OnCurrencyChanged.
    /// </summary>
    public void SaveEcopuntosActual(int cantidad)
    {
        if (isDuplicate) { instance.SaveEcopuntosActual(cantidad); return; }

        Progress.ecopuntos = cantidad;
        Save();
    }

    /// <summary>
    /// Reinicia todo el progreso: stats, nombre, inventario y estado del mundo.
    /// Si se llama desde gameplay, recarga la escena para resetear
    /// visualmente los objetos del mundo.
    /// </summary>
    public void ResetProgress()
    {
        if (isDuplicate) { instance.ResetProgress(); return; }

        Progress = new GameProgress();
        Save();
        SaveSystem.DeleteSave();

        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
        {
            currency.SetAmount(0);
            currency.ForceNotify();
        }

        if (SceneManager.GetActiveScene().name == gameplaySceneName)
        {
            isTrackingTime = false;
            StopAllCoroutines();
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            OnProgressChanged?.Invoke();
        }

        Debug.Log("[ProgressManager] Progreso reiniciado.");
    }

    // ── Progress Save / Load ─────────────────────────────────────────

    public void Save()
    {
        if (isDuplicate) { instance.Save(); return; }

        try
        {
            string json = JsonUtility.ToJson(Progress, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ProgressManager] Error al guardar: {e.Message}");
        }
    }

    public void Load()
    {
        if (isDuplicate) { instance.Load(); return; }

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
            Debug.LogError($"[ProgressManager] Error al cargar: {e.Message}");
            Progress = new GameProgress();
        }
    }

    public string GetFormattedTime()
    {
        if (isDuplicate) return instance.GetFormattedTime();

        float total   = Progress.tiempoJugadoSegundos;
        int   hours   = (int)(total / 3600);
        int   minutes = (int)(total % 3600 / 60);
        int   seconds = (int)(total % 60);
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }
}