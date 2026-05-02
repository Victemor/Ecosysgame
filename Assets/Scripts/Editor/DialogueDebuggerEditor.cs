using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor personalizado para DialogueDebugger.
/// Expone un botón de prueba que solo se activa en Play Mode.
/// </summary>
[CustomEditor(typeof(DialogueDebugger))]
public class DialogueDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8);

        if (Application.isPlaying)
        {
            if (GUILayout.Button("▶   Probar diálogo", GUILayout.Height(36)))
            {
                ((DialogueDebugger)target).RunTest();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Entra en Play Mode para probar el diálogo.", MessageType.Info);
        }
    }
}