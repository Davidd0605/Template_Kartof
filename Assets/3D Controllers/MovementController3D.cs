using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class that uses the InputSystem to move the runner(3D character).
/// To do this it uses the CharacterController component, which handles
/// non physics movement better than a rigidbody setup and deals with 
/// slopes and stairs automatically. 
/// 
/// Author: Damyan
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MovementController3D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpCutMultiplier = 2f;
    [SerializeField] private float gravity = -9.81f;
    [Header("Crouching")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float crouchCameraOffset = 0.5f;
    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField, Tooltip("In degrees")] private float maxCameraPitch = 80;

    [Header("Input values")]
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool jumpQueued;
    private bool jumpHeld;
    private bool crouchHeld;

    [Header("State")]
    private Vector3 velocity;
    private float defaultHeight;
    private Vector3 defaultCameraLocalPosition;
    private float cameraPitch = 0f;

    [Header("Directions")]
    private Vector3 forward;
    private Vector3 right;

    [Header("References")]
    [SerializeField] private Camera runnerCamera;
    [SerializeField] private LayerMask environmentMask;
    private CharacterController characterController;

    public bool releaseMouse = false;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        defaultHeight = characterController.height;
        defaultCameraLocalPosition = runnerCamera.transform.localPosition;

        if (releaseMouse)
        {
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
        }
    }

    private void Update()
    {
        CollectMovementDirections();

        ProcessHorizontalMovement();
        ProcessVerticalMovement();
        ProcessLook();
        ProcessCrouch();
    }

    #region Input System Callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && characterController.isGrounded)
        {
            jumpQueued = true;
            jumpHeld = true;
        }
        else if (context.canceled)
        {
            jumpHeld = false;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            crouchHeld = true;
        }
        else if (context.canceled)
        {
            if (CanStand()) crouchHeld = false;

        }
    }
    #endregion

    #region Processing Methods
    private void ProcessHorizontalMovement()
    {
        Vector3 move = forward * movementInput.y + right * movementInput.x;
        characterController.Move(move * speed * Time.deltaTime);
    }

    private void ProcessLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Horizontal rotation
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation 
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxCameraPitch, maxCameraPitch);

        runnerCamera.transform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }

    private void ProcessCrouch()
    {
        float targetHeight = crouchHeld ? crouchHeight : defaultHeight;
        float targetCenterY = targetHeight / 2f;

        characterController.height = targetHeight;
        characterController.center = new Vector3(0f, targetCenterY, 0f);

        runnerCamera.transform.localPosition = defaultCameraLocalPosition +
            (crouchHeld ? Vector3.down * crouchCameraOffset : Vector3.zero);
    }

    private void ProcessVerticalMovement()
    {
        // Stick to ground
        if (characterController.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // Start jump
        if (jumpQueued)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpQueued = false;
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Short hop if jump released early
        if (!jumpHeld && velocity.y > 0f)
        {
            velocity.y += gravity * (jumpCutMultiplier - 1f) * Time.deltaTime;
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    private void CollectMovementDirections()
    {
        forward = runnerCamera.transform.forward;
        right = runnerCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();
    }

    private bool CanStand()
    {
        Vector3 start = transform.position + Vector3.up * characterController.height / 2f;
        Vector3 end = transform.position + Vector3.up * defaultHeight - Vector3.up * 0.01f;
        float radius = characterController.radius - 0.05f;

        // True if space above is empty of objects in environmentMask
        return !Physics.CheckCapsule(start, end, radius, environmentMask);
    }

    #endregion
}
