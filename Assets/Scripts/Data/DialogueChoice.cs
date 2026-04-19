using UnityEngine;

/// <summary>
/// Representa una opción que el jugador puede elegir.
/// </summary>
[System.Serializable]
public class DialogueChoice
{
    [TextArea]
    [SerializeField, Tooltip("Texto de la opción mostrada.")]
    private string text;

    [SerializeField, Tooltip("ID del siguiente nodo al elegir esta opción.")]
    private string nextNodeId;

    public string Text => text;
    public string NextNodeId => nextNodeId;
}