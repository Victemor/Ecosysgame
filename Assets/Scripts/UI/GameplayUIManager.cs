using UnityEngine;

/// <summary>
/// Gestiona la UI del HUD durante gameplay.
/// Punto de entrada para acciones de UI en escena de juego:
/// volver al menú, pausa, etc.
/// Se amplía aquí cuando se agreguen más elementos de UI en gameplay.
/// </summary>
public class GameplayUIManager : MonoBehaviour
{
    /// <summary>
    /// Guarda el progreso y vuelve al menú principal.
    /// Llamado por el botón "Volver al menú" en el HUD.
    /// </summary>
    public void ReturnToMenu()
    {
        // Guardar antes de salir
        ProgressManager.Instance?.Save();

        // Cambiar estado antes de cargar la escena
        GameStateController.Instance.RequestState(GameState.Transition);

        SceneLoader.Instance.LoadMainMenu();
    }
}