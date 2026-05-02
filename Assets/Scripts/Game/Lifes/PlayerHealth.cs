using System;
using UnityEngine;

/// <summary>
/// Maneja el valor de vida del jugador y notifica cambios a la UI.
/// No conoce la representación visual: solo gestiona el número y dispara eventos.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuration")]

    [SerializeField, Tooltip("Vida máxima. Debe ser múltiplo de 3 (un fragmento por corazón).")]
    private int vidaMax = 9;

    /// <summary>
    /// Vida actual del jugador. Solo se modifica a través de TakeDamage/Heal.
    /// </summary>
    public int VidaActual { get; private set; }

    /// <summary>
    /// Vida máxima configurada.
    /// </summary>
    public int VidaMax => vidaMax;

    /// <summary>
    /// Se dispara cada vez que la vida cambia. La UI se suscribe aquí.
    /// </summary>
    public event Action<int> OnVidaChanged;

    /// <summary>
    /// Se dispara cuando la vida llega a 0.
    /// </summary>
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
    /// </summary>
    public void Heal(int cantidad)
    {
        if (cantidad <= 0) return;

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