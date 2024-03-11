using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UNIT_Darkn : UnitBase, IDamager
{
    public Rigidbody2D rb2d;
    [SerializeField] GameObject fireParticle;
    [SerializeField] GameObject fireZone;
    [SerializeField] GameObject explosion;
    [SerializeField] LayerMask blockMask;
    public Vector2 targetVelocity;
    public Vector2 targetPosition;
    bool positionControlled = false;
    bool rotationControlled = false;
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

    public override bool PositionControlled
    {
        get { return positionControlled; }

        set { positionControlled = value; }
    }

    private bool isLocal;
    public override bool IsLocal
    {
        get { return isLocal; }

        set { isLocal = value; }
    }

    public override bool RotationControlled
    {
        get { return rotationControlled; }

        set { rotationControlled = value; }
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
        return null;
    }

    void Start()
    {

    }

    public override void AiFrame()
    {
        if (Vector2.Distance(targetPosition, transform.position) > 0.5)
            targetVelocity = Vector2.ClampMagnitude((targetPosition - (Vector2)transform.position) * 20, 30);
        else
            targetVelocity = Vector2.zero;
        rb2d.velocity = Vector2.Lerp(rb2d.velocity, targetVelocity, 0.8f * Time.deltaTime);
        if (!rotationControlled)
            transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(transform.eulerAngles.z, ManagingFunctions.PointToPivotUp(Vector2.zero, rb2d.velocity), 0.3f));

        if (rb2d.velocity.magnitude > 0.1f && fireZone.transform.childCount == 0)
        {
            Instantiate(fireParticle, fireZone.transform);
        }
        else if (rb2d.velocity.magnitude < 0.1f)
            if (fireZone.transform.childCount > 0)
                if (!fireZone.transform.GetChild(0).GetComponent<CoreFire>().endOnNext)
                    fireZone.transform.GetChild(0).GetComponent<CoreFire>().endOnNext = true;
    }


    public override EntityBase Spawn(string[] args, Vector2 spawnPos)
    {
        transform.SetParent(GameManager.gameManagerReference.entitiesContainer.transform);
        transform.position = spawnPos;
        transform.GetChild(0).GetComponent<DamagersCollision>().target = this;
        transform.GetChild(0).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();

        if(args != null)
        {

        }
        else
        {
            targetPosition = transform.position;
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
                ManagingFunctions.DropItem(31, transform.position, Random.Range(1, 4));
                ManagingFunctions.DropItem(93, transform.position, Random.Range(2, 5));
                Despawn();
            }
        }
        else
            Despawn();
    }
}
