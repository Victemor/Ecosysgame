using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el movimiento autónomo del NPC con Rigidbody puro.
/// Usa linearVelocity para respetar la gravedad y las colisiones.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class NPCMovement : MonoBehaviour
{
    [Header("Points")]

    [SerializeField, Tooltip("Punto de origen al que el NPC regresa tras patrullar.")]
    private Transform originPoint;

    [SerializeField, Tooltip("Lista de puntos de patrulla.")]
    private List<Transform> patrolPoints;

    [Header("Movement")]

    [SerializeField, Tooltip("Velocidad de desplazamiento horizontal en unidades/segundo.")]
    private float moveSpeed = 2f;

    [SerializeField, Tooltip("Distancia horizontal mínima para considerar que llegó al destino.")]
    private float stoppingDistance = 0.2f;

    [Header("Behavior")]

    [SerializeField, Tooltip("Tiempo en segundos entre decisiones de movimiento.")]
    private float decisionInterval = 3f;

    [SerializeField, Tooltip("Probabilidad de moverse en cada ciclo (0 = nunca, 1 = siempre).")]
    [Range(0f, 1f)]
    private float moveProbability = 0.6f;

    private Rigidbody rb;
    private bool isMoving;

    public Vector2 MovementDirection { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Kinematic = false para que la gravedad funcione
        rb.isKinematic            = false;
        rb.useGravity             = true;
        rb.freezeRotation         = true;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
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

            if (isMoving) continue;

            if (Random.value <= moveProbability)
                StartCoroutine(MoveRoutine());
        }
    }

    private IEnumerator MoveRoutine()
    {
        isMoving = true;

        Transform target = GetRandomPoint();
        yield return MoveTo(target);

        yield return new WaitForSeconds(1f);
        yield return MoveTo(originPoint);

        isMoving          = false;
        MovementDirection = Vector2.zero;

        // Detener movimiento horizontal, preservar Y (gravedad)
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    /// <summary>
    /// Mueve el NPC hacia el destino usando velocity horizontal.
    /// La Y del Rigidbody no se toca para que la gravedad funcione.
    /// </summary>
    private IEnumerator MoveTo(Transform target)
    {
        while (true)
        {
            Vector3 selfFlat   = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 targetFlat = new Vector3(target.position.x,    0f, target.position.z);

            if (Vector3.Distance(selfFlat, targetFlat) <= stoppingDistance)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                break;
            }

            Vector3 direction = (targetFlat - selfFlat).normalized;
            MovementDirection = new Vector2(direction.x, direction.z);

            // Solo velocidad horizontal — Y la gestiona el Rigidbody
            rb.linearVelocity = new Vector3(
                direction.x * moveSpeed,
                rb.linearVelocity.y,
                direction.z * moveSpeed
            );

            yield return new WaitForFixedUpdate();
        }
    }

    private Transform GetRandomPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
            return originPoint;

        return patrolPoints[Random.Range(0, patrolPoints.Count)];
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null) return;

        Gizmos.color = Color.cyan;
        foreach (Transform point in patrolPoints)
            if (point != null) Gizmos.DrawWireSphere(point.position, 0.2f);

        if (originPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(originPoint.position, 0.25f);
        }
    }
}