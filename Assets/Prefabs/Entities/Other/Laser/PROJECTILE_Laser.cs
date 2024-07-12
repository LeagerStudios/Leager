using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PROJECTILE_Laser : ProjectileBase {

    private int damage = 13;
    private int frame = 0;
    public Rigidbody2D rb2D;
    public EntityCommonScript from;
    public override int Damage { get { return damage; } }


    void Update()
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
        transform.eulerAngles = new Vector3(0, 0, ManagingFunctions.PointToPivotUp(Vector2.zero, rb2D.velocity));
    }

    public override void Spawn(float dir, Vector2 spawnPos, int extraDamage, EntityCommonScript procedence)
    {
        transform.eulerAngles = new Vector3(0, 0, dir);
        transform.SetParent(GameManager.gameManagerReference.entitiesContainer.transform);
        damage += extraDamage;
        rb2D.AddForce(new Vector2(Mathf.Sin(dir * Mathf.Deg2Rad) * -400, Mathf.Cos(dir * Mathf.Deg2Rad) * 400));
        from = procedence;
    }

    public static void StaticSpawn(float dir, Vector2 spawnPos, int extraDamage, EntityCommonScript procedence)
    {
        Instantiate(GameManager.gameManagerReference.ProjectilesGameObject[(int)Projectiles.Laser], spawnPos, Quaternion.identity).GetComponent<PROJECTILE_Laser>().Spawn(dir, spawnPos, extraDamage, procedence);
    }

    public override void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            if (collision.gameObject.GetComponent<PlatformEffector2D>() == null)
            {
                if (collision.gameObject.transform.parent != null)
                {
                    if (collision.gameObject.transform.parent.GetComponent<ChunkController>() != null)
                    {
                        ChunkController chunkCollision = collision.gameObject.transform.parent.GetComponent<ChunkController>();
                        int collisionIdx = System.Array.IndexOf(chunkCollision.TileObject, collision.gameObject);
                        if (collisionIdx + 1 < chunkCollision.TileGrid.Length)
                            if (chunkCollision.TileGrid[collisionIdx + 1] == 0)
                            {
                                chunkCollision.TileGrid[collisionIdx + 1] = 84;
                                chunkCollision.UpdateChunk();
                            }
                    }
                }

                Despawn();
            }
        }

        if (collision.gameObject.GetComponent<EntityCommonScript>() != null)
        {
            if (collision.gameObject.GetComponent<EntityCommonScript>().EntityFamily == "yellow")
            {
                if (Random.Range(0, 13) == 0)
                    collision.gameObject.GetComponent<EntityCommonScript>().AddState(EntityState.OnFire, 3f);
                else
                    if (collision.gameObject.GetComponent<EntityCommonScript>().entityDamager != null)
                    collision.gameObject.GetComponent<EntityCommonScript>().entityDamager.Hit(damage, from);
                Despawn();
            }
        }
}

    public override void CallStaticSpawn(float dir, Vector2 spawnPos, int extraDamage, EntityCommonScript procedence)
    {
        StaticSpawn(dir, spawnPos, extraDamage, procedence);
    }
}
