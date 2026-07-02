using UnityEngine;

/// <summary>
/// A kinematic 2D character controller that uses raycasts to detect collisions,
/// climb and descend slopes, similar to Unity's CharacterController (3D).
/// Inspired by Sebastian Lague's 2D Platformer series.
/// 
/// Author: Damyan
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CollisionHandler2D : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField][Range(0.001f, 0.1f)] private float skinWidth = 0.02f;
    [Header("Raycast Settings")]
    [SerializeField] private float horizontalRaySpacing = 0.25f;
    [SerializeField] private float verticalRaySpacing = 0.25f;
    private int horizontalRayCount;
    private int verticalRayCount;

    [Header("Slope Settings")]
    [SerializeField] private bool descendSlopes = true;
    [SerializeField] private float maxClimbAngle = 80f;
    [SerializeField] private float maxDescendAngle = 80f;

    [Header("Debug")]
    [SerializeField] private bool visualizeRays = true;

    [Header("Functional")]
    private RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;
    private Vector2 moveAmountOld;

    private BoxCollider2D boxCollider;

    private Transform _platform;
    private Vector3 _platformPosOld;

    public Collider2D IgnoredPlatform { get; set; }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        CalculateRaySpacing();
    }

    #region Public API

    /// <summary>
    /// Method that moves the game object with respect
    /// to velocity and its collider, without using
    /// a rigidbody.
    /// </summary>
    /// <param name="moveAmount"></param>
    public void Move(Vector2 moveAmount)
    {
        // Reset IgnoredPlatform if player has fallen below the top surface of the platform or moved away
        if (IgnoredPlatform != null)
        {
            float playerFeetY = boxCollider.bounds.min.y;
            float platformTopY = IgnoredPlatform.bounds.max.y;

            // Check if player has moved horizontally off the platform
            float playerMinX = boxCollider.bounds.min.x;
            float playerMaxX = boxCollider.bounds.max.x;
            float platformMinX = IgnoredPlatform.bounds.min.x;
            float platformMaxX = IgnoredPlatform.bounds.max.x;
            bool overlapsHorizontally = playerMaxX >= platformMinX && playerMinX <= platformMaxX;

            if (playerFeetY < platformTopY - 0.05f || !overlapsHorizontally)
            {
                IgnoredPlatform = null;
            }
        }

        // Ride moving platforms: apply their delta before own movement
        if (_platform != null)
        {
            Vector2 platformDelta = (Vector2)(_platform.position - _platformPosOld);
            transform.Translate(platformDelta);
            Physics2D.SyncTransforms();
        }

        UpdateRaycastOrigins();
        collisions.Reset();
        moveAmountOld = moveAmount;

        // Descend slopes first
        if (moveAmount.y <= 0 && descendSlopes)
            DescendSlope(ref moveAmount);

        // Check vertical collisions first if moving upward (detect ceilings)
        if (moveAmount.y > 0)
            VerticalCollisions(ref moveAmount);

        // Horizontal collisions and climbing
        if (moveAmount.x != 0)
            HorizontalCollisions(ref moveAmount);

        // Vertical collisions again to finalize vertical movement
        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }
        else
        {
            GroundCheck();
        }

        transform.Translate(moveAmount);
        Physics2D.SyncTransforms();

        // Update platform tracking for next frame
        if (collisions.below && collisions.belowCollider != null)
        {
            _platform = collisions.belowCollider.transform;
            _platformPosOld = _platform.position;
        }
        else
        {
            _platform = null;
        }
    }

    #endregion

    #region Collision Handling

    private void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = Mathf.Sign(moveAmount.x);
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hit = CastRay(rayOrigin, Vector2.right * directionX, rayLength);
            if (!hit) continue;

            // One-way platform effector support: ignore horizontal collisions entirely
            if (hit.collider.usedByEffector) continue;

            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            // Climb slope if first ray, slope is climbable, moving down or level, and not blocked above
            if (i == 0 && slopeAngle <= maxClimbAngle && moveAmount.y <= 0 && !collisions.above)
            {
                if (collisions.descendingSlope)
                {
                    collisions.descendingSlope = false;
                    moveAmount = moveAmountOld;
                }

                float distanceToSlopeStart = 0;
                if (slopeAngle != collisions.slopeAngleOld)
                {
                    distanceToSlopeStart = hit.distance - skinWidth;
                    moveAmount.x -= distanceToSlopeStart * directionX;
                }

                ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                moveAmount.x += distanceToSlopeStart * directionX;
            }

            // Horizontal collision if slope too steep or not climbing
            if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
            {
                moveAmount.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
            }
        }
    }

    private void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);

            RaycastHit2D hit = CastRay(rayOrigin, Vector2.up * directionY, rayLength);

            if (!hit) continue;

            if (hit.collider.usedByEffector)
            {
                if (directionY == 1 || hit.collider == IgnoredPlatform) continue;

                // If the ray starts inside the platform, only ignore it if the player's
                // feet are structurally below the top surface of the platform.
                if (hit.distance == 0)
                {
                    float platformTopY = hit.collider.bounds.max.y;
                    float playerFeetY = boxCollider.bounds.min.y;
                    if (playerFeetY < platformTopY - 0.05f) continue;
                }
            }

            if (visualizeRays)
                Debug.DrawRay(rayOrigin, directionY * rayLength * Vector2.up, Color.red);

            moveAmount.y = (hit.distance - skinWidth) * directionY;
            rayLength = hit.distance;

            collisions.below = directionY == -1;
            collisions.above = directionY == 1;

            if (directionY == -1)
                collisions.belowCollider = hit.collider;

            // Adjust horizontal if climbing slope
            if (collisions.climbingSlope)
                moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);

            // Stop climbing slope if blocked above
            if (collisions.above && collisions.climbingSlope)
            {
                moveAmount.x = 0;
                collisions.climbingSlope = false;
            }
        }
    }

    private void GroundCheck()
    {
        float rayLength = skinWidth * 2f;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = raycastOrigins.bottomLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i);

            RaycastHit2D hit = CastRay(rayOrigin, Vector2.down, rayLength);

            if (!hit) continue;

            if (hit.collider.usedByEffector)
            {
                if (hit.collider == IgnoredPlatform) continue;

                // If the ray starts inside the platform, only ignore it if the player's
                // feet are structurally below the top surface of the platform.
                if (hit.distance == 0)
                {
                    float platformTopY = hit.collider.bounds.max.y;
                    float playerFeetY = boxCollider.bounds.min.y;
                    if (playerFeetY < platformTopY - 0.05f) continue;
                }
            }

            collisions.below = true;
            collisions.belowCollider = hit.collider;
            return;
        }
    }

    private void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbMoveY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (moveAmount.y <= climbMoveY)
        {
            moveAmount.y = climbMoveY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);

            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = slopeNormal;
        }
    }

    private void DescendSlope(ref Vector2 moveAmount)
    {
        float directionX = Mathf.Sign(moveAmount.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;

        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth + 0.1f;
        RaycastHit2D hit = CastRay(rayOrigin, Vector2.down, rayLength);

        if (!hit) return;

        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        if (slopeAngle == 0 || slopeAngle > maxDescendAngle) return;
        if (Mathf.Sign(hit.normal.x) != directionX) return;

        float moveDistance = Mathf.Abs(moveAmount.x);
        float descendMoveY = Mathf.Min(Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance, hit.distance - skinWidth);

        moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
        moveAmount.y -= descendMoveY;

        collisions.slopeAngle = slopeAngle;
        collisions.descendingSlope = true;
        collisions.below = true;
        collisions.slopeNormal = hit.normal;
    }

    #endregion

    #region Raycast Utility

    private void UpdateRaycastOrigins()
    {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(-skinWidth * 2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    private void CalculateRaySpacing()
    {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(-skinWidth * 2);

        horizontalRayCount = Mathf.Max(Mathf.CeilToInt(bounds.size.y / horizontalRaySpacing) + 1, 2);
        verticalRayCount = Mathf.Max(Mathf.CeilToInt(bounds.size.x / verticalRaySpacing) + 1, 2);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;
        public Collider2D belowCollider;

        public void Reset()
        {
            above = below = left = right = false;
            climbingSlope = descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
            belowCollider = null;
        }
    }

    #endregion

    public float GetSkinWidth()
    {
        return skinWidth;
    }

    private RaycastHit2D CastRay(Vector2 origin, Vector2 direction, float length)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, length, collisionMask);
        foreach (var hit in hits)
        {
            if (hit.collider == boxCollider) continue;
            if (hit.collider.isTrigger && !hit.collider.usedByEffector) continue;
            
            // Prevent cross-scene physics interference (e.g. Tetris Scene colliding with Computer Hub Scene)
            if (hit.collider.gameObject.scene != gameObject.scene) continue;
            
            return hit;
        }
        return default;
    }
}
