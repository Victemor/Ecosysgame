using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor personalizado para HealthDebugger.
/// Muestra botones de control solo en Play Mode.
/// </summary>
[CustomEditor(typeof(HealthDebugger))]
public class HealthDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Entra en Play Mode para probar el sistema de vida.", MessageType.Info);
            return;
        }

        HealthDebugger debugger = (HealthDebugger)target;

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("❤  Quitar vida", GUILayout.Height(32)))
            debugger.QuitarVida();

        GUI.backgroundColor = new Color(0.4f, 1f, 0.6f);
        if (GUILayout.Button("✚  Recuperar vida", GUILayout.Height(32)))
            debugger.RecuperarVida();

        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(4);

        if (GUILayout.Button("↺   Reset vida completa", GUILayout.Height(28)))
            debugger.ResetearVida();
    }
}