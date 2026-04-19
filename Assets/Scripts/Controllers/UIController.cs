using UnityEngine;

/// <summary>
/// Gestiona la visibilidad de la UI según el estado del juego.
/// </summary>
public class UIController : BaseController
{
    [Header("UI Elements")]

    [SerializeField, Tooltip("Canvas del menú principal.")]
    private GameObject mainMenuUI;

    [SerializeField, Tooltip("Canvas del HUD de juego.")]
    private GameObject gameplayUI;

    protected override void HandleGameStateChanged(GameState newState)
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(newState == GameState.MainMenu);

        if (gameplayUI != null)
            gameplayUI.SetActive(newState == GameState.Gameplay);
    }
}