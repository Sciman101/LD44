using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [SerializeField]
    private float explosionRadius = 0;

    private void Start()
    {
        //Destroy this after about a second
        Destroy(gameObject,1);

        //Camera shake
        CameraController.instance.AddCameraShake(.75f);

        //Check for all objects in radius
        int size = WaveSpawner.instance?.GetEnemyCount() * 2 ?? 10; 
        Collider2D[] hits = new Collider2D[size];
        if (Physics2D.OverlapCircleNonAlloc(transform.position,explosionRadius,hits) > 0)
        {
            //Look for hittable thing
            foreach (Collider2D coll in hits)
            {
                //Ignore null colliders and the player
                if (coll == null) continue;
                
                //Find hittable objects
                IHittable hit = coll.GetComponent<IHittable>();
                if (hit != null)
                {
                    //Do 4 damage
                    hit.OnHit(4,(coll.transform.position-transform.position) * 15);
                }
            }
        }
    }
}
