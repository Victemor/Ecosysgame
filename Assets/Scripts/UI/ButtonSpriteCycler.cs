using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cicla a través de una lista de sprites cada vez que se presiona el botón.
/// El sprite se aplica sobre la Image del propio botón.
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ButtonSpriteCycler : MonoBehaviour
{
    [SerializeField, Tooltip("Lista de sprites a ciclar. El primero se aplica al inicio.")]
    private Sprite[] sprites;

    private Image  buttonImage;
    private int    currentIndex;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(CycleSprite);

        if (sprites != null && sprites.Length > 0)
            buttonImage.sprite = sprites[0];
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(CycleSprite);
    }

    /// <summary>
    /// Avanza al siguiente sprite de la lista.
    /// Al llegar al final vuelve al primero.
    /// </summary>
    private void CycleSprite()
    {
        if (sprites == null || sprites.Length == 0) return;

        currentIndex = (currentIndex + 1) % sprites.Length;
        buttonImage.sprite = sprites[currentIndex];
    }
}