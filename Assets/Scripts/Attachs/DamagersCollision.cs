using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagersCollision : MonoBehaviour
{
    public IDamager target;
    public EntityCommonScript entity;
    public string team = "";

    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        target.Hit(damageDeal, procedence, ignoreImunity, knockback, penetrate);
    }
}

public interface IDamager
{
    void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false);
}
