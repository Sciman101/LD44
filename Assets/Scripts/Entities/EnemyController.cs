using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MovingThing, IHittable
{
    private static readonly Vector2 KNOCKBACK = new Vector2(1, .5f);

    [SerializeField]
    protected float moveSpeed = 0;
    [SerializeField]
    protected float acceleration = 0;
    [SerializeField]
    protected float jumpSpeed = 0;
    
    public Transform target = null;

    /// <summary>
    /// Max health this enemy can have
    /// </summary>
    [Header("Enemy Properties"),SerializeField]
    private int maxHealth = 0;
    [SerializeField]
    private int damageAmount = 0;
    [SerializeField]
    private GameObject drop = null;

    /// <summary>
    /// Particle system for when hit
    /// </summary>
    [Header("Particles"),SerializeField]
    private ParticleSystem onHitParticles = null;

    /// <summary>
    /// How much health do we currently have
    /// </summary>
    private int health;

    /// <summary>
    /// Refrence to sprite renderer
    /// </summary>
    private new SpriteRenderer renderer;

    /// <summary>
    /// Refrence to animator
    /// </summary>
    private Animator animator;

    public override void Start()
    {
        base.Start();

        //Get components
        renderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();

        //Randomize health slightly
        health = maxHealth;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        float dt = Time.deltaTime;

        //Get direction to target
        float dir = Mathf.Sign(target.position.x - transform.position.x);

        renderer.flipX = dir < 0;

        velocity.x = Mathf.Clamp(velocity.x + dir * acceleration, -moveSpeed, moveSpeed);

        //Check for obstacle
        if ((platformer.collisions.left || platformer.collisions.right) && platformer.IsGrounded)
        {
            velocity.y = jumpSpeed;
        }

        HandleMovement(dt);
    }

    /// <summary>
    /// When something hits us...
    /// </summary>
    /// <param name="collision"></param>
    public virtual void OnTriggerStay2D(Collider2D collision)
    {
        IHittable hit = collision.GetComponent<IHittable>();
        if (hit != null && hit.GetTeam() == Team.Player)
        {
            hit.OnHit(damageAmount, KNOCKBACK * velocity.x * damageAmount);
        }
    }

    /// <summary>
    /// Called when we are hit
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="knockback"></param>
    public void OnHit(int amount, Vector2 knockback)
    {
        health -= amount;
        //We have been hit
        velocity += knockback;

        animator.SetTrigger("Hit");

        onHitParticles.Play();

        //Check for death
        if (health <= 0)
        {
            WaveSpawner.instance?.OnEnemyKilled();
            Instantiate(drop, transform.position + Vector3.up, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public Team GetTeam()
    {
        return Team.Enemy;
    }
}
