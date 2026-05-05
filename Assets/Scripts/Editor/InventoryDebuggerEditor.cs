#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Editor para InventoryDebugger.
/// Muestra el estado del inventario y un botón para agregar el siguiente ítem.
/// Solo disponible en Play Mode.
/// </summary>
[CustomEditor(typeof(InventoryDebugger))]
public class InventoryDebuggerEditor : Editor
{
    private static readonly Color ColorAdd      = new Color(0.25f, 0.75f, 0.45f);
    private static readonly Color ColorFull     = new Color(1f,    0.35f, 0.35f);
    private static readonly Color ColorAvail    = new Color(0.30f, 0.85f, 0.45f);

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Los controles de debug están disponibles en Play Mode.", MessageType.Info);
            return;
        }

        InventoryDebugger debugger = (InventoryDebugger)target;

        // ── Estado actual ──────────────────────────────────────────────
        EditorGUILayout.LabelField("Estado del Inventario", EditorStyles.boldLabel);

        bool isFull = InventorySystem.Instance != null && InventorySystem.Instance.IsFull;

        GUI.color = isFull ? ColorFull : ColorAvail;
        EditorGUILayout.LabelField(
            isFull ? "● Inventario lleno" : "● Hay espacio disponible",
            EditorStyles.helpBox
        );
        GUI.color = Color.white;

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField($"Próximo ítem: {debugger.NextItemName}",
            EditorStyles.miniLabel);

        EditorGUILayout.Space(8);

        // ── Botón ──────────────────────────────────────────────────────
        GUI.enabled = !isFull;
        GUI.color   = ColorAdd;

        if (GUILayout.Button("+ Agregar Ítem", GUILayout.Height(32)))
            debugger.AddNextItem();

        GUI.color   = Color.white;
        GUI.enabled = true;
    }
}
#endif