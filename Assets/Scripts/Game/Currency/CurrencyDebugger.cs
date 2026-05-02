using UnityEngine;

/// <summary>
/// Herramienta de prueba para el sistema de dinero en runtime.
/// </summary>
public class CurrencyDebugger : MonoBehaviour
{
    [SerializeField, Tooltip("Referencia al CurrencyManager.")]
    private CurrencyManager currencyManager;

    [SerializeField, Tooltip("Cantidad a añadir o restar.")]
    private int amount = 10;

    public void Add()      => currencyManager?.Add(amount);
    public void Subtract() => currencyManager?.Subtract(amount);
    public void Reset()    => currencyManager?.SetAmount(0);
}