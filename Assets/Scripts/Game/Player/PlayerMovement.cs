using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Maneja el movimiento del jugador usando Input System configurado por código.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField, Tooltip("Velocidad de movimiento.")]
    private float moveSpeed = 5f;

    [SerializeField, Tooltip("Referencia a la cámara.")]
    private Transform cameraTransform;

    private CharacterController characterController;

    private InputAction moveAction;

    private Vector2 inputVector;

    /// <summary>
    /// Input actual para otros sistemas (animación).
    /// </summary>
    public Vector2 CurrentInput => inputVector;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        // Crear acción por código
        moveAction = new InputAction(
            name: "Move",
            type: InputActionType.Value
        );

        // WASD
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Flechas
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void Update()
    {
        ReadInput();
        HandleMovement();
    }

    /// <summary>
    /// Lee el input desde el Input System.
    /// </summary>
    private void ReadInput()
    {
        inputVector = moveAction.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
    }

    /// <summary>
    /// Aplica movimiento en el mundo.
    /// </summary>
    private void HandleMovement()
    {
        if (inputVector.sqrMagnitude <= 0.01f)
            return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * inputVector.y + right * inputVector.x;

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}