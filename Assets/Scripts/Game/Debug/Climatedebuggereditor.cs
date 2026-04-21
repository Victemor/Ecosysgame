// Este archivo DEBE estar en una carpeta llamada Editor/
// Ruta: Assets/Scripts/Editor/ClimateDebuggerEditor.cs
// No se incluye en builds de producción automáticamente.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Editor para ClimateDebugger.
/// Muestra el estado del clima activo, botones por evento y un botón de stop.
/// Solo visible en el Inspector durante Play Mode (los eventos requieren que
/// ClimateController esté inicializado en la escena).
/// </summary>
[CustomEditor(typeof(ClimateDebugger))]
public class ClimateDebuggerEditor : Editor
{
    // Colores de los botones
    private static readonly Color ColorTrigger  = new Color(0.25f, 0.65f, 1f);
    private static readonly Color ColorStop     = new Color(1f, 0.35f, 0.35f);
    private static readonly Color ColorActive   = new Color(0.3f, 0.85f, 0.45f);
    private static readonly Color ColorInactive = new Color(0.55f, 0.55f, 0.55f);

    public override void OnInspectorGUI()
    {
        ClimateDebugger debugger = (ClimateDebugger)target;

        // ── Propiedades serializadas normales ──────────────────────────────
        DrawDefaultInspector();

        EditorGUILayout.Space(12);

        // ── Solo funciona en Play Mode ─────────────────────────────────────
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Los controles de debug están disponibles en Play Mode.",
                MessageType.Info
            );
            return;
        }

        // ── Estado actual ──────────────────────────────────────────────────
        EditorGUILayout.LabelField("Estado del Clima", EditorStyles.boldLabel);

        bool hasActive = ClimateController.Instance != null &&
                         ClimateController.Instance.HasActiveEvent;

        GUI.color = hasActive ? ColorActive : ColorInactive;
        EditorGUILayout.LabelField(
            $"● Evento activo: {debugger.ActiveEventName}",
            EditorStyles.helpBox
        );
        GUI.color = Color.white;

        EditorGUILayout.Space(8);

        // ── Botones por evento ─────────────────────────────────────────────
        EditorGUILayout.LabelField("Disparar Evento", EditorStyles.boldLabel);

        if (debugger.TestEvents == null || debugger.TestEvents.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Agrega ClimateEventData a la lista 'Test Events' para ver botones aquí.",
                MessageType.Warning
            );
        }
        else
        {
            for (int i = 0; i < debugger.TestEvents.Count; i++)
            {
                ClimateEventData evt = debugger.TestEvents[i];

                if (evt == null)
                    continue;

                GUI.color = ColorTrigger;

                if (GUILayout.Button($"▶  {evt.DisplayName}  (intensidad {evt.Intensity:F1})", GUILayout.Height(32)))
                {
                    debugger.TriggerEvent(i);
                }

                GUI.color = Color.white;
            }
        }

        EditorGUILayout.Space(8);

        // ── Botón Stop ─────────────────────────────────────────────────────
        EditorGUILayout.LabelField("Control", EditorStyles.boldLabel);

        GUI.enabled = hasActive;
        GUI.color   = ColorStop;

        if (GUILayout.Button("■  Detener evento actual", GUILayout.Height(36)))
        {
            debugger.StopCurrentEvent();
        }

        GUI.color   = Color.white;
        GUI.enabled = true;

        // Fuerza repintado del Inspector cada frame en Play Mode para
        // mantener el estado del evento sincronizado visualmente.
        if (Application.isPlaying)
            Repaint();
    }
}
#endif