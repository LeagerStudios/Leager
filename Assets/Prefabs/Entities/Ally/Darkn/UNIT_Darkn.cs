using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UNIT_Darkn : UnitBase, IDamager
{
    public Rigidbody2D rb2d;
    public EntityCommonScript entityScript;
    [SerializeField] StackStorage stackStorage;
    [SerializeField] GameObject fireParticle;
    [SerializeField] GameObject fireZone;
    [SerializeField] GameObject explosion;
    [SerializeField] LayerMask blockMask;
    [SerializeField] GameObject[] attachs;
    public Vector2 targetVelocity;
    public Vector2 targetPosition;
    UnitTool control;
    float HpMax = 32;
    float HP = 32;
    public float maxVelocity;

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

    public override EntityCommonScript EntityCommonScript => entityScript;
    public override GameObject[] Attachs => attachs;
    public override UnitTool Control { get => control; set { control = value; } }
    public override StackStorage Storage { get => stackStorage; set => stackStorage = value; }

    private bool isLocal;
    public override bool IsLocal
    {
        get { return isLocal; }

        set { isLocal = value; }
    }

    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
            AiFrame();
    }

    public static EntityBase StaticSpawn(string[] args, Vector2 spawnPos)
    {
        return Instantiate(GameManager.gameManagerReference.EntitiesGameObject[(int)Entities.Darkn], Vector2.zero, Quaternion.identity).GetComponent<UNIT_Darkn>().Spawn(args, spawnPos);
    }

    public override string[] GenerateArgs()
    {
        return new string[] { maxVelocity + "" };
    }

    void Start()
    {

    }

    public override void AiFrame()
    {
        if(!control)
        targetPosition = GameManager.gameManagerReference.player.transform.position + Vector3.up * (maxVelocity / 5 - 3);

        if (Vector2.Distance(targetPosition, transform.position) > 0.5)
            targetVelocity = Vector2.ClampMagnitude((targetPosition - (Vector2)transform.position) * 20, maxVelocity);
        else
            targetVelocity = Vector2.zero;
        rb2d.velocity = Vector2.Lerp(rb2d.velocity, targetVelocity, 0.8f * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(transform.eulerAngles.z, ManagingFunctions.PointToPivotUp(Vector2.zero, rb2d.velocity), 0.3f));

        if (fireZone.transform.childCount == 0)
        {
            Instantiate(fireParticle, fireZone.transform);
        }
    }


    public override EntityBase Spawn(string[] args, Vector2 spawnPos)
    {
        transform.SetParent(GameManager.gameManagerReference.entitiesContainer.transform);
        transform.position = spawnPos;
        transform.GetChild(0).GetComponent<DamagersCollision>().target = this;
        transform.GetChild(0).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();

        if(args != null)
        {
            Debug.Log(args[0]);
            maxVelocity = System.Convert.ToSingle(args[0]);
        }
        else
        {
            targetPosition = transform.position;
            maxVelocity = Random.Range(25f, 35f);
        }

        return this;
    }

    public override void Despawn()
    {
        Destroy(gameObject);
    }


    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        if (procedence.EntityFamily != "yellow")
        {
            Hp -= damageDeal;
        }
    }

    public override void SetTargetPosition(Vector2 targetPos)
    {
        targetPosition = targetPos;
    }

    public override Vector2 GetTargetPosition()
    {
        return targetPosition;
    }

    public override void Kill(string[] args)
    {
        if (args != null)
        {
            if (args[0] == "dead")
            {
                ManagingFunctions.DropItem(31, transform.position, amount: Random.Range(1, 4));
                ManagingFunctions.DropItem(93, transform.position, amount: Random.Range(2, 5));
                Despawn();
            }
        }
        else
            Despawn();
    }
}
