using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PROJECTILE_SwordParticle : ProjectileBase {

    private int damage = 0;
    int frame = 0;
    EntityCommonScript procedence;
    public override int Damage { get { return damage; } }

    public void Update()
    {
        Frame();
    }

    public override void Frame()
    {
        if(frame > 20)
        {
            Despawn();
        }
        frame++;
    }

    public override ProjectileBase Spawn(float dir, Vector2 spawnPos, int extraDamage, EntityCommonScript procedence)
    {
        damage = extraDamage;
        this.procedence = procedence;
        return this;
    }

    public static ProjectileBase StaticSpawn(Vector2 spawnPos, int damage, EntityCommonScript procedence)
    {
        return Instantiate(GameManager.gameManagerReference.ProjectilesGameObject[(int)Projectiles.SwordParticle], spawnPos, Quaternion.identity).GetComponent<PROJECTILE_SwordParticle>().Spawn(0, spawnPos, damage, procedence); ;
    }

    public override void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<DamagersCollision>() != null)
        {
            collision.gameObject.GetComponent<DamagersCollision>().Hit(damage, procedence);
            GameManager.gameManagerReference.DisplayDamage(damage, transform.position + Vector3.up);
            damage = 0;
            Despawn();
        }
    }
}
