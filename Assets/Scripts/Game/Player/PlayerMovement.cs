using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Maneja el movimiento del jugador con Rigidbody puro.
/// Usa linearVelocity en lugar de MovePosition para que las
/// colisiones con colliders funcionen correctamente en todas las direcciones.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField, Tooltip("Velocidad de movimiento horizontal en unidades/segundo.")]
    private float moveSpeed = 5f;

    [SerializeField, Tooltip("Referencia a la cámara para calcular dirección relativa.")]
    private Transform cameraTransform;

    private Rigidbody rb;
    private InputAction moveAction;
    private Vector2 inputVector;

    public Vector2 CurrentInput => inputVector;
    public bool IsMoving        => inputVector.sqrMagnitude > 0.01f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic            = false;
        rb.useGravity             = true;
        rb.freezeRotation         = true;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        moveAction = new InputAction(name: "Move", type: InputActionType.Value);

        moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/upArrow")
            .With("Down",  "<Keyboard>/downArrow")
            .With("Left",  "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
    }

    private void OnEnable()  => moveAction.Enable();
    private void OnDisable() => moveAction.Disable();

    private void Update()
    {
        inputVector = moveAction.ReadValue<Vector2>().normalized;
    }

    /// <summary>
    /// Reemplaza solo la velocidad horizontal y preserva la Y (gravedad).
    /// A diferencia de MovePosition, velocity respeta los colliders
    /// en todas las direcciones correctamente.
    /// </summary>
    private void FixedUpdate()
    {
        Vector3 horizontal = CalculateHorizontalMovement();

        rb.linearVelocity = new Vector3(
            horizontal.x,
            rb.linearVelocity.y,
            horizontal.z
        );
    }

    private Vector3 CalculateHorizontalMovement()
    {
        if (inputVector.sqrMagnitude <= 0.01f)
            return Vector3.zero;

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right   = cameraTransform != null ? cameraTransform.right   : Vector3.right;

        forward.y = 0f;
        right.y   = 0f;

        forward.Normalize();
        right.Normalize();

        return (forward * inputVector.y + right * inputVector.x) * moveSpeed;
    }
}