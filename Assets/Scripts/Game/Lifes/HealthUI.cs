using UnityEngine;

/// <summary>
/// Controla el grupo de corazones en pantalla.
/// Se suscribe a PlayerHealth.OnVidaChanged y distribuye
/// los fragmentos entre los HeartDisplay disponibles.
/// El corazón izquierdo es el último en vaciarse.
/// </summary>
public class HealthUI : MonoBehaviour
{
    private const int FragmentosPorCorazon = 3;

    [Header("References")]

    [SerializeField, Tooltip("Referencia al componente de vida del jugador.")]
    private PlayerHealth playerHealth;

    [SerializeField, Tooltip("Corazones en orden de izquierda a derecha.")]
    private HeartDisplay[] corazones;

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            Debug.LogError("[HealthUI] PlayerHealth no asignado.", this);
            return;
        }

        playerHealth.OnVidaChanged += ActualizarUI;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnVidaChanged -= ActualizarUI;
    }

    private void Start()
    {
        // Inicializar UI con la vida actual al arrancar
        ActualizarUI(playerHealth.VidaActual);
    }

    /// <summary>
    /// Distribuye la vida en fragmentos para cada corazón.
    /// El corazón en índice 0 (izquierda) representa la vida más alta.
    /// El daño vacía primero el corazón derecho (mayor índice).
    /// </summary>
    private void ActualizarUI(int vidaActual)
    {
        for (int i = 0; i < corazones.Length; i++)
        {
            // Cada corazón representa un bloque de 3 fragmentos
            // El izquierdo (i=0) consume vida 7-9, el derecho consume vida 1-3
            int fragmentos = Mathf.Clamp(vidaActual - i * FragmentosPorCorazon, 0, FragmentosPorCorazon);
            corazones[i].SetFragmentos(fragmentos);
        }
    }
}