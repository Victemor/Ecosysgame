using UnityEngine;

/// <summary>
/// Arranca el estado Gameplay cuando la escena de juego carga.
/// Necesario porque GameManager vive en GamePersistence (Menu scene)
/// y al cargar SampleScene nadie pide el estado Gameplay automáticamente.
/// </summary>
public class GameplaySceneBootstrap : MonoBehaviour
{
    private void Start()
    {
        GameStateController.Instance.RequestState(GameState.Gameplay);
    }
}