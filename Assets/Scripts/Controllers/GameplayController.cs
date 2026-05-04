using UnityEngine;

/// <summary>
/// Controla la activación de sistemas de gameplay según el estado del juego.
/// También es responsable de restaurar el estado Gameplay al finalizar un diálogo,
/// desacoplando así a DialogueController de GameStateController.
/// </summary>
public class GameplayController : BaseController
{
    [Header("Gameplay Systems")]

    [SerializeField, Tooltip("Sistema de movimiento del jugador.")]
    private MonoBehaviour playerMovement;

    [SerializeField, Tooltip("Sistema de cámara principal.")]
    private MonoBehaviour cameraSystem;

    protected override void OnEnable()
    {
        base.OnEnable();
        DialogueController.Instance.OnDialogueEnded += HandleDialogueEnded;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (DialogueController.Instance != null)
            DialogueController.Instance.OnDialogueEnded -= HandleDialogueEnded;
    }

    protected override void HandleGameStateChanged(GameState newState)
    {
        bool isGameplay = newState == GameState.Gameplay;

        SetSystemState(playerMovement, isGameplay);
        SetSystemState(cameraSystem,   isGameplay);

        // Pausa el tiempo del motor en estados que requieren congelar el mundo.
        // Time.timeScale = 0 detiene físicas, animaciones, Coroutines con
        // WaitForSeconds y cualquier Update que use Time.deltaTime.
        // La UI del diálogo usa WaitForSecondsRealtime para ignorar esta pausa.
        Time.timeScale = (newState == GameState.Dialogue || newState == GameState.Paused)
            ? 0f
            : 1f;
    }

    /// <summary>
    /// Cuando el diálogo termina, solicita volver al estado Gameplay.
    /// Esta responsabilidad reside aquí y no en DialogueController,
    /// ya que es GameplayController quien sabe qué estado debe seguir al diálogo.
    /// </summary>
    private void HandleDialogueEnded()
    {
        GameStateController.Instance.RequestState(GameState.Gameplay);
    }

    /// <summary>
    /// Activa o desactiva un sistema MonoBehaviour de forma segura.
    /// </summary>
    private void SetSystemState(MonoBehaviour system, bool state)
    {
        if (system != null)
            system.enabled = state;
    }
}