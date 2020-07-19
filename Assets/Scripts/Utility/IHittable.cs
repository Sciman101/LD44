using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHittable
{
    /// <summary>
    /// Called when this thing is hit
    /// </summary>
    /// <param name="amount">Hot much damage to deal</param>
    /// <param name="knockback">In what direction are we being hit</param>
    void OnHit(int amount, Vector2 knockback);

    /// <summary>
    /// Get the 'team' this thing is on
    /// </summary>
    /// <returns></returns>
    Team GetTeam();
}
