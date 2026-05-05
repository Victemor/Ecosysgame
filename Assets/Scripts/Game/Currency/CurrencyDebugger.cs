using UnityEngine;

/// <summary>
/// Herramienta de prueba para el sistema de dinero en runtime.
/// Auto-busca el CurrencyManager si no está asignado manualmente.
/// </summary>
public class CurrencyDebugger : MonoBehaviour
{
    [SerializeField, Tooltip("Referencia al CurrencyManager. Si está vacío se busca automáticamente.")]
    private CurrencyManager currencyManager;

    [SerializeField, Tooltip("Cantidad a añadir o restar.")]
    private int amount = 10;

    private void Awake()
    {
        if (currencyManager == null)
            currencyManager = CurrencyManager.Instance;
    }

    private CurrencyManager GetManager()
    {
        if (currencyManager == null)
            currencyManager = CurrencyManager.Instance;
        return currencyManager;
    }

    public void Add()      => GetManager()?.Add(amount);
    public void Subtract() => GetManager()?.Subtract(amount);
    public void Reset()    => GetManager()?.SetAmount(0);
}