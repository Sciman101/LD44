using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldEnemyController : EnemyController
{
    /// <summary>
    /// Explosion prefab
    /// </summary>
    [Header("Gold Enemy"), SerializeField]
    private GameObject explosionPrefab;
    [SerializeField]
    private SpriteRenderer explosionTimer;
    [SerializeField]
    private float patience;

    /// <summary>
    /// How long will the gold slime go before exploding?
    /// </summary>
    private float patienceAmount;

    /// <summary>
    /// Previous x position
    /// </summary>
    private float lastX;

    protected override void Update()
    {
        base.Update();
        float dt = Time.deltaTime;

        //How far have we moved?
        float dist = Mathf.Abs(lastX - transform.position.x);
        if (dist < (moveSpeed-1)*dt)
        {
            patienceAmount -= dt;

            //Amimate timer
            float prog = (1 - (patienceAmount / patience));
            explosionTimer.transform.localScale = Vector3.one * 3 * prog;
            Color c = Color.Lerp(Color.yellow, Color.red, prog);
            c.a = 0.5f;
            explosionTimer.color = c;

            //Set off bomb
            if (patienceAmount <= 0)
            {
                //Blow up
                Explode();
            }
        }
        else if (patienceAmount < patience)
        {
            patienceAmount = patience;
            explosionTimer.transform.localScale = Vector3.zero;
        }

        lastX = transform.position.x;
    }

    public override void OnTriggerStay2D(Collider2D collision)
    {
        IHittable hit = collision.GetComponent<IHittable>();
        if (hit != null && hit.GetTeam() == Team.Player)
        {
            Explode();
        }
    }

    /// <summary>
    /// Blow up
    /// </summary>
    void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        WaveSpawner.instance?.OnEnemyKilled();
        Destroy(gameObject);
    }

}
