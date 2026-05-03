using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de progreso y persistencia.
/// Guarda: tiempo jugado, ecopuntos, progreso total y vida actual.
/// Autosave periódico + guardado en cambio de escena y al salir.
/// </summary>
public class ProgressManager : MonoBehaviour
{
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

    [Header("Settings")]

    [SerializeField, Tooltip("Nombre del archivo de guardado.")]
    private string saveFileName = "progress.json";

    [SerializeField, Tooltip("Nombre exacto de la escena de gameplay.")]
    private string gameplaySceneName = "SampleScene";

    [SerializeField, Tooltip("Intervalo de autosave en segundos durante gameplay.")]
    private float autosaveInterval = 30f;

    public GameProgress Progress { get; private set; } = new GameProgress();
    public event Action OnProgressChanged;

    private string SavePath     => Path.Combine(Application.persistentDataPath, saveFileName);
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
    /// Al entrar en gameplay: restaura ecopuntos y vida guardados.
    /// </summary>
    private void SyncToGameplay()
    {
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
            currency.SetAmount(Progress.ecopuntos);

        StartCoroutine(RestoreHealthDelayed());
    }

    /// <summary>
    /// Al salir de gameplay: guarda ecopuntos y vida actuales.
    /// </summary>
    private void SyncFromGameplay()
    {
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
            Progress.ecopuntos = currency.Amount;

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
            Progress.vidaActual = health.VidaActual;

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

    // ── API pública ──────────────────────────────────────────────────

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
    /// Reinicia todo el progreso a cero y sincroniza todos los sistemas vivos.
    /// Fuerza la actualización de UI aunque los valores ya fueran cero.
    /// </summary>
    public void ResetProgress()
    {
        Progress = new GameProgress();
        Save();

        // Resetear dinero y forzar notificación aunque ya fuera 0
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
        {
            currency.SetAmount(0);
            currency.ForceNotify();
        }

        // Resetear vida al máximo si hay PlayerHealth en escena
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
            health.SetVida(health.VidaMax);

        OnProgressChanged?.Invoke();

        Debug.Log("[ProgressManager] Progreso reiniciado.");
    }

    public void Save()
    {
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
                Debug.Log("[ProgressManager] Nuevo progreso iniciado.");
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
        float total   = Progress.tiempoJugadoSegundos;
        int   hours   = (int)(total / 3600);
        int   minutes = (int)(total % 3600 / 60);
        int   seconds = (int)(total % 60);
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }
}