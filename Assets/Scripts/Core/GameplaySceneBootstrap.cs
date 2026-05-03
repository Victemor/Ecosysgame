using UnityEngine;

/// <summary>
/// Arranca el estado Gameplay cuando se carga la escena de juego.
/// Necesario porque GameManager vive en GamePersistence (escena Menu)
/// y no sabe automáticamente qué estado pedir al cargar una nueva escena.
/// </summary>
public class GameplaySceneBootstrap : MonoBehaviour
{
    private void Start()
    {
        if (GameStateController.Instance != null)
            GameStateController.Instance.RequestState(GameState.Gameplay);
        else
            Debug.LogWarning("[GameplaySceneBootstrap] GameStateController no encontrado. " +
                             "¿Olvidaste poner GamePersistence en la escena Menu?", this);
    }
}