using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENTITY_AntiLaser : EntityBase, IDamager
{

    public GameObject player;
    public GameObject particle;
    [SerializeField] Rigidbody2D rb2D;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip shootSound;
    [SerializeField] GameObject laserPrefab;
    public float HP = 20;
    public float HpMax = 20;

    void Start()
    {
        
    }

    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
        {
            AiFrame();
        }
    }

    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        Hp -= damageDeal;
    }

    public override int Hp
    {
        get { return Mathf.FloorToInt(HP); }

        set
        {
            if (value > HpMax)
            {
                HP = HpMax;
            }
            else if (value <= 0f)
            {
                HP = 0;
                Kill(null);
            }
            else
            {
                HP = value;
            }
        }
    }

    public override void AiFrame()
    {
        if(Vector2.Distance(transform.position, player.transform.position) > 3.5f)
        {
            GetComponent<CircleCollider2D>().enabled = false;
        }
        else
        {
            if (Vector2.Distance(transform.position, player.transform.position) < 3.5f)
            {
                GetComponent<CircleCollider2D>().enabled = true ;
            }
        }

        Vector2 playerPos = player.transform.position;
        playerPos += Vector2.up;
        if (player.GetComponent<SpriteRenderer>().flipX)
        {
            playerPos += Vector2.right;
        }
        else
        {
            playerPos += Vector2.left;
        }

        rb2D.MovePosition(Vector2.Lerp(transform.position, playerPos, 0.3f));

        PROJECTILE_Laser[] lasers = GameManager.gameManagerReference.entitiesContainer.GetComponentsInChildren<PROJECTILE_Laser>();
        if (lasers.Length != 0)
        {
            float nearest = 1000000f;
            PROJECTILE_Laser nearesT = null;
            foreach(PROJECTILE_Laser laser in lasers)
            {
                float dis = Vector2.Distance(transform.position, laser.transform.position);
                if(dis < nearest)
                {
                    nearest = dis;
                    nearesT = laser;
                }
            }

            Vector2 targetPos = nearesT.transform.position;
            transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPos);

            if (nearest < 4.5f)
            {
                audioSource.PlayOneShot(shootSound);
                GameObject g = Instantiate(particle, transform.position, Quaternion.identity);
                g.GetComponent<ParticleController>().Spawn();

                GameObject newLaser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
                newLaser.transform.eulerAngles = transform.eulerAngles;
                newLaser.transform.localScale = new Vector3(1, Vector2.Distance(transform.position, targetPos), 1);
                Destroy(nearesT.gameObject);
            }
        }
        else
        {
            if (player.GetComponent<SpriteRenderer>().flipX)
            {
                transform.eulerAngles = Vector3.forward * 90f;
            }
            else
            {
                transform.eulerAngles = Vector3.forward * -90f;
            }
            
        }
    }

    public override void Despawn()
    {
        Destroy(gameObject);
    }

    public override void Kill(string[] args)
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject g = Instantiate(particle, transform.position, Quaternion.identity);
            g.GetComponent<ParticleController>().Spawn();
        }
        Despawn();
    }

    public static EntityBase StaticSpawn(string[] args, Vector2 spawnPos)
    {
        return Instantiate(GameManager.gameManagerReference.EntitiesGameObject[(int)Entities.AntiLaser], spawnPos, Quaternion.identity).GetComponent<ENTITY_AntiLaser>().Spawn(args, spawnPos);
    }

    public override EntityBase Spawn(string[] args, Vector2 spawnPos)
    {
        transform.parent = GameManager.gameManagerReference.entitiesContainer.transform;
        player = GameManager.gameManagerReference.player.gameObject;
        transform.GetChild(0).GetComponent<DamagersCollision>().target = this;
        transform.GetChild(0).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();
        return this;
    }
}
