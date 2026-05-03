using UnityEngine;

/// <summary>
/// Controla el grupo de dígitos que muestra el dinero del jugador.
/// Si no hay CurrencyManager asignado en el Inspector, lo busca automáticamente
/// usando el singleton. Esto permite que funcione con GamePersistence.
/// </summary>
public class CurrencyDisplay : MonoBehaviour
{
    [Header("References")]

    [SerializeField, Tooltip("Referencia al CurrencyManager. Si está vacío se busca automáticamente.")]
    private CurrencyManager currencyManager;

    [SerializeField, Tooltip("Dígitos de izquierda a derecha. El primero es el de mayor valor.")]
    private DigitDisplay[] digits;

    [SerializeField, Tooltip("Delay escalonado entre dígitos para efecto cascada (segundos).")]
    private float digitStagger = 0.04f;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        // Auto-buscar si no fue asignado en Inspector
        if (currencyManager == null)
            currencyManager = CurrencyManager.Instance;

        if (currencyManager == null)
            Debug.LogError("[CurrencyDisplay] No se encontró CurrencyManager en la escena.", this);
    }

    private void OnEnable()
    {
        if (currencyManager == null)
            currencyManager = CurrencyManager.Instance;

        if (currencyManager == null) return;

        currencyManager.OnCurrencyChanged += HandleCurrencyChanged;
        UpdateDisplay(currencyManager.Amount, false);
    }

    private void OnDisable()
    {
        if (currencyManager != null)
            currencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
    }

    // ── Handler ──────────────────────────────────────────────────────

    private void HandleCurrencyChanged(int previous, int current)
    {
        UpdateDisplay(current, true);
    }

    // ── Display ──────────────────────────────────────────────────────

    private void UpdateDisplay(int value, bool animate)
    {
        if (digits == null || digits.Length == 0) return;

        int totalSlots = digits.Length;

        for (int i = 0; i < totalSlots; i++)
        {
            if (digits[i] == null) continue;

            int fromRight  = totalSlots - 1 - i;
            int digitValue = (value / (int)Mathf.Pow(10, fromRight)) % 10;

            float delay = animate ? (totalSlots - 1 - i) * digitStagger : 0f;

            if (animate && delay > 0f)
                StartCoroutine(AnimateWithDelay(digits[i], digitValue, delay));
            else
                digits[i].SetDigit(digitValue, animate);
        }
    }

    private System.Collections.IEnumerator AnimateWithDelay(
        DigitDisplay digit, int value, float delay)
    {
        yield return new WaitForSeconds(delay);
        digit.SetDigit(value, true);
    }
}