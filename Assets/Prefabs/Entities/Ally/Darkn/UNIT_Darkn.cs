using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UNIT_Darkn : EntityBase, IDamager
{
    public Rigidbody2D rb2d;
    [SerializeField] GameObject fireParticle;
    [SerializeField] GameObject fireZone;
    [SerializeField] GameObject explosion;
    [SerializeField] LayerMask blockMask;
    public Vector2 targetVelocity;
    public Vector2 targetPosition;
    public UnitWeapon[] weapons;
    float HpMax = 32;
    float HP = 32;

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
                Kill(new string[] { "dead" });
            }
            else
            {
                HP = value;
            }
        }
    }

    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
            AiFrame();
    }

    public static void StaticSpawn(string[] args, Vector2 spawnPos)
    {
        Instantiate(GameManager.gameManagerReference.EntitiesGameObject[(int)UnitEntities.Darkn], Vector2.zero, Quaternion.identity).transform.GetChild(0).GetComponent<UNIT_Darkn>().Spawn(args, spawnPos);
    }

    void Start()
    {

    }

    public override void AiFrame()
    {
        rb2d.velocity = Vector2.Lerp(rb2d.velocity, targetVelocity, 0.8f * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(transform.eulerAngles.z, ManagingFunctions.PointToPivotUp(Vector2.zero, rb2d.velocity), 0.3f));

        if (rb2d.velocity.magnitude > 0.1f && fireZone.transform.childCount == 0)
        {
            Instantiate(fireParticle, fireZone.transform);
        }
        else if (rb2d.velocity.magnitude < 0.1f)
            if (fireZone.transform.childCount > 0)
                if (!fireZone.transform.GetChild(0).GetComponent<CoreFire>().endOnNext)
                    fireZone.transform.GetChild(0).GetComponent<CoreFire>().endOnNext = true;

        if(Physics2D.Raycast(transform.position, Vector2.up, 1f, blockMask))
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }


    public override EntityBase Spawn(string[] args, Vector2 spawnPos)
    {
        transform.parent.SetParent(GameManager.gameManagerReference.entitiesContainer.transform);
        transform.position = spawnPos;
        transform.GetChild(1).GetComponent<DamagersCollision>().target = this;
        transform.GetChild(1).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();
        weapons = GetComponentsInChildren<UnitWeapon>();

        return this;
    }

    public override void Despawn()
    {
        Destroy(gameObject);
    }


    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        Hp = Hp - damageDeal;
    }

    public override void Kill(string[] args)
    {
        if (args != null)
        {
            if (args[0] == "dead")
            {
                float shortestDist = 9999999;
                int segmentShortestDist = 0;

                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    Transform indexSegment = transform.parent.GetChild(i);
                    if (Vector2.Distance(GameManager.gameManagerReference.player.transform.position, indexSegment.position) < shortestDist)
                    {
                        shortestDist = Vector2.Distance(GameManager.gameManagerReference.player.transform.position, indexSegment.position);
                        segmentShortestDist = i;
                    }
                }
                Transform targetSegment = transform.parent.GetChild(segmentShortestDist);

                ManagingFunctions.DropItem(65, targetSegment.position, Random.Range(20, 40));
                ManagingFunctions.DropItem(34, targetSegment.position, Random.Range(5, 10));
                Despawn();
            }
        }
        else
            Despawn();
    }
}
