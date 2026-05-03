// GameplayUIManager.cs
using UnityEngine;

/// <summary>
/// Gestiona la UI del HUD durante gameplay.
/// Controla el panel de pausa y el panel de bitácora.
/// Usa un modelo de panel activo único para evitar superposiciones.
/// </summary>
public class GameplayUIManager : MonoBehaviour
{
    [Header("Panels")]

    [SerializeField, Tooltip("Panel de pausa con opciones: Volver al menú y Salir.")]
    private PanelAnimator pausePanel;

    [SerializeField, Tooltip("Panel de bitácora del jugador.")]
    private PanelAnimator bitacoraPanel;

    private PanelAnimator activePanel;

    private void Start()
    {
        pausePanel?.gameObject.SetActive(false);
        bitacoraPanel?.gameObject.SetActive(false);
        activePanel = null;
    }

    

    // ── Pausa ────────────────────────────────────────────────────────

    /// <summary>
    /// Abre o cierra el panel de pausa. Conectar al botón de menú del HUD.
    /// </summary>
    public void OnMenuButtonPressed() => TogglePanel(pausePanel);

    /// <summary>
    /// Cierra el panel de pausa. Conectar al botón X dentro del panel.
    /// </summary>
    public void ClosePausePanel() => ForceClosePanel(pausePanel);

    // ── Navegación ───────────────────────────────────────────────────

    /// <summary>
    /// Guarda y vuelve al menú principal.
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
    /// Abre o cierra el panel de bitácora. Conectar al botón de bitácora del HUD.
    /// </summary>
    public void OpenPanelBitacora() => TogglePanel(bitacoraPanel);

    /// <summary>
    /// Cierra el panel de bitácora directamente, sin condiciones.
    /// Conectar al botón X dentro del panel de bitácora.
    /// </summary>
    public void ClosePanelBitacora() => ForceClosePanel(bitacoraPanel);

    // ── Helpers ──────────────────────────────────────────────────────

    private void TogglePanel(PanelAnimator panel)
    {
        if (panel == null)
        {
            Debug.LogError("[GameplayUIManager] Panel no asignado en el Inspector.", this);
            return;
        }

        if (activePanel == panel)
        {
            ForceClosePanel(panel);
            return;
        }

        // Cierra el panel activo antes de abrir uno nuevo
        if (activePanel != null)
            ForceClosePanel(activePanel);

        activePanel = panel;
        activePanel.Show();
    }

    /// <summary>
    /// Cierra un panel específico sin importar cuál sea el activePanel.
    /// Este es el método real de cierre: no tiene guards que fallen silenciosamente.
    /// Se separó de TogglePanel para que los botones dedicados de cierre
    /// siempre funcionen, incluso si el estado interno desincroniza.
    /// </summary>
    private void ForceClosePanel(PanelAnimator panel)
    {
        if (panel == null)
        {
            Debug.LogError("[GameplayUIManager] Panel no asignado en el Inspector.", this);
            return;
        }

        panel.Hide();

        if (activePanel == panel)
            activePanel = null;
    }
}