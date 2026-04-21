using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Herramienta de debug para el sistema climático.
/// Solo existe en el GameObject durante desarrollo — no afecta ningún sistema de gameplay.
/// El Custom Editor en ClimateDebuggerEditor.cs pinta los botones en el Inspector.
/// </summary>
public class ClimateDebugger : MonoBehaviour
{
    [Header("Eventos Disponibles")]

    [SerializeField, Tooltip("Lista de ClimateEventData para probar manualmente.")]
    private List<ClimateEventData> testEvents = new List<ClimateEventData>();

    [Header("Estado (solo lectura)")]

    [SerializeField, HideInInspector]
    private string activeEventName = "Ninguno";

    /// <summary>
    /// Nombre del evento activo actual. Leído por el Editor para mostrar el estado.
    /// </summary>
    public string ActiveEventName => activeEventName;

    /// <summary>
    /// Lista de eventos de prueba disponibles. Leída por el Editor para pintar los botones.
    /// </summary>
    public IReadOnlyList<ClimateEventData> TestEvents => testEvents;

    private void Update()
    {
        // Mantiene el nombre sincronizado con el estado real del ClimateController.
        activeEventName = ClimateController.Instance.HasActiveEvent
            ? ClimateController.Instance.ActiveEvent.DisplayName
            : "Ninguno";
    }

    // ─────────────────────────────────────────────
    // API pública — llamada desde el Custom Editor
    // ─────────────────────────────────────────────

    /// <summary>
    /// Inicia el evento climático en el índice dado de la lista de prueba.
    /// </summary>
    public void TriggerEvent(int index)
    {
        if (index < 0 || index >= testEvents.Count)
            return;

        ClimateEventData data = testEvents[index];

        if (data == null)
            return;

        ClimateController.Instance.StartClimateEvent(data);
        Debug.Log($"[ClimateDebugger] Evento iniciado: {data.DisplayName}");
    }

    /// <summary>
    /// Detiene el evento climático activo mediante reflexión interna.
    /// Llama al método privado EndCurrentEvent por la vía pública StopEvent.
    /// </summary>
    public void StopCurrentEvent()
    {
        if (!ClimateController.Instance.HasActiveEvent)
        {
            Debug.Log("[ClimateDebugger] No hay evento activo.");
            return;
        }

        // Forzamos el fin llamando al mismo método que usa el timer interno.
        // Lo hacemos iniciando un evento nulo para que EndCurrentEvent se ejecute.
        // La forma más limpia es exponer un método público en ClimateController.
        ClimateController.Instance.ForceStopEvent();
        Debug.Log("[ClimateDebugger] Evento detenido manualmente.");
    }
}