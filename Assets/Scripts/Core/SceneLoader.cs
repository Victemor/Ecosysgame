using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema global de carga de escenas.
/// Soporta carga directa o con pantalla de transición.
/// Persistente entre escenas.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;

    public static SceneLoader Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<SceneLoader>();
            return instance;
        }
    }

    [SerializeField, Tooltip("Panel de transición opcional (fade, etc). Puede ser null.")]
    private GameObject transitionPanel;

    [SerializeField, Tooltip("Duración de la transición en segundos.")]
    private float transitionDuration = 0.3f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Carga una escena por nombre.</summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadRoutine(sceneName));
    }

    /// <summary>Recarga la escena activa.</summary>
    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Vuelve al menú principal.</summary>
    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        if (transitionPanel != null)
            transitionPanel.SetActive(true);

        yield return new WaitForSeconds(transitionDuration);

        SceneManager.LoadScene(sceneName);
    }
}