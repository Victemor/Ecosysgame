using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Maneja el movimiento del jugador usando Input System configurado por código.
/// Incluye gravedad para terrenos inclinados o con desniveles.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField, Tooltip("Velocidad de movimiento horizontal.")]
    private float moveSpeed = 5f;

    [SerializeField, Tooltip("Referencia a la cámara para calcular dirección relativa.")]
    private Transform cameraTransform;

    [Header("Gravity")]

    [SerializeField, Tooltip("Fuerza de gravedad aplicada al jugador. Usa el valor de Physics.gravity.y por defecto.")]
    private float gravityScale = 1f;

    private CharacterController characterController;
    private InputAction moveAction;

    private Vector2 inputVector;
    private float verticalVelocity;

    /// <summary>
    /// Input actual expuesto para otros sistemas (animación, interacción).
    /// </summary>
    public Vector2 CurrentInput => inputVector;

    /// <summary>
    /// Indica si el jugador está actualmente en movimiento.
    /// </summary>
    public bool IsMoving => inputVector.sqrMagnitude > 0.01f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        moveAction = new InputAction(
            name: "Move",
            type: InputActionType.Value
        );

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
        ReadInput();
        ApplyGravity();
        HandleMovement();
    }

    /// <summary>
    /// Lee el vector de input normalizado.
    /// </summary>
    private void ReadInput()
    {
        inputVector = moveAction.ReadValue<Vector2>().normalized;
    }

    /// <summary>
    /// Acumula velocidad vertical cuando el jugador está en el aire.
    /// Resetea al tocar el suelo para evitar acumulación infinita.
    /// </summary>
    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            // Pequeño valor negativo constante para mantener el jugador pegado a rampas
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;
        }
    }

    /// <summary>
    /// Aplica movimiento horizontal relativo a la cámara + velocidad vertical acumulada.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (inputVector.sqrMagnitude > 0.01f)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right   = cameraTransform.right;

            forward.y = 0f;
            right.y   = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection = forward * inputVector.y + right * inputVector.x;
        }

        moveDirection.y = verticalVelocity;

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}