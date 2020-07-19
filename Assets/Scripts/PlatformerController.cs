using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class PlatformerController : MonoBehaviour
{
    public readonly static string TAG_ONEWAYPLATFORM = "One Way Platform";

    //Raycast controller vars
    /// <summary>
    /// Width of collider 'skin'
    /// </summary>
    protected const float skinWidth = .015f;

    /// <summary>
    /// Mask we can collide with
    /// </summary>
    public LayerMask collisionMask;

    /// <summary>
    /// Number of horizontal and vertical rays, respectively
    /// </summary>
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    protected float horizontalRaySpacing, verticalRaySpacing;

    /// <summary>
    /// Collider and raycast data
    /// </summary>
    protected new BoxCollider2D collider;
    protected RaycastOrigins rayOrigins;
    public CollisionData collisions;

    //Get the bottom of the character, for use in particle effects and such
    public Vector2 Bottom
    { get { return (collider.bounds.center + new Vector3(0, -collider.bounds.extents.y)); } }

    public bool IsGrounded//Shortcut to collisions.bottom
    { get { return collisions.bottom; } }
    [HideInInspector]
    public bool wasGrounded;//Used to trigger onGrounded, by comparing was and is grounded

    //UnityEvents
    [HideInInspector]
    public UnityEvent onGrounded;
    //These events are called when their respective name occurs

    protected void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector2 velocity)
    {
        UpdateRaycastOrigins();

        collisions.Reset();

        //Do collision detection
        //Horizontal
        if (velocity.x != 0)
        {
            HorizontalCollisions(ref velocity);
        }

        //Vertical
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        //Actually move
        transform.Translate(velocity,Space.World);

        //Trigger 'on grounded' method
        if (IsGrounded && !wasGrounded)
        {
            onGrounded.Invoke();
        }
        wasGrounded = IsGrounded;
    }

    void VerticalCollisions(ref Vector2 velocity)
    {
        float yDir = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            //Find origin of ray
            Vector2 origin = (yDir == -1 ? rayOrigins.bottomLeft : rayOrigins.topLeft);
            origin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * yDir, rayLength, collisionMask);

            Debug.DrawRay(origin, Vector2.up * yDir * rayLength, Color.red);

            //If we hit something...
            if (hit)
            {

                if (yDir == 1 && PlatformIsOneWay(hit.transform)) continue;//Don't bother with one way platforms
                    
                //Change y velocity and ray distance
                velocity.y = (hit.distance - skinWidth) * yDir;
                rayLength = hit.distance;

                //Set collisions
                collisions.top = (yDir == 1);
                collisions.bottom = (yDir == -1);
            }
        }
    }


    //Evaluate horizontal collisions
    void HorizontalCollisions(ref Vector2 velocity)
    {
        float xDir = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            //Find origin of ray
            Vector2 origin = (xDir == -1 ? rayOrigins.bottomLeft : rayOrigins.bottomRight);
            origin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * xDir, rayLength, collisionMask);

            Debug.DrawRay(origin, Vector2.right * xDir * rayLength, Color.red);

            //If we hit something...
            if (hit)
            {
                if (PlatformIsOneWay(hit.transform)) continue;//Ignore these, too

                //Change x velocity and ray distance
                velocity.x = (hit.distance - skinWidth) * xDir;
                rayLength = hit.distance;

                //Set collisions
                collisions.right = (xDir == 1);
                collisions.left = (xDir == -1);
            }
        }
    }

    /// <summary>
    /// Check if a platform has one way colllisions enabled
    /// </summary>
    /// <param name="platform">The transform of the platform to check</param>
    /// <returns></returns>
    public static bool PlatformIsOneWay(Transform platform)
    {
        return platform.CompareTag(TAG_ONEWAYPLATFORM);
    }

    /// <summary>
    /// Raycast controller methods begin below
    /// </summary>
    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        //Set proper ray origins based on bounds
        rayOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        rayOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        rayOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        rayOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        //Clamp ray count
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        //Set raycast spacing based on bounds
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }


    //Struct to hold the origins of the raycasts
    protected struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    //Struct to hold current collisions
    public struct CollisionData
    {
        public bool top, bottom;
        public bool right, left;

        public void Reset()
        {
            top = bottom = left = right = false;
        }
    }

}