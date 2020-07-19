using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingThing : MonoBehaviour
{
    /// <summary>
    /// Movement properties
    /// </summary>
    [Header("Movement Properties")]
    [SerializeField]
    //Gravity to be applied to character
    protected float gravity = 0;

    //How fast does the thing slow down
    [SerializeField]
    protected float friction = 0;

    /// <summary>
    /// Refrence to platformer controller
    /// </summary>
    protected PlatformerController platformer;

    /// <summary>
    /// Player velocity
    /// </summary>
    [HideInInspector]
    public Vector2 velocity;

    // Start is called before the first frame update
    public virtual void Start()
    {
        platformer = GetComponent<PlatformerController>();
    }

    /// <summary>
    /// Handle movement of thing
    /// </summary>
    /// <param name="dt"></param>
    protected void HandleMovement(float dt, float gravMult=1)
    {
        //Add gravity
        velocity.y -= gravity * dt * gravMult;

        //Slow down
        velocity.x = Mathf.MoveTowards(velocity.x, 0, friction * dt);

        //Move
        platformer.Move(velocity * dt);

        //Test for collisions that would stop velocity
        if (platformer.collisions.bottom || platformer.collisions.top)
        {
            velocity.y = 0;
        }
        if (platformer.collisions.right || platformer.collisions.left)
        {
            velocity.x = 0;
        }
    }
}
