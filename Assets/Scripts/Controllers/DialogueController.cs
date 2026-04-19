using UnityEngine;

/// <summary>
/// Controla la activación del sistema de diálogo.
/// </summary>
public class DialogueController : BaseController
{
    [Header("Dialogue System")]

    [SerializeField, Tooltip("Sistema principal de diálogos.")]
    private MonoBehaviour dialogueSystem;

    protected override void HandleGameStateChanged(GameState newState)
    {
        bool isDialogue = newState == GameState.Dialogue;

        if (dialogueSystem != null)
            dialogueSystem.enabled = isDialogue;
    }
}