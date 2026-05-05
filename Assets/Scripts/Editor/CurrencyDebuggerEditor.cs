using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CurrencyDebugger))]
public class CurrencyDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(8);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Entra en Play Mode para probar.", MessageType.Info);
            return;
        }

        CurrencyDebugger debugger = (CurrencyDebugger)target;

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.4f, 1f, 0.5f);
        if (GUILayout.Button("＋  Añadir", GUILayout.Height(32)))
            debugger.Add();

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("－  Restar", GUILayout.Height(32)))
            debugger.Subtract();

        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(4);

        if (GUILayout.Button("↺  Reset a 0", GUILayout.Height(26)))
            debugger.Reset();
    }
}