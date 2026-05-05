using UnityEngine;

/// <summary>
/// Herramienta de prueba para el sistema de progreso.
/// Permite modificar valores en runtime desde el Inspector.
/// </summary>
public class ProgressDebugger : MonoBehaviour
{
    [Header("Ecopuntos")]

    [SerializeField, Tooltip("Cantidad de ecopuntos a añadir o restar.")]
    private int ecopuntosAmount = 50;

    [Header("Progreso Total")]

    [SerializeField, Tooltip("Valor de progreso total a establecer (0-100).")]
    private float progresoValue = 10f;

    public void AddEcopuntos()      => ProgressManager.Instance?.AddEcopuntos(ecopuntosAmount);
    public void SubtractEcopuntos() => ProgressManager.Instance?.AddEcopuntos(-ecopuntosAmount);
    public void SetProgreso()       => ProgressManager.Instance?.SetProgresoTotal(progresoValue);
    public void ResetProgress()     => ProgressManager.Instance?.ResetProgress();
    public void ForceSave()         => ProgressManager.Instance?.Save();
    public void ForceLoad()         => ProgressManager.Instance?.Load();
}