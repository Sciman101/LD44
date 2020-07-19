using UnityEngine;

public class SlimeBlockController : MonoBehaviour, IHittable
{
    public void OnHit(int amount, Vector2 knockback)
    {
        Destroy(gameObject);
    }

    public Team GetTeam()
    {
        return Team.Neutral;
    }
}
