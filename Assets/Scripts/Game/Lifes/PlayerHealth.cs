using System;
using UnityEngine;

/// <summary>
/// Maneja el valor de vida del jugador y notifica cambios a la UI.
/// Si se cura estando al máximo de vida, otorga 25 monedas por fragmento.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuration")]

    [SerializeField, Tooltip("Vida máxima. Debe ser múltiplo de 3 (un fragmento por corazón).")]
    private int vidaMax = 9;

    [SerializeField, Tooltip("Monedas que otorga curar un fragmento cuando la vida ya está al máximo.")]
    private int monedasPorCuracionExtra = 25;

    /// <summary>Vida actual del jugador.</summary>
    public int VidaActual { get; private set; }

    /// <summary>Vida máxima configurada.</summary>
    public int VidaMax => vidaMax;

    /// <summary>Se dispara cada vez que la vida cambia.</summary>
    public event Action<int> OnVidaChanged;

    /// <summary>Se dispara cuando la vida llega a 0.</summary>
    public event Action OnMuerte;

    private void Awake()
    {
        VidaActual = vidaMax;
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Reduce la vida del jugador. Si llega a 0, dispara OnMuerte.
    /// </summary>
    public void TakeDamage(int cantidad)
    {
        if (cantidad <= 0) return;

        VidaActual = Mathf.Max(0, VidaActual - cantidad);
        OnVidaChanged?.Invoke(VidaActual);

        if (VidaActual == 0)
            OnMuerte?.Invoke();
    }

    /// <summary>
    /// Recupera vida del jugador sin superar el máximo.
    /// Si ya estaba al máximo, otorga monedas en lugar de curar.
    /// </summary>
    public void Heal(int cantidad)
    {
        if (cantidad <= 0) return;

        bool estabaLleno = VidaActual >= vidaMax;

        if (estabaLleno)
        {
            CurrencyManager.Instance?.Add(monedasPorCuracionExtra);
            return;
        }

        VidaActual = Mathf.Min(vidaMax, VidaActual + cantidad);
        OnVidaChanged?.Invoke(VidaActual);
    }

    /// <summary>
    /// Establece la vida directamente. Útil para testing y guardado.
    /// </summary>
    public void SetVida(int valor)
    {
        VidaActual = Mathf.Clamp(valor, 0, vidaMax);
        OnVidaChanged?.Invoke(VidaActual);
    }
}