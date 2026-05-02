using UnityEngine;

/// <summary>
/// Muestra u oculta un panel estático.
/// Usado por los botones de Configuración, Opciones e Información.
/// </summary>
public class StaticPanelToggle : MonoBehaviour
{
    [SerializeField, Tooltip("Panel que se activa/desactiva.")]
    private GameObject panel;

    /// <summary>Muestra el panel.</summary>
    public void Show() => panel?.SetActive(true);

    /// <summary>Oculta el panel.</summary>
    public void Hide() => panel?.SetActive(false);

    /// <summary>Alterna entre mostrar y ocultar.</summary>
    public void Toggle() => panel?.SetActive(!panel.activeSelf);
}