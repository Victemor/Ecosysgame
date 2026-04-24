using UnityEngine;

/// <summary>
/// Clase base para todos los controladores del sistema.
/// Define una estructura común para reaccionar a cambios de estado del juego.
///
/// NOTA DE ORDEN DE EJECUCIÓN:
/// Para evitar race conditions en OnEnable, GameManager debe inicializarse
/// antes que cualquier BaseController. Configura esto en:
/// Edit → Project Settings → Script Execution Order
/// Añade GameManager con un valor negativo (ej. -100) para garantizar
/// que exista antes de que cualquier controlador intente suscribirse.
/// </summary>
public abstract class BaseController : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        // Verificación defensiva: si GameManager no existe aún,
        // el getter del singleton lo encontrará o lo creará.
        // El orden de ejecución en Project Settings garantiza que
        // GameManager.Awake() ya corrió antes de llegar aquí.
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                $"[{GetType().Name}] GameManager no encontrado al intentar suscribirse. " +
                "Verifica el Script Execution Order en Project Settings.",
                this
            );
            return;
        }

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    protected virtual void OnDisable()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    /// <summary>
    /// Cada controlador implementa este método para reaccionar a cambios de estado.
    /// </summary>
    protected abstract void HandleGameStateChanged(GameState newState);
}