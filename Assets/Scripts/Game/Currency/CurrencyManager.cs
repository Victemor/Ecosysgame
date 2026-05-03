using System;
using UnityEngine;

/// <summary>
/// Gestiona el dinero del jugador.
/// Fuente única de verdad — la UI se suscribe a OnCurrencyChanged.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    private static CurrencyManager instance;

    public static CurrencyManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<CurrencyManager>();
            return instance;
        }
    }

    [Header("Settings")]

    [SerializeField, Tooltip("Dinero inicial al arrancar.")]
    private int startingAmount = 0;

    public int Amount { get; private set; }

    /// <summary>
    /// Se dispara cada vez que el dinero cambia.
    /// Proporciona el valor anterior y el nuevo.
    /// </summary>
    public event Action<int, int> OnCurrencyChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        Amount   = startingAmount;
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>Añade dinero al jugador.</summary>
    public void Add(int amount)
    {
        if (amount <= 0) return;
        SetAmount(Amount + amount);
    }

    /// <summary>Resta dinero. No baja de 0.</summary>
    public void Subtract(int amount)
    {
        if (amount <= 0) return;
        SetAmount(Mathf.Max(0, Amount - amount));
    }

    /// <summary>Establece el dinero directamente.</summary>
    public void SetAmount(int amount)
    {
        int previous = Amount;
        Amount       = Mathf.Max(0, amount);

        if (previous != Amount)
            OnCurrencyChanged?.Invoke(previous, Amount);
    }

    /// <summary>
    /// Fuerza la notificación del valor actual aunque no haya cambiado.
    /// Usado por ResetProgress para asegurar que la UI se actualice
    /// incluso cuando el valor ya era 0 antes del reset.
    /// </summary>
    public void ForceNotify()
    {
        OnCurrencyChanged?.Invoke(Amount, Amount);
    }
}