using UnityEngine;

/// <summary>
/// Herramienta de prueba para el sistema de vida en runtime.
/// Permite quitar y recuperar vida desde el Inspector sin conectar enemigos.
/// </summary>
public class HealthDebugger : MonoBehaviour
{
    [SerializeField, Tooltip("Referencia al PlayerHealth a modificar.")]
    private PlayerHealth playerHealth;

    [SerializeField, Tooltip("Cantidad de vida a quitar o recuperar al presionar el botón.")]
    private int cantidad = 1;

    public void QuitarVida()  => playerHealth?.TakeDamage(cantidad);
    public void RecuperarVida() => playerHealth?.Heal(cantidad);
    public void ResetearVida()  => playerHealth?.SetVida(playerHealth.VidaMax);
}