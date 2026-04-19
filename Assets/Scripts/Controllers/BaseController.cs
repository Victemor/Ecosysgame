using UnityEngine;

/// <summary>
/// Clase base para todos los controladores del sistema.
/// Define una estructura común para reaccionar a cambios de estado.
/// </summary>
public abstract class BaseController : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    protected virtual void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    /// <summary>
    /// Método que cada controlador implementa para reaccionar a cambios de estado.
    /// </summary>
    protected abstract void HandleGameStateChanged(GameState newState);
}