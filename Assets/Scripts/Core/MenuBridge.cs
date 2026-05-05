using UnityEngine;

/// <summary>
/// Proxy delgado para los botones del menú que necesitan llamar sistemas DDOL.
/// Vive en la escena del menú (no es DDOL) y siempre llama ProgressManager.Instance,
/// garantizando que cada click llegue al singleton correcto sin importar
/// cuántas veces se haya recargado la escena.
/// </summary>
public class MenuBridge : MonoBehaviour
{
    /// <summary>
    /// Reinicia todo el progreso del juego.
    /// Llamado por el botón "Reiniciar Progreso" en el Inspector.
    /// </summary>
    public void ResetProgress()
    {
        ProgressManager.Instance?.ResetProgress();
    }
}