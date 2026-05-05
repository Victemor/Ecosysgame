using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProgressDebugger))]
public class ProgressDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(8);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Entra en Play Mode para usar el debugger.", MessageType.Info);
            return;
        }

        ProgressDebugger d = (ProgressDebugger)target;

        EditorGUILayout.LabelField("Ecopuntos", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.4f, 1f, 0.5f);
        if (GUILayout.Button("＋ Añadir", GUILayout.Height(30)))
            d.AddEcopuntos();

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("－ Restar", GUILayout.Height(30)))
            d.SubtractEcopuntos();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Progreso Total", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);
        if (GUILayout.Button("✔ Establecer progreso", GUILayout.Height(30)))
            d.SetProgreso();

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Sistema", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);
        if (GUILayout.Button("💾 Guardar", GUILayout.Height(30)))
            d.ForceSave();

        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        if (GUILayout.Button("📂 Cargar", GUILayout.Height(30)))
            d.ForceLoad();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
        if (GUILayout.Button("↺ Reiniciar progreso", GUILayout.Height(30)))
            d.ResetProgress();

        GUI.backgroundColor = Color.white;

        // Mostrar datos actuales
        if (ProgressManager.Instance != null)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Estado actual", EditorStyles.boldLabel);
            GameProgress p = ProgressManager.Instance.Progress;
            EditorGUILayout.LabelField($"Tiempo: {ProgressManager.Instance.GetFormattedTime()}");
            EditorGUILayout.LabelField($"Ecopuntos: {p.ecopuntos}");
            EditorGUILayout.LabelField($"Progreso: {p.progresoTotal:0.0}%");
            EditorGUILayout.LabelField($"Guardado en: {Application.persistentDataPath}");
        }
    }
}