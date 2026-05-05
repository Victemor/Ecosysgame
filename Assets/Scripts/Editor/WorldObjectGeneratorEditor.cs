using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor personalizado para WorldObjectGenerator.
/// Expone botones de generación y limpieza usables en Edit Mode y Play Mode.
/// </summary>
[CustomEditor(typeof(WorldObjectGenerator))]
public class WorldObjectGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        WorldObjectGenerator generator = (WorldObjectGenerator)target;

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("⟳   Generar mundo", GUILayout.Height(38)))
        {
            generator.Generate();
            EditorUtility.SetDirty(generator.gameObject);
        }

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("✕   Limpiar objetos", GUILayout.Height(30)))
        {
            generator.ClearGenerated();
            EditorUtility.SetDirty(generator.gameObject);
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox(
            "Puedes generar en Edit Mode sin entrar en Play. " +
            "Los objetos se instancian como prefabs y soportan Undo (Ctrl+Z).",
            MessageType.Info
        );
    }
}