using UnityEngine;

/// <summary>
/// Define un objetivo dentro de un nivel.
/// Está orientado tanto a gameplay como a aprendizaje.
/// </summary>
[CreateAssetMenu(menuName = "Game/Level/Objective Data")]
public class ObjectiveData : ScriptableObject
{
    [Header("Identification")]

    [SerializeField, Tooltip("ID único del objetivo.")]
    private string id;

    [TextArea]
    [SerializeField, Tooltip("Descripción del objetivo.")]
    private string description;

    [Header("Objective Type")]

    [SerializeField, Tooltip("Tipo de objetivo.")]
    private ObjectiveType type;

    [SerializeField, Tooltip("ID del elemento relacionado (interactable, diálogo, etc).")]
    private string targetId;

    public string Id => id;
    public string Description => description;
    public ObjectiveType Type => type;
    public string TargetId => targetId;
}