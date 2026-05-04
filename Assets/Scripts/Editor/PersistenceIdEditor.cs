#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Editor para PersistenceId.
/// Muestra el ID actual y un botón para resetearlo al nombre del GameObject.
/// </summary>
[CustomEditor(typeof(PersistenceId))]
public class PersistenceIdEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(6);

        PersistenceId pid = (PersistenceId)target;

        if (GUILayout.Button("Resetear ID al nombre del GameObject"))
        {
            Undo.RecordObject(pid, "Reset PersistenceId");
            pid.ResetToGameObjectName();
            EditorUtility.SetDirty(pid);
        }
    }
}
#endif