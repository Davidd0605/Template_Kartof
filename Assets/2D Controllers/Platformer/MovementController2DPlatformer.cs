using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// 2D movement controller using InputSystem and CollisionHandler2D.
/// Mimics the naming and structure of Movement3D, adapted for 2D.
/// Supports acceleration, jump buffering, coyote time, apex modifiers,
/// and edge catching.
/// 
/// Author: Damyan
/// 
/// Extended by Lars to include audio
/// </summary>
[RequireComponent(typeof(CollisionHandler2D))]
public class MovementController2DPlatformer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float maxSpeed = 8f;
    [SerializeField] protected float acceleration = 100f;
    [SerializeField] protected float deceleration = 120f;
    [SerializeField] protected float apexBonusSpeed = 2f;


    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float timeToApex = 0.4f;
    [SerializeField] protected float jumpCutMultiplier = 2f;
    [SerializeField] protected float coyoteTime = 0.15f;
    [SerializeField] protected float jumpBufferTime = 0.15f;
    [SerializeField] protected float apexAntiGravity = 0.5f;
    [SerializeField] protected float maxFallSpeed = -20f;


    [Header("Edge Catching")]
    [SerializeField] private float edgeCatchDistance = 0.2f;
    [SerializeField] private float edgeCatchHeight = 0.15f;


    [Header("Input values")]
    protected Vector2 movementInput;
    protected bool jumpHeld;
    protected bool jumpQueued;


    [Header("State")]
    protected Vector2 velocity;
    protected float coyoteTimer;
    protected float jumpBufferTimer;
    protected bool hasJumped;

    [Header("Physics")]
    protected float gravity;
    protected float jumpVelocity;


    [Header("References")]
    protected CollisionHandler2D collisionHandler;
    [SerializeField] private LayerMask collisionMask;
    private BoxCollider2D boxCollider;
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;

    protected virtual void Awake()
    {
        collisionHandler = GetComponent<CollisionHandler2D>();
        boxCollider = GetComponent<BoxCollider2D>();


        gravity = -2f * jumpHeight / (timeToApex * timeToApex);
        jumpVelocity = Mathf.Abs(gravity) * timeToApex;
    }


    private void Start()
    {
        moveAction = inputActions.FindAction("Move", throwIfNotFound: true);
        jumpAction = inputActions.FindAction("Jump", throwIfNotFound: true);


        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        jumpAction.performed += OnJump;
        jumpAction.canceled += OnJump;


        inputActions.FindActionMap("Player", throwIfNotFound: true).Enable();
    }

    private void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.canceled -= OnJump;
        }
        inputActions?.FindActionMap("Player")?.Disable();
    }

    protected virtual void Update()
    {

        UpdateTimers();
        ProcessHorizontalMovement();
        ProcessJump();
        ApplyGravity();

        // Move character
        collisionHandler.Move(velocity * Time.deltaTime);
        PostCollisionVelocityAdjust();

        // Edge catching only after jump
        HandleEdgeCatching();
    }


    #region Input Callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferTimer = jumpBufferTime;
            jumpHeld = true;
            jumpQueued = true;
        }
        else if (context.canceled)
        {
            jumpHeld = false;
        }
    }
    #endregion


    #region Horizontal Movement
    protected virtual void ProcessHorizontalMovement()
    {
        float targetSpeed = movementInput.x * maxSpeed;


        if (Mathf.Abs(targetSpeed) > 0.01f)
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.deltaTime);
        else
            velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);

        if (!collisionHandler.collisions.below && Mathf.Abs(velocity.y) < 1f && Mathf.Abs(velocity.x) > 0.01f)
            velocity.x += Mathf.Sign(velocity.x) * apexBonusSpeed * Time.deltaTime;
    }
    #endregion


    #region Jumping
    protected virtual void UpdateTimers()
    {
        if (collisionHandler.collisions.below)
        {
            coyoteTimer = coyoteTime;
            hasJumped = false;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }


        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;
    }


    protected virtual void ProcessJump()
    {
        if ((jumpBufferTimer > 0 || jumpQueued) && coyoteTimer > 0)
        {
            velocity.y = jumpVelocity;
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            jumpQueued = false;
            hasJumped = true;
        }


        if (!jumpHeld && velocity.y > 0)
            velocity.y += gravity * (jumpCutMultiplier - 1f) * Time.deltaTime;
    }
    #endregion


    #region Gravity & Collision
    protected virtual void ApplyGravity()
    {
        if (!collisionHandler.collisions.below)
        {
            float g = gravity;
            if (velocity.y > 0 && Mathf.Abs(velocity.y) < 1f)
                g *= apexAntiGravity;


            velocity.y += g * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
        }
        else
            velocity.y = Mathf.Max(velocity.y, 0);
    }


    protected virtual void PostCollisionVelocityAdjust()
    {
        if (collisionHandler.collisions.above && velocity.y > 0)
            velocity.y = 0;
    }
    #endregion


    #region Edge Catching


    protected void HandleEdgeCatching()
    {
        if (!hasJumped || velocity.y >= 0 || collisionHandler.collisions.below) return;


        Vector2 origin = (Vector2)transform.position;
        origin.y -= boxCollider.size.y / 2f; // feet level
        Vector2 dir = Vector2.right * Mathf.Sign(velocity.x);


        RaycastHit2D hit = Physics2D.Raycast(origin, dir, edgeCatchDistance, collisionMask);
        Debug.DrawRay(origin, dir * edgeCatchDistance, (hit && !hit.collider.isTrigger) ? Color.magenta : Color.red);

        if (hit && !hit.collider.isTrigger)
        {
            // Filter out slopes: require surface normal to be "mostly vertical"
            if (Mathf.Abs(hit.normal.y) < 0.1f && Mathf.Abs(hit.normal.x) > 0.9f)
            {
                // Apply a gentle vertical pop
                float verticalImpulse = edgeCatchHeight / Time.deltaTime;
                if (verticalImpulse > 0f && verticalImpulse < Mathf.Abs(gravity))
                {
                    velocity.y = Mathf.Max(velocity.y, verticalImpulse);


                    // Horizontal translation instead of impulse to avoid launch
                    transform.position += new Vector3(Mathf.Sign(dir.x) * edgeCatchDistance * 0.3f, 0f, 0f);
                }


                // Prevent retriggering
                hasJumped = false;
            }
        }
    }


    #endregion
}