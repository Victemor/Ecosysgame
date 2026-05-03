using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del menú principal.
/// Gestiona navegación, salida del juego y apertura de paneles con animación.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene To Load")]

    [SerializeField, Tooltip("Nombre de la escena de juego a cargar al presionar Jugar.")]
    private string gameSceneName;

    [Header("Panels")]

    [SerializeField, Tooltip("Animator del panel de configuración.")]
    private PanelAnimator configPanel;

    [SerializeField, Tooltip("Animator del panel de opciones.")]
    private PanelAnimator optionsPanel;

    [SerializeField, Tooltip("Animator del panel de información.")]
    private PanelAnimator infoPanel;

    private PanelAnimator activePanel;

    private void Start()
    {
        // Desactivar todos los paneles sin animación al iniciar
        configPanel?.gameObject.SetActive(false);
        optionsPanel?.gameObject.SetActive(false);
        infoPanel?.gameObject.SetActive(false);

        CloseAllPanels();

        // Pedir estado MainMenu al entrar en la escena de menú
        GameStateController.Instance.RequestState(GameState.MainMenu);
    }



    // ── Botones ──────────────────────────────────────────────────────

    public void OnPlayPressed()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("[MainMenuController] No hay escena de juego asignada.", this);
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnConfigPressed()  => OpenPanel(configPanel);
    public void OnOptionsPressed() => OpenPanel(optionsPanel);
    public void OnInfoPressed()    => OpenPanel(infoPanel);

    public void OnClosePanelPressed()
    {
        if (activePanel != null)
        {
            activePanel.Hide();
            activePanel = null;
        }
    }

    // ── Lógica de paneles ────────────────────────────────────────────

    /// <summary>
    /// Abre un panel. Si hay otro abierto, lo cierra primero.
    /// Si se abre el mismo panel que ya está activo, lo cierra.
    /// </summary>
    private void OpenPanel(PanelAnimator panel)
    {
        if (panel == null) return;

        if (activePanel == panel)
        {
            activePanel.Hide();
            activePanel = null;
            return;
        }

        activePanel?.Hide();
        activePanel = panel;
        activePanel.Show();
    }
}