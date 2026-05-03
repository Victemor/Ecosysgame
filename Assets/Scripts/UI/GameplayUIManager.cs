using UnityEngine;

/// <summary>
/// Gestiona la UI del HUD durante gameplay.
/// El botón de menú abre un panel intermedio con dos opciones:
/// volver al menú o salir del juego.
/// También gestiona la apertura de la bitácora.
/// </summary>
public class GameplayUIManager : MonoBehaviour
{
    [Header("Panels")]

    [SerializeField, Tooltip("Panel de pausa/salida con botones: Volver al menú y Salir del juego.")]
    private PanelAnimator pausePanel;

    [SerializeField, Tooltip("Panel de bitácora.")]
    private PanelAnimator bitacoraPanel;

    private PanelAnimator activePanel;

    private void Start()
    {
        pausePanel?.gameObject.SetActive(false);
        bitacoraPanel?.gameObject.SetActive(false);
    }

    // ── Botón principal de menú ──────────────────────────────────────

    /// <summary>
    /// Abre o cierra el panel de pausa.
    /// Conectar al botón de "menú" en el HUD.
    /// </summary>
    public void OnMenuButtonPressed() => TogglePanel(pausePanel);

    /// <summary>
    /// Cierra el panel de pausa explícitamente.
    /// Conectar al botón de cerrar/volver dentro del panel de pausa.
    /// </summary>
    public void ClosePausePanel()
    {
        if (activePanel == pausePanel)
        {
            pausePanel.Hide();
            activePanel = null;
        }
    }

    // ── Desde el panel de pausa ──────────────────────────────────────

    /// <summary>
    /// Guarda el progreso y vuelve al menú principal.
    /// </summary>
    public void ReturnToMenu()
    {
        ProgressManager.Instance?.Save();
        GameStateController.Instance?.RequestState(GameState.Transition);
        SceneLoader.Instance.LoadMainMenu();
    }

    /// <summary>
    /// Guarda y cierra el juego.
    /// </summary>
    public void QuitGame()
    {
        ProgressManager.Instance?.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Bitácora ─────────────────────────────────────────────────────

    /// <summary>
    /// Abre o cierra el panel de bitácora.
    /// </summary>
    public void OpenPanelBitacora() => TogglePanel(bitacoraPanel);

    /// <summary>
    /// Cierra el panel de bitácora explícitamente.
    /// Conectar al botón de cerrar dentro del panel de bitácora.
    /// </summary>
    public void ClosePanelBitacora()
    {
        if (activePanel == bitacoraPanel)
        {
            bitacoraPanel.Hide();
            activePanel = null;
        }
    }

    // ── Helper ───────────────────────────────────────────────────────

    private void TogglePanel(PanelAnimator panel)
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