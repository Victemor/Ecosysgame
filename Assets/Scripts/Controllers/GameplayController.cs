using UnityEngine;

/// <summary>
/// Controla la activación de sistemas de gameplay según el estado del juego.
/// </summary>
public class GameplayController : BaseController
{
    [Header("Gameplay Systems")]

    [SerializeField, Tooltip("Sistema de movimiento del jugador.")]
    private MonoBehaviour playerMovement;

    [SerializeField, Tooltip("Sistema de cámara principal.")]
    private MonoBehaviour cameraSystem;

    protected override void HandleGameStateChanged(GameState newState)
    {
        bool isGameplay = newState == GameState.Gameplay;

        SetSystemState(playerMovement, isGameplay);
        SetSystemState(cameraSystem, isGameplay);
    }

    /// <summary>
    /// Activa o desactiva un sistema de forma segura.
    /// </summary>
    private void SetSystemState(MonoBehaviour system, bool state)
    {
        if (system != null)
            system.enabled = state;
    }
}