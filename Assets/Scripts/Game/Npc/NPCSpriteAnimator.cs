using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja animación del NPC basada en dirección automática.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class NPCSpriteAnimator : MonoBehaviour
{
    [SerializeField] private NPCMovement movement;

    [Header("Sprites")]

    [SerializeField] private List<Sprite> upSprites;
    [SerializeField] private List<Sprite> downSprites;
    [SerializeField] private List<Sprite> leftSprites;
    [SerializeField] private List<Sprite> rightSprites;

    [SerializeField] private Sprite idleSprite;

    [SerializeField] private float frameRate = 6f;

    private SpriteRenderer spriteRenderer;

    private float timer;
    private int frame;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<NPCMovement>();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        Vector2 dir = movement.MovementDirection;

        if (dir.sqrMagnitude <= 0.01f)
        {
            spriteRenderer.sprite = idleSprite;
            return;
        }

        List<Sprite> list = GetDirectionSprites(dir);

        if (list == null || list.Count == 0)
            return;

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            frame = (frame + 1) % list.Count;
            spriteRenderer.sprite = list[frame];
        }
    }

    private List<Sprite> GetDirectionSprites(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? rightSprites : leftSprites;
        }
        else
        {
            return dir.y > 0 ? upSprites : downSprites;
        }
    }
}