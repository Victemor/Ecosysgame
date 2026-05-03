using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reproduce una secuencia de sprites en un Image de UI sin loop.
/// Soporta dos animaciones independientes (A y B).
/// Expone IsPlaying para que BitacoraController espere a que termine.
/// </summary>
[RequireComponent(typeof(Image))]
public class SpriteSequenceAnimator : MonoBehaviour
{
    [Header("Animation A — izquierda")]

    [SerializeField, Tooltip("Sprites de la animación A en orden.")]
    private Sprite[] spritesA;

    [Header("Animation B — derecha")]

    [SerializeField, Tooltip("Sprites de la animación B en orden.")]
    private Sprite[] spritesB;

    [Header("Settings")]

    [SerializeField, Tooltip("Tiempo entre frames en segundos.")]
    private float frameInterval = 0.08f;

    private Image     image;
    private Coroutine currentAnimation;

    /// <summary>True mientras hay una animación reproduciéndose.</summary>
    public bool IsPlaying => currentAnimation != null;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void PlayAnimationA() => PlaySequence(spritesA);
    public void PlayAnimationB() => PlaySequence(spritesB);

    public void StopAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }

    private void PlaySequence(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("[SpriteSequenceAnimator] Lista de sprites vacía.", this);
            return;
        }

        StopAnimation();
        currentAnimation = StartCoroutine(SequenceRoutine(sprites));
    }

    /// <summary>
    /// Reproduce los sprites en orden y se detiene al llegar al último.
    /// No hace loop — inicio y fin definidos.
    /// </summary>
    private IEnumerator SequenceRoutine(Sprite[] sprites)
    {
        foreach (Sprite sprite in sprites)
        {
            if (sprite != null)
                image.sprite = sprite;

            yield return new WaitForSeconds(frameInterval);
        }

        currentAnimation = null;
    }
}