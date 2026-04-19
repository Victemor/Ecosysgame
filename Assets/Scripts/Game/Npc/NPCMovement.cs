using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el movimiento autónomo del NPC entre puntos.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class NPCMovement : MonoBehaviour
{
    [Header("Points")]

    [SerializeField] private Transform originPoint;
    [SerializeField] private List<Transform> patrolPoints;

    [Header("Movement")]

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.1f;

    [Header("Behavior")]

    [SerializeField, Tooltip("Tiempo entre decisiones.")]
    private float decisionInterval = 3f;

    [SerializeField, Tooltip("Probabilidad de moverse (0–1).")]
    private float moveProbability = 0.6f;

    private CharacterController controller;
    private Transform currentTarget;

    private bool isMoving;

    /// <summary>
    /// Dirección actual para animación.
    /// </summary>
    public Vector2 MovementDirection { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        StartCoroutine(BehaviorLoop());
    }

    private IEnumerator BehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(decisionInterval);

            if (isMoving)
                continue;

            if (Random.value <= moveProbability)
            {
                StartCoroutine(MoveRoutine());
            }
        }
    }

    private IEnumerator MoveRoutine()
    {
        isMoving = true;

        Transform target = GetRandomPoint();

        yield return MoveTo(target);

        yield return new WaitForSeconds(1f);

        yield return MoveTo(originPoint);

        isMoving = false;
        MovementDirection = Vector2.zero;
    }

    private IEnumerator MoveTo(Transform target)
    {
        currentTarget = target;

        while (Vector3.Distance(transform.position, target.position) > stoppingDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized;

            // Guardar dirección para animación
            MovementDirection = new Vector2(direction.x, direction.z);

            // Intentar mover
            if (!TryMove(direction))
            {
                // Bloqueado → esperar
                yield return null;
                continue;
            }

            yield return null;
        }
    }

    private bool TryMove(Vector3 direction)
    {
        Vector3 move = direction * moveSpeed * Time.deltaTime;

        CollisionFlags flags = controller.Move(move);

        // Si chocó adelante → bloqueado
        if ((flags & CollisionFlags.Sides) != 0)
        {
            return false;
        }

        return true;
    }

    private Transform GetRandomPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
            return originPoint;

        int index = Random.Range(0, patrolPoints.Count);
        return patrolPoints[index];
    }
}