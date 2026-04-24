using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el movimiento autónomo del NPC entre puntos de patrulla.
/// Aplica gravedad para mantener al NPC en terrenos con desniveles,
/// consistente con el comportamiento de PlayerMovement.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class NPCMovement : MonoBehaviour
{
    [Header("Points")]

    [SerializeField, Tooltip("Punto de origen al que el NPC regresa tras patrullar.")]
    private Transform originPoint;

    [SerializeField, Tooltip("Lista de puntos de patrulla a los que puede desplazarse.")]
    private List<Transform> patrolPoints;

    [Header("Movement")]

    [SerializeField, Tooltip("Velocidad de desplazamiento horizontal en unidades/segundo.")]
    private float moveSpeed = 2f;

    [SerializeField, Tooltip("Distancia mínima al destino para considerar que llegó.")]
    private float stoppingDistance = 0.1f;

    [Header("Gravity")]

    [SerializeField, Tooltip("Escala de gravedad aplicada al NPC. Ajusta si flota o cae demasiado rápido.")]
    private float gravityScale = 1f;

    [Header("Behavior")]

    [SerializeField, Tooltip("Tiempo en segundos entre decisiones de movimiento.")]
    private float decisionInterval = 3f;

    [SerializeField, Tooltip("Probabilidad de moverse al siguiente ciclo de decisión (0 = nunca, 1 = siempre).")]
    private float moveProbability = 0.6f;

    private CharacterController controller;
    private bool   isMoving;
    private float  verticalVelocity;

    /// <summary>
    /// Dirección 2D de movimiento actual. Leída por NPCSpriteAnimator para animación.
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

    /// <summary>
    /// Loop principal de comportamiento. Decide periódicamente si el NPC se mueve.
    /// </summary>
    private IEnumerator BehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(decisionInterval);

            if (isMoving)
                continue;

            if (Random.value <= moveProbability)
                StartCoroutine(MoveRoutine());
        }
    }

    /// <summary>
    /// Rutina completa de movimiento: va a un punto aleatorio y regresa al origen.
    /// </summary>
    private IEnumerator MoveRoutine()
    {
        isMoving = true;

        Transform target = GetRandomPoint();

        yield return MoveTo(target);
        yield return new WaitForSeconds(1f);
        yield return MoveTo(originPoint);

        isMoving          = false;
        MovementDirection = Vector2.zero;
    }

    /// <summary>
    /// Mueve el NPC hacia un Transform destino aplicando gravedad en cada frame.
    /// </summary>
    private IEnumerator MoveTo(Transform target)
    {
        while (Vector3.Distance(transform.position, target.position) > stoppingDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized;

            // Exponer dirección horizontal para el animador
            MovementDirection = new Vector2(direction.x, direction.z);

            ApplyGravity();

            Vector3 horizontalMove = direction * moveSpeed * Time.deltaTime;
            horizontalMove.y = verticalVelocity * Time.deltaTime;

            CollisionFlags flags = controller.Move(horizontalMove);

            // Si chocó lateralmente, esperar un frame antes de continuar
            if ((flags & CollisionFlags.Sides) != 0)
            {
                yield return null;
                continue;
            }

            yield return null;
        }

        // Asegurar velocidad vertical limpia al llegar al destino
        if (controller.isGrounded)
            verticalVelocity = -2f;
    }

    /// <summary>
    /// Acumula velocidad vertical cuando el NPC está en el aire.
    /// Resetea a un valor negativo pequeño al tocar el suelo para pegarlo al terreno.
    /// Espeja la lógica de PlayerMovement para consistencia entre personajes.
    /// </summary>
    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;
        }
    }

    /// <summary>
    /// Devuelve un punto de patrulla aleatorio, o el origen si la lista está vacía.
    /// </summary>
    private Transform GetRandomPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
            return originPoint;

        int index = Random.Range(0, patrolPoints.Count);
        return patrolPoints[index];
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null)
            return;

        Gizmos.color = Color.cyan;

        foreach (Transform point in patrolPoints)
        {
            if (point != null)
                Gizmos.DrawWireSphere(point.position, 0.2f);
        }

        if (originPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(originPoint.position, 0.25f);
        }
    }
}