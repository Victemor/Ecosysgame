using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa un corazón individual en la UI.
/// Recibe la cantidad de fragmentos activos y actualiza su sprite + animación.
/// No tiene lógica de vida: solo presentación.
/// </summary>
public class HeartDisplay : MonoBehaviour
{
    [Header("Sprites — asignar en orden: vacío, 1, 2, lleno")]

    [SerializeField, Tooltip("Sprite cuando el corazón no tiene fragmentos.")]
    private Sprite spriteVacio;

    [SerializeField, Tooltip("Sprite con 1 fragmento activo.")]
    private Sprite spriteUnFragmento;

    [SerializeField, Tooltip("Sprite con 2 fragmentos activos.")]
    private Sprite spriteDosFragmentos;

    [SerializeField, Tooltip("Sprite con 3 fragmentos activos (lleno).")]
    private Sprite spriteLleno;

    [Header("Animación DOTween")]

    [SerializeField, Tooltip("Escala máxima del punch al cambiar de estado.")]
    private float punchStrength = 0.3f;

    [SerializeField, Tooltip("Duración de la animación de punch.")]
    private float punchDuration = 0.25f;

    private Image image;
    private int fragmentosActuales = 3;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    /// <summary>
    /// Actualiza el estado visual del corazón.
    /// Solo anima si el número de fragmentos cambió realmente.
    /// </summary>
    public void SetFragmentos(int fragmentos)
    {
        fragmentos = Mathf.Clamp(fragmentos, 0, 3);

        bool cambio = fragmentos != fragmentosActuales;
        fragmentosActuales = fragmentos;

        image.sprite = GetSprite(fragmentos);

        if (cambio)
            AnimarCambio();
    }

    private Sprite GetSprite(int fragmentos) => fragmentos switch
    {
        0 => spriteVacio,
        1 => spriteUnFragmento,
        2 => spriteDosFragmentos,
        _ => spriteLleno
    };

    /// <summary>
    /// Punch scale: el corazón se agranda y vuelve a su tamaño original.
    /// Se mata la animación anterior para evitar conflictos si llegan cambios rápidos.
    /// </summary>
    private void AnimarCambio()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(
            punch:     Vector3.one * punchStrength,
            duration:  punchDuration,
            vibrato:   1,
            elasticity: 0.5f
        );
    }
}