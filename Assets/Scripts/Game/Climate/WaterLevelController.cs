using UnityEngine;

/// <summary>
/// Controlador de nivel de agua.
/// Actualmente solo registra cambios por debug.
/// </summary>
public class WaterLevelController : MonoBehaviour
{
    private void OnEnable()
    {
        ClimateController.Instance.OnClimateEventStarted += HandleEventStarted;
    }

    private void OnDisable()
    {
        ClimateController.Instance.OnClimateEventStarted -= HandleEventStarted;
    }

    /// <summary>
    /// Simula cambio de nivel de agua.
    /// </summary>
    private void HandleEventStarted(ClimateEventData eventData)
    {
        if (eventData == null)
            return;

        float waterModifier = eventData.Effect.WaterLevelModifier;

        if (waterModifier > 0)
        {
            Debug.Log("El nivel del agua está subiendo.");
        }
        else if (waterModifier < 0)
        {
            Debug.Log("El nivel del agua está bajando.");
        }
    }
}