using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotSlimeballController : MonoBehaviour
{
    /// <summary>
    /// Gravity acting on this projectile
    /// </summary>
    [SerializeField]
    private float gravity = 0;

    /// <summary>
    /// What can this collide with?
    /// </summary>
    [SerializeField]
    private LayerMask collisionMask = 0;

    [SerializeField]
    private ParticleSystem splashParticles = null;

    [Header("On Impact"), SerializeField]
    private GameObject dropPrefab = null;
    [SerializeField]
    private GameObject explosionPrefab = null;
    [SerializeField]
    private GameObject blockPrefab = null;

    AudioSource audio;

    /// <summary>
    /// Velocity of slimeball
    /// </summary>
    private Vector2 velocity;

    /// <summary>
    /// What kind of slime is this slimeball?
    /// </summary>
    private SlimeType slimeType;

    /// <summary>
    /// Auto destroy after 15 seconds, in case something goes wrong
    /// </summary>
    private void Start()
    {
        audio = GetComponentInChildren<AudioSource>();
        Destroy(gameObject, 15);
    }

    /// <summary>
    /// Move
    /// </summary>
    private void Update()
    {
        float dt = Time.deltaTime;

        velocity.y -= gravity * dt;
        transform.Translate(velocity * dt, Space.World);

        //Check for collision
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, .5f, velocity, velocity.magnitude*dt, collisionMask);
        if (hit && !hit.collider.isTrigger)
        {
            OnImpact(hit);
        }
    }

    /// <summary>
    /// Set the velocity of the slimeball
    /// </summary>
    /// <param name="velocity"></param>
    public void SetVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
    }

    /// <summary>
    /// Set the appropriate slime type for this slimeball
    /// </summary>
    /// <param name="type"></param>
    public void SetSlimeType(SlimeType type)
    {
        slimeType = type;
        Color c = PlayerController.gooColors[(int)slimeType];
        GetComponent<SpriteRenderer>().color = c;

        ParticleSystem.MainModule main = splashParticles.main;
        main.startColor = c;
    }

    /// <summary>
    /// When we enter something...
    /// </summary>
    /// <param name="collision"></param>
    private void OnImpact(RaycastHit2D hit)
    {
        Collider2D other = hit.collider;
        //Check for collideable surface
        if (((1 << other.gameObject.layer) & collisionMask) != 0)
        {
            //Check for one way platforms
            if (!(other.CompareTag(PlatformerController.TAG_ONEWAYPLATFORM) && Vector2.Dot(velocity, Vector2.up) > .5f))
            {
                bool isEnemy = false;
                //Check for hittable
                IHittable hittable = other.gameObject.GetComponent<IHittable>();
                //If we hit something and it is not player affiliated
                if (hittable != null && hittable.GetTeam() != Team.Player)
                {
                    //Determine if it is an enemy
                    isEnemy = hittable.GetTeam() == Team.Enemy;

                    //If it is not a block
                    if (!(slimeType == SlimeType.Purple && other.CompareTag("Block")))
                    {
                        hittable.OnHit(1, 2 * (other.transform.position - transform.position));
                    }
                }

                //Do something, depending on what type of slime we are
                switch (slimeType)
                {
                    case SlimeType.Green:
                        //Only do it on vertical faces
                        if (!isEnemy && Vector3.Dot(hit.normal, Vector3.up) > .9f)
                        {
                            //Make drop prefab
                            Instantiate(dropPrefab, hit.point - (Vector2.up * .25f), Quaternion.identity);
                        }
                        break;
                    case SlimeType.Purple:
                        if (!isEnemy)
                        {
                            //Round position
                            Vector3 pos = transform.position;
                            pos.x = Mathf.Round(pos.x / 2) * 2;
                            pos.y = Mathf.Round(pos.y / 2) * 2;
                            Instantiate(blockPrefab, pos, Quaternion.identity);
                        }
                        break;
                    case SlimeType.Gold:
                        //Make explosion prefab
                        Instantiate(explosionPrefab, hit.point, Quaternion.identity);
                        //Increase particle size
                        splashParticles.transform.localScale = Vector3.one * 1.5f;
                        break;
                }

                //Play particle effect
                splashParticles.transform.SetParent(null);
                splashParticles.Play();

                audio.Play();

                //Destroy self
                Destroy(gameObject);
            }
        }
    }
}
