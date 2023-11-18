using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PROJECTILE_Arrow : ProjectileBase
{
    private int damage = 2;
    private int frame = 0;
    public Rigidbody2D rb2D;
    public EntityCommonScript procedence;
    public override int Damage { get { return damage; } }
    public bool flying = false;


    void Update ()
    {
        if (GameManager.gameManagerReference.InGame)
        {
            frame++;
            if (frame > 400) Despawn();
            Frame();
        }
	}

    public override void Frame()
    {
        if (flying)
        {
            transform.eulerAngles = new Vector3(0, 0, ManagingFunctions.PointToPivotUp(Vector2.zero, rb2D.velocity));
        }
    }

    public override void Spawn(float dir, Vector2 spawnPos, int extraDamage, EntityCommonScript procedence)
    {
        transform.eulerAngles = new Vector3(0, 0, dir);
        transform.SetParent(GameManager.gameManagerReference.entitiesContainer.transform);
        damage += extraDamage;
        this.procedence = procedence;
        flying = true;
        rb2D.AddForce(new Vector2(Mathf.Sin(dir * Mathf.Deg2Rad) * 350, Mathf.Cos(dir * Mathf.Deg2Rad) * 350));
    }

    public static void StaticSpawn(float dir, Vector2 spawnPos, int extraDamage, EntityCommonScript procedence)
    {
        Instantiate(GameManager.gameManagerReference.ProjectilesGameObject[(int)Projectiles.Arrow], spawnPos, Quaternion.identity).GetComponent<PROJECTILE_Arrow>().Spawn(dir, spawnPos, extraDamage, procedence);
    }

    public override void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (flying)
        {
            if (collision.gameObject.layer == 8 && collision.gameObject.GetComponent<PlatformEffector2D>() == null)
            {
                Despawn();
                flying = false;
            }

            if (collision.gameObject.GetComponent<DamagersCollision>() != null)
            {
                if (collision.gameObject.GetComponent<DamagersCollision>().entity.EntityFamily == procedence.EntityFamily)
                {

                }
                else
                {
                    collision.gameObject.GetComponent<DamagersCollision>().Hit(damage, procedence);
                    Despawn();
                    flying = false;
                }

            }
        }
    }

    public override void CallStaticSpawn(float dir, Vector2 spawnPos, int extraDamage, EntityCommonScript procedence)
    {
        StaticSpawn(dir, spawnPos, extraDamage, procedence);
    }
}
