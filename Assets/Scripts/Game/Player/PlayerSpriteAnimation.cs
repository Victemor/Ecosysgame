using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja animaciones del jugador usando sprites (sin Animator).
/// Soporta múltiples frames por dirección en loop.
/// Busca PlayerMovement automáticamente en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    [Header("Animation Frames")]

    [SerializeField, Tooltip("Sprites para movimiento hacia arriba.")]
    private List<Sprite> upSprites;

    [SerializeField, Tooltip("Sprites para movimiento hacia abajo.")]
    private List<Sprite> downSprites;

    [SerializeField, Tooltip("Sprites para movimiento hacia la izquierda.")]
    private List<Sprite> leftSprites;

    [SerializeField, Tooltip("Sprites para movimiento hacia la derecha.")]
    private List<Sprite> rightSprites;

    [Header("Settings")]

    [SerializeField, Tooltip("Frames por segundo de la animación.")]
    private float frameRate = 8f;

    [SerializeField, Tooltip("Tiempo mínimo antes de aceptar un cambio de dirección. Evita flickering.")]
    private float directionChangeCooldown = 0.05f;

    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;

    private float timer;
    private int currentFrame;

    private Direction currentDirection = Direction.Down;
    private Direction lastDirection    = Direction.Down;
    private float directionTimer;

    private enum Direction { Up, Down, Left, Right }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Búsqueda automática: evita arrastrar referencia al Inspector.
        // Se busca en el mismo GameObject ya que animador y movimiento viven juntos.
        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
            Debug.LogError($"[PlayerSpriteAnimator] No se encontró PlayerMovement en '{gameObject.name}'.", this);
    }

    private void Update()
    {
        UpdateDirection();
        UpdateAnimation();
    }

    /// <summary>
    /// Determina la dirección de movimiento desde el input.
    /// Aplica un cooldown para evitar cambios de frame rápidos al moverse diagonalmente.
    /// </summary>
    private void UpdateDirection()
    {
        Vector2 input = playerMovement.CurrentInput;

        if (input.sqrMagnitude <= 0.01f)
        {
            currentDirection = lastDirection;
            return;
        }

        Direction newDirection = Mathf.Abs(input.x) > Mathf.Abs(input.y)
            ? (input.x > 0 ? Direction.Right : Direction.Left)
            : (input.y > 0 ? Direction.Up    : Direction.Down);

        if (newDirection != currentDirection)
        {
            directionTimer += Time.deltaTime;

            if (directionTimer >= directionChangeCooldown)
            {
                currentDirection = newDirection;
                lastDirection    = currentDirection;
                ResetAnimation();
                directionTimer   = 0f;
            }
        }
        else
        {
            directionTimer = 0f;
        }
    }

    /// <summary>
    /// Avanza el frame de animación según frameRate.
    /// Muestra el frame 0 cuando el jugador está quieto (idle).
    /// </summary>
    private void UpdateAnimation()
    {
        List<Sprite> currentList = GetCurrentSpriteList();

        if (currentList == null || currentList.Count == 0)
            return;

        if (!playerMovement.IsMoving)
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

    private List<Sprite> GetCurrentSpriteList()
    {
        return currentDirection switch
        {
            Direction.Up    => upSprites,
            Direction.Down  => downSprites,
            Direction.Left  => leftSprites,
            Direction.Right => rightSprites,
            _               => null
        };
    }

    private void ResetAnimation()
    {
        currentFrame = 0;
        timer        = 0f;

        List<Sprite> list = GetCurrentSpriteList();

        if (list != null && list.Count > 0)
            spriteRenderer.sprite = list[0];
    }
}