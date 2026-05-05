using System.Collections;
using UnityEngine;

/// <summary>
/// Objeto del entorno con animación por sprites en loop y colisión sólida.
/// La animación inicia en un frame aleatorio para evitar sincronización
/// entre instancias del mismo objeto en escena.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WorldObject : MonoBehaviour
{
    [Header("Animation")]

    [SerializeField, Tooltip("Sprites que conforman la animación en orden.")]
    private Sprite[] sprites;

    [SerializeField, Tooltip("Tiempo en segundos entre cada frame de animación.")]
    private float frameInterval = 0.12f;

    [SerializeField, Tooltip("Si está desactivado, la animación se congela en el frame actual.")]
    private bool isAnimating = true;

    // ── Estado interno ───────────────────────────────────────────────

    private SpriteRenderer spriteRenderer;
    private int            currentFrame;
    private Coroutine      animationRoutine;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[WorldObject] '{gameObject.name}' no tiene sprites asignados.", this);
            return;
        }

        // Frame inicial aleatorio para que cada instancia esté desincronizada
        currentFrame           = Random.Range(0, sprites.Length);
        spriteRenderer.sprite  = sprites[currentFrame];

        if (isAnimating)
            animationRoutine = StartCoroutine(AnimationLoop());
    }

    // ── Animación ────────────────────────────────────────────────────

    /// <summary>
    /// Loop de animación. Avanza al siguiente frame cada frameInterval segundos.
    /// Al llegar al último frame vuelve al índice 0, formando un ciclo infinito.
    /// WaitForSeconds es suficiente aquí ya que no requiere precisión de física.
    /// </summary>
    private IEnumerator AnimationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(frameInterval);

            currentFrame          = (currentFrame + 1) % sprites.Length;
            spriteRenderer.sprite = sprites[currentFrame];
        }
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Pausa o reanuda la animación sin perder el frame actual.
    /// </summary>
    public void SetAnimating(bool animate)
    {
        if (animate == isAnimating) return;

        isAnimating = animate;

        if (isAnimating)
        {
            animationRoutine = StartCoroutine(AnimationLoop());
        }
        else
        {
            if (animationRoutine != null)
                StopCoroutine(animationRoutine);
        }
    }
}