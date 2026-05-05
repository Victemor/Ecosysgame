using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema global de cursores. Lanza un RaycastAll desde la cámara hacia
/// el mouse cada frame y aplica el cursor del layer con mayor prioridad.
///
/// Solo actúa en GameState.Gameplay — en Dialogue y Paused el cursor
/// queda congelado en el último estado activo.
///
/// La LayerMask del raycast se construye automáticamente desde CursorDatabase,
/// así el designer solo mantiene una lista, no dos.
/// </summary>
public class CursorSystem : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────

    private static CursorSystem instance;

    public static CursorSystem Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<CursorSystem>();
            return instance;
        }
    }

    // ── Campos serializados ──────────────────────────────────────────

    [SerializeField, Tooltip("Base de datos de cursores por layer.")]
    private CursorDatabase database;

    [SerializeField, Tooltip("Distancia máxima del raycast desde la cámara.")]
    private float raycastDistance = 100f;

    // ── Estado ───────────────────────────────────────────────────────

    private Camera       mainCamera;
    private LayerMask    detectableLayers;
    private Texture2D    currentCursor;
    private bool         isActive;

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

        if (database != null)
            detectableLayers = database.BuildCombinedMask();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded           += OnSceneLoaded;
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded           -= OnSceneLoaded;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        if (!isActive || database == null || mainCamera == null) return;

        UpdateCursor();
    }

    // ── Handlers ────────────────────────────────────────────────────

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
                               LoadSceneMode mode)
    {
        // La cámara es un objeto de escena — hay que refrescarla en cada carga.
        mainCamera = Camera.main;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        isActive = newState == GameState.Gameplay;

        // Al salir de Gameplay restauramos el cursor por defecto
        // para que no quede un cursor de interacción congelado en pausa.
        if (!isActive)
            ApplyCursor(database != null ? database.DefaultCursor : null);
    }

    // ── Lógica de cursor ─────────────────────────────────────────────

    /// <summary>
    /// Lanza un RaycastAll desde la cámara hacia la posición del mouse.
    /// Entre todos los hits con layer registrado, aplica el cursor
    /// cuya entrada tenga la mayor prioridad.
    /// </summary>
    private void UpdateCursor()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray     ray      = mainCamera.ScreenPointToRay(mousePos);

        RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance, detectableLayers);

        CursorEntry bestEntry = null;

        foreach (RaycastHit hit in hits)
        {
            CursorEntry entry = database.GetEntryForLayer(hit.collider.gameObject.layer);

            if (entry == null) continue;

            if (bestEntry == null || entry.Priority > bestEntry.Priority)
                bestEntry = entry;
        }

        Texture2D target = bestEntry != null
            ? bestEntry.CursorTexture
            : database.DefaultCursor;

        ApplyCursor(target);
    }

    /// <summary>
    /// Aplica el cursor solo si cambió respecto al actual.
    /// Evita llamar Cursor.SetCursor cada frame cuando no hay cambios.
    /// </summary>
    private void ApplyCursor(Texture2D texture)
    {
        if (texture == currentCursor) return;

        currentCursor = texture;
        Cursor.SetCursor(currentCursor, Vector2.zero, CursorMode.Auto);
    }
}