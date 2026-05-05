using UnityEngine;

/// <summary>
/// Escucha cambios en vida y ecopuntos durante el gameplay y
/// persiste cada cambio inmediatamente a disco.
///
/// Vive en GameplayManagers — es específico de la escena de gameplay.
/// Esto garantiza que si el jugador cierra en cualquier momento,
/// la vida y el dinero actuales ya están guardados.
/// </summary>
public class GameProgressAutoSaver : MonoBehaviour
{
    private PlayerHealth    playerHealth;
    private CurrencyManager currency;

    private void Start()
    {
        // Buscar en Start (no Awake) para asegurar que los sistemas
        // del jugador ya se inicializaron en sus propios Awake.
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        currency     = CurrencyManager.Instance;

        if (playerHealth != null)
            playerHealth.OnVidaChanged += HandleVidaChanged;
        else
            Debug.LogWarning("[GameProgressAutoSaver] PlayerHealth no encontrado en escena.", this);

        if (currency != null)
            currency.OnCurrencyChanged += HandleCurrencyChanged;
        else
            Debug.LogWarning("[GameProgressAutoSaver] CurrencyManager no encontrado.", this);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnVidaChanged -= HandleVidaChanged;

        if (currency != null)
            currency.OnCurrencyChanged -= HandleCurrencyChanged;
    }

    // ── Handlers ─────────────────────────────────────────────────────

    private void HandleVidaChanged(int vida)
    {
        ProgressManager.Instance?.SaveVidaActual(vida);
    }

    private void HandleCurrencyChanged(int previous, int current)
    {
        ProgressManager.Instance?.SaveEcopuntosActual(current);
    }
}