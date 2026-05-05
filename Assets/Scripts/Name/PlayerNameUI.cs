using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la UI de nombre del jugador en el menú principal.
/// Gestiona el InputField, el botón de confirmación y el texto de saludo.
///
/// Robusto ante recargas de escena: se re-suscribe a los datos en OnEnable
/// y refresca la UI en cada visita al menú sin depender de estado previo.
/// </summary>
public class PlayerNameUI : MonoBehaviour
{
    [Header("Componentes UI")]

    [SerializeField, Tooltip("Campo de texto donde el jugador escribe su nombre.")]
    private TMP_InputField nameInputField;

    [SerializeField, Tooltip("Botón que confirma y guarda el nombre.")]
    private Button confirmButton;

    [SerializeField, Tooltip("Texto que muestra el saludo con el nombre del jugador.")]
    private TextMeshProUGUI greetingText;

    [SerializeField, Tooltip("Texto pequeño que muestra feedback de validación " +
                             "(error o confirmación). Puede ser null si no se usa.")]
    private TextMeshProUGUI feedbackText;

    [Header("Configuración")]

    [SerializeField, Tooltip("Mensaje de saludo cuando el jugador tiene nombre. " +
                             "Usa {0} como placeholder para el nombre. Ej: '¡Hola, {0}!'")]
    private string greetingFormat = "¡Hola, {0}!";

    [SerializeField, Tooltip("Mensaje de saludo cuando no hay nombre guardado.")]
    private string fallbackGreeting = "¡Hola de nuevo!";

    [SerializeField, Tooltip("Lista de nombres prohibidos. " +
                             "Puede ser null — la validación igual funciona sin ella.")]
    private ForbiddenNamesList forbiddenNames;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        if (nameInputField != null)
            nameInputField.characterLimit = 15;

        confirmButton?.onClick.AddListener(OnConfirmPressed);
    }

    private void OnEnable()
    {
        // Se llama cada vez que el menú se activa (incluyendo vueltas desde gameplay).
        // Refrescar aquí garantiza que el saludo siempre refleje el nombre actual.
        RefreshGreeting();
        ClearInputField();
        ClearFeedback();
    }

    private void OnDestroy()
    {
        confirmButton?.onClick.RemoveListener(OnConfirmPressed);
    }

    // ── Handlers ─────────────────────────────────────────────────────

    private void OnConfirmPressed()
    {
        if (nameInputField == null) return;

        string input = nameInputField.text;

        PlayerNameValidator.ValidationResult result =
            PlayerNameValidator.Validate(input, forbiddenNames);

        if (result != PlayerNameValidator.ValidationResult.Valid)
        {
            ShowFeedback(PlayerNameValidator.GetErrorMessage(result), isError: true);
            return;
        }

        string validName = input.Trim();

        ProgressManager.Instance.SetPlayerName(validName);

        RefreshGreeting();
        ClearInputField();
        ShowFeedback("¡Nombre guardado!", isError: false);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Actualiza el texto de saludo con el nombre guardado.
    /// Si no hay nombre, muestra el fallback.
    /// </summary>
    private void RefreshGreeting()
    {
        if (greetingText == null || ProgressManager.Instance == null) return;

        string savedName = ProgressManager.Instance.Progress.playerName;

        greetingText.text = string.IsNullOrWhiteSpace(savedName)
            ? fallbackGreeting
            : string.Format(greetingFormat, savedName);
    }

    private void ClearInputField()
    {
        if (nameInputField != null)
            nameInputField.text = string.Empty;
    }

    private void ShowFeedback(string message, bool isError)
    {
        if (feedbackText == null) return;

        feedbackText.text  = message;
        feedbackText.color = isError
            ? new Color(0.9f, 0.3f, 0.3f)
            : new Color(0.3f, 0.85f, 0.45f);
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = string.Empty;
    }
}