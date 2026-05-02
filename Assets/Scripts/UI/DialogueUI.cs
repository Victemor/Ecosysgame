using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la presentación visual del sistema de diálogo.
/// Gestiona el panel, el efecto typewriter y el avance por input del jugador.
/// Solo maneja vista y entrada; la lógica de nodos reside en DialogueController.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Panel")]

    [SerializeField, Tooltip("Raíz del panel de diálogo. Se activa/desactiva con cada diálogo.")]
    private GameObject dialoguePanel;

    [SerializeField, Tooltip("Texto del nombre del personaje que habla.")]
    private TextMeshProUGUI speakerNameText;

    [SerializeField, Tooltip("Texto del cuerpo del diálogo.")]
    private TextMeshProUGUI bodyText;

    [SerializeField, Tooltip("Indicador visual de que el texto terminó y el jugador puede avanzar.")]
    private GameObject continueIndicator;

    [Header("Typewriter")]

    [SerializeField, Tooltip("Caracteres revelados por segundo.")]
    private float charsPerSecond = 40f;

    // ── Estado interno ──────────────────────────────────────────────

    private bool isTyping;
    private bool canAdvance;
    private Coroutine typewriterRoutine;

    // ── Input ───────────────────────────────────────────────────────

    /// <summary>
    /// InputAction creado por código para no depender de un Input Actions asset.
    /// Soporta mouse (PC) y touch (móvil).
    /// </summary>
    private InputAction advanceAction;

    // ── Unity lifecycle ─────────────────────────────────────────────

    private void Awake()
    {
        advanceAction = new InputAction(name: "AdvanceDialogue", type: InputActionType.Button);
        advanceAction.AddBinding("<Mouse>/leftButton");
        advanceAction.AddBinding("<Touchscreen>/primaryTouch/tap");

        advanceAction.performed += _ => HandleAdvanceInput();

        // El panel empieza oculto
        dialoguePanel.SetActive(false);
        SetContinueIndicator(false);
    }

    private void OnEnable()
    {
        advanceAction.Enable();

        DialogueController.Instance.OnDialogueStarted += HandleDialogueStarted;
        DialogueController.Instance.OnNodeChanged     += HandleNodeChanged;
        DialogueController.Instance.OnDialogueEnded   += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        advanceAction.Disable();

        if (DialogueController.Instance == null) return;

        DialogueController.Instance.OnDialogueStarted -= HandleDialogueStarted;
        DialogueController.Instance.OnNodeChanged     -= HandleNodeChanged;
        DialogueController.Instance.OnDialogueEnded   -= HandleDialogueEnded;
    }

    // ── Handlers de DialogueController ──────────────────────────────

    private void HandleDialogueStarted(DialogueData data)
    {
        dialoguePanel.SetActive(true);
        speakerNameText.text = data.SpeakerName;
        bodyText.text        = string.Empty;
        canAdvance           = false;
        SetContinueIndicator(false);
    }

    private void HandleNodeChanged(DialogueNode node)
    {
        if (node == null) return;

        if (typewriterRoutine != null)
            StopCoroutine(typewriterRoutine);

        typewriterRoutine = StartCoroutine(TypewriterRoutine(node.Text));
    }

    private void HandleDialogueEnded()
    {
        if (typewriterRoutine != null)
            StopCoroutine(typewriterRoutine);

        dialoguePanel.SetActive(false);
        isTyping   = false;
        canAdvance = false;
        SetContinueIndicator(false);
    }

    // ── Input handler ────────────────────────────────────────────────

    /// <summary>
    /// Si el texto está escribiéndose: lo completa al instante.
    /// Si ya terminó: avanza al siguiente nodo o cierra el diálogo.
    /// </summary>
    private void HandleAdvanceInput()
    {
        if (!dialoguePanel.activeSelf) return;

        if (isTyping)
        {
            SkipTypewriter();
            return;
        }

        if (canAdvance)
        {
            canAdvance = false;
            SetContinueIndicator(false);
            DialogueController.Instance.ContinueDialogue();
        }
    }

    // ── Typewriter ───────────────────────────────────────────────────

    /// <summary>
    /// Revela el texto carácter por carácter usando el sistema de visibilidad de TMP.
    /// Usar maxVisibleCharacters es más eficiente que manipular el string en cada frame.
    /// </summary>
    private IEnumerator TypewriterRoutine(string text)
    {
        isTyping   = true;
        canAdvance = false;
        SetContinueIndicator(false);

        bodyText.text = text;
        bodyText.ForceMeshUpdate(); // ← esto es lo que falta

        int totalChars = bodyText.textInfo.characterCount;
        float interval = 1f / charsPerSecond;

        bodyText.maxVisibleCharacters = 0;

        for (int i = 0; i < totalChars; i++)
        {
            bodyText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(interval);
        }

        FinishTypewriter();
    }

    /// <summary>
    /// Completa el texto instantáneamente sin esperar el intervalo.
    /// </summary>
    private void SkipTypewriter()
    {
        if (typewriterRoutine != null)
            StopCoroutine(typewriterRoutine);

        bodyText.maxVisibleCharacters = bodyText.textInfo.characterCount;
        FinishTypewriter();
    }

    private void FinishTypewriter()
    {
        isTyping   = false;
        canAdvance = true;
        typewriterRoutine = null;
        SetContinueIndicator(true);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private void SetContinueIndicator(bool active)
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(active);
    }
}