using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja animaciones del jugador usando sprites (sin Animator).
/// Soporta múltiples frames por dirección en loop.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    [Header("References")]

    [SerializeField, Tooltip("Referencia al movimiento del jugador.")]
    private PlayerMovement playerMovement;

    [Header("Animation Frames")]

    [SerializeField] private List<Sprite> upSprites;
    [SerializeField] private List<Sprite> downSprites;
    [SerializeField] private List<Sprite> leftSprites;
    [SerializeField] private List<Sprite> rightSprites;

    [Header("Settings")]

    [SerializeField, Tooltip("Frames por segundo de la animación.")]
    private float frameRate = 8f;

    private SpriteRenderer spriteRenderer;

    private float timer;
    private int currentFrame;

    private Direction currentDirection = Direction.Down;
    private Direction lastDirection = Direction.Down;

    private Direction previousDirection;
    private float directionChangeCooldown = 0.05f;
    private float directionTimer;

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        UpdateDirection();
        UpdateAnimation();
    }

    /// <summary>
    /// Determina la dirección según input.
    /// </summary>
    private void UpdateDirection()
    {
        Vector2 input = playerMovement.CurrentInput;

        if (input.sqrMagnitude <= 0.01f)
        {
            currentDirection = lastDirection;
            return;
        }

        Direction newDirection;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            newDirection = input.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            newDirection = input.y > 0 ? Direction.Up : Direction.Down;
        }

        // 🔥 DETECTAR CAMBIO DE DIRECCIÓN
        if (newDirection != currentDirection)
        {
            directionTimer += Time.deltaTime;

            if (directionTimer >= directionChangeCooldown)
            {
                previousDirection = currentDirection;
                currentDirection = newDirection;
                lastDirection = currentDirection;

                ResetAnimation(); // 🔥 CLAVE
                directionTimer = 0f;
            }
        }
        else
        {
            directionTimer = 0f;
        }
    }

    private void UpdateAnimation()
    {
        List<Sprite> currentList = GetCurrentSpriteList();

        if (currentList == null || currentList.Count == 0)
            return;

        bool isMoving = playerMovement.CurrentInput.sqrMagnitude > 0.01f;

        if (!isMoving)
        {
            spriteRenderer.sprite = currentList[0];
            return;
        }

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % currentList.Count;
            spriteRenderer.sprite = currentList[currentFrame];
        }
    }

    /// <summary>
    /// Obtiene la lista de sprites según dirección actual.
    /// </summary>
    private List<Sprite> GetCurrentSpriteList()
    {
        switch (currentDirection)
        {
            case Direction.Up:
                return upSprites;
            case Direction.Down:
                return downSprites;
            case Direction.Left:
                return leftSprites;
            case Direction.Right:
                return rightSprites;
        }

        return null;
    }

    /// <summary>
    /// Reinicia la animación al cambiar de dirección.
    /// </summary>
    private void ResetAnimation()
    {
        currentFrame = 0;
        timer = 0f;

        List<Sprite> list = GetCurrentSpriteList();

        if (list != null && list.Count > 0)
        {
            spriteRenderer.sprite = list[0];
        }
    }
}