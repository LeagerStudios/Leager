using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    public abstract int Damage { get; }
    public abstract void Frame();
    public abstract ProjectileBase Spawn(float dir, Vector2 spawnPos, int extraDamage, EntityCommonScript procedence);
    public abstract void Despawn();

    //public override void CallStaticSpawn(float dir, Vector2 spawnPos, int extraDamage)
    //{
    //    StaticSpawn(dir, spawnPos, extraDamage);
    //}
}
