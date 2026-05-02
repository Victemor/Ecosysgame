using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Herramienta de prueba en runtime para el sistema de diálogo.
/// Permite disparar un diálogo con texto libre sin necesitar un DialogueData asset.
/// Solo para uso en desarrollo; no incluir en builds de producción.
/// </summary>
public class DialogueDebugger : MonoBehaviour
{
    [Header("Test Dialogue")]

    [SerializeField, Tooltip("Nombre del personaje que aparecerá en el cuadro de diálogo.")]
    private string speakerName = "Personaje";

    [SerializeField, Tooltip("Cada entrada es una línea de diálogo. El jugador avanza haciendo clic.")]
    private List<string> lines = new List<string>
    {
        "Hola, soy un personaje de prueba.",
        "Puedes avanzar haciendo clic en cualquier parte.",
        "Esta es la última línea. El cuadro se cerrará al avanzar."
    };

    /// <summary>
    /// Inicia el diálogo de prueba. Llamado desde DialogueDebuggerEditor en Play Mode.
    /// </summary>
    public void RunTest()
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("[DialogueDebugger] No hay líneas de diálogo configuradas.", this);
            return;
        }

        DialogueController.Instance.StartTestDialogue(speakerName, lines.ToArray());
    }
}