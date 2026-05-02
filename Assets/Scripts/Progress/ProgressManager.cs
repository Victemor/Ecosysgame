using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de progreso y persistencia del juego.
/// Guarda y carga automáticamente usando JSON en Application.persistentDataPath.
/// Sincroniza ecopuntos con CurrencyManager al entrar y salir de escenas.
/// Persistente entre escenas (DontDestroyOnLoad).
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

    // ── Configuración ────────────────────────────────────────────────

    [Header("Settings")]

    [SerializeField, Tooltip("Nombre del archivo de guardado.")]
    private string saveFileName = "progress.json";

    [SerializeField, Tooltip("Nombre de la escena de gameplay para sincronizar ecopuntos.")]
    private string gameplaySceneName = "SampleScene";

    // ── Datos ────────────────────────────────────────────────────────

    /// <summary>Progreso actual del jugador.</summary>
    public GameProgress Progress { get; private set; } = new GameProgress();

    /// <summary>Se dispara cuando el progreso cambia.</summary>
    public event Action OnProgressChanged;

    // ── Estado interno ───────────────────────────────────────────────

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
    private bool   isTrackingTime;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
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
        SyncEcopuntosFromCurrency();
        Save();
    }

    // ── Escenas ──────────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplay = scene.name == gameplaySceneName;
        isTrackingTime  = isGameplay;

        if (isGameplay)
            SyncEcopuntosToCurrency();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == gameplaySceneName)
        {
            SyncEcopuntosFromCurrency();
            Save();
        }
    }

    /// <summary>
    /// Al entrar en gameplay, carga los ecopuntos guardados en CurrencyManager.
    /// </summary>
    private void SyncEcopuntosToCurrency()
    {
        CurrencyManager currency = FindFirstObjectByType<CurrencyManager>();

        if (currency != null)
            currency.SetAmount(Progress.ecopuntos);
    }

    /// <summary>
    /// Al salir de gameplay, guarda los ecopuntos de CurrencyManager en Progress.
    /// </summary>
    private void SyncEcopuntosFromCurrency()
    {
        CurrencyManager currency = FindFirstObjectByType<CurrencyManager>();

        if (currency != null)
        {
            Progress.ecopuntos = currency.Amount;
            OnProgressChanged?.Invoke();
        }
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>Añade ecopuntos directamente al progreso guardado.</summary>
    public void AddEcopuntos(int cantidad)
    {
        Progress.ecopuntos = Mathf.Max(0, Progress.ecopuntos + cantidad);
        OnProgressChanged?.Invoke();
    }

    /// <summary>Establece el progreso total del juego (0-100).</summary>
    public void SetProgresoTotal(float valor)
    {
        Progress.progresoTotal = Mathf.Clamp(valor, 0f, 100f);
        OnProgressChanged?.Invoke();
        Save();
    }

    /// <summary>
    /// Reinicia todo el progreso a cero y guarda.
    /// </summary>
    public void ResetProgress()
    {
        Progress = new GameProgress();
        OnProgressChanged?.Invoke();
        Save();

        Debug.Log("[ProgressManager] Progreso reiniciado.");
    }

    // ── Guardado / Carga ─────────────────────────────────────────────

    /// <summary>Guarda el progreso en disco como JSON.</summary>
    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Progress, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[ProgressManager] Guardado en: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ProgressManager] Error al guardar: {e.Message}");
        }
    }

    /// <summary>Carga el progreso desde disco. Si no existe, crea uno nuevo.</summary>
    public void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Progress = JsonUtility.FromJson<GameProgress>(json);
                Debug.Log("[ProgressManager] Progreso cargado.");
            }
            else
            {
                Progress = new GameProgress();
                Debug.Log("[ProgressManager] No hay guardado previo. Iniciando nuevo progreso.");
            }

            OnProgressChanged?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[ProgressManager] Error al cargar: {e.Message}");
            Progress = new GameProgress();
        }
    }

    // ── Formato de tiempo ────────────────────────────────────────────

    /// <summary>Devuelve el tiempo jugado formateado como HH:MM:SS.</summary>
    public string GetFormattedTime()
    {
        float total   = Progress.tiempoJugadoSegundos;
        int   hours   = (int)(total / 3600);
        int   minutes = (int)(total % 3600 / 60);
        int   seconds = (int)(total % 60);

        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }
}