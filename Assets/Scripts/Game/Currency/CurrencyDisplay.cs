using UnityEngine;

/// <summary>
/// Controla el grupo de dígitos que muestra el dinero del jugador.
/// Funciona igual que HealthUI: tú colocas N imágenes en escena,
/// las asignas al array, y el script las actualiza automáticamente.
/// Los dígitos de mayor peso van a la izquierda.
/// Si el número tiene menos dígitos que imágenes, los sobrantes muestran 0.
/// </summary>
public class CurrencyDisplay : MonoBehaviour
{
    [Header("References")]

    [SerializeField, Tooltip("Referencia al CurrencyManager del jugador.")]
    private CurrencyManager currencyManager;

    [SerializeField, Tooltip("Dígitos de izquierda a derecha. El primero es el de mayor valor.")]
    private DigitDisplay[] digits;

    [SerializeField, Tooltip("Delay escalonado entre dígitos para efecto cascada (segundos).")]
    private float digitStagger = 0.04f;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void OnEnable()
    {
        if (currencyManager == null)
        {
            Debug.LogError("[CurrencyDisplay] Asigna un CurrencyManager.", this);
            return;
        }

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

    /// <summary>
    /// Distribuye el valor en los dígitos de derecha a izquierda.
    /// Los slots sobrantes (dígitos de mayor peso sin valor) muestran 0.
    /// </summary>
    private void UpdateDisplay(int value, bool animate)
    {
        if (digits == null || digits.Length == 0) return;

        int totalSlots = digits.Length;

        for (int i = 0; i < totalSlots; i++)
        {
            if (digits[i] == null) continue;

            // Índice desde la derecha: el último dígito es las unidades
            int fromRight  = totalSlots - 1 - i;
            int digitValue = (value / (int)Mathf.Pow(10, fromRight)) % 10;

            float delay = animate ? (totalSlots - 1 - i) * digitStagger : 0f;

            // Si hay delay, se necesita animar con un pequeño delay por dígito
            // Se pasa el animate directo — DigitDisplay maneja si cambió o no
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