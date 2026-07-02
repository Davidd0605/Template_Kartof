using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class MovementController2DIsometric : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Top speed the character can reach on a single axis.")]
    [SerializeField] private float maxSpeed = 8f;
    [Tooltip("How quickly the character ramps up to max speed (units/s�).")]
    [SerializeField] private float acceleration = 100f;
    [Tooltip("How quickly the character brakes when no input is given (units/s�).")]
    [SerializeField] private float deceleration = 120f;
    [Tooltip("Extra speed added when moving diagonally.")]
    [SerializeField] private float diagonalBonusSpeed = 1.5f;
    [Tooltip("Multiplier applied to deceleration when the player suddenly reverses direction.")]
    [SerializeField] private float turnFrictionMultiplier = 2f;

    private Vector3 movementInput;
    private Vector3 velocity;
    private Rigidbody rb;

    private float flipThreshold = 0.01f;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        anim = GetComponent<Animator>(); 
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = new Vector3(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y);
    }

    private void FixedUpdate()
    {
        ProcessMovement();
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void ProcessMovement()
    {
        Vector3 inputDir = movementInput;
        bool isDiagonal = Mathf.Abs(inputDir.x) > 0.01f && Mathf.Abs(inputDir.y) > 0.01f;

        if (isDiagonal)
            inputDir = inputDir.normalized;

        Vector3 targetVelocity = inputDir * maxSpeed;

        if (isDiagonal)
            targetVelocity += inputDir * diagonalBonusSpeed;

        velocity.x = ProcessAxis(velocity.x, targetVelocity.x);
        velocity.z = ProcessAxis(velocity.z, targetVelocity.z);

        FlipSprite();

    }

    private float ProcessAxis(float current, float target)
    {
        bool hasInput = Mathf.Abs(target) > 0.01f;
        if (hasInput)
        {
            bool reversing = Mathf.Sign(current) != Mathf.Sign(target) && Mathf.Abs(current) > 0.1f;
            float rate = reversing ? deceleration * turnFrictionMultiplier : acceleration;
            return Mathf.MoveTowards(current, target, rate * Time.fixedDeltaTime);
        }
        else
        {
            return Mathf.MoveTowards(current, 0f, deceleration * Time.fixedDeltaTime);
        }
    }

    private void FlipSprite()
    {
        if (movementInput.x > flipThreshold)
        {
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
        }
        else if (movementInput.x < -flipThreshold)
        {
            transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
        }
    }
}
