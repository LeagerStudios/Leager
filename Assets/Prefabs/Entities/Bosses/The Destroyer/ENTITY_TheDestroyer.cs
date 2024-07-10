using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENTITY_TheDestroyer : EntityBase, IDamager
{
    public Rigidbody2D rb2d;
    public AudioClip shoot;
    [SerializeField] GameObject segment;
    [SerializeField] GameObject tail;
    [SerializeField] LayerMask blockMask;
    public Vector2 velocity;
    float HpMax = 3200;
    float HP = 3200;
    public bool IsIaActive = true;
    public bool DescentIa = false;
    public bool collided = false;
    public bool goDown = false;
    public float descentVelocity = 0;

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
        if (GameManager.gameManagerReference.InGame && IsIaActive) AiFrame();
        else if (GameManager.gameManagerReference.InGame && DescentIa) DescentIaFrame();
    }

    public static EntityBase StaticSpawn(string[] args, Vector2 spawnPos)
    {
        return Instantiate(GameManager.gameManagerReference.EntitiesGameObject[(int)Entities.TheDestroyer], Vector2.zero, Quaternion.identity).transform.GetChild(0).GetComponent<ENTITY_TheDestroyer>().Spawn(args, spawnPos);
    }

    void Start()
    {

    }

    public override string[] GenerateArgs()
    {
        return null;
    }

    public void CheckCollision()
    {
        collided = false;

        if (Physics2D.Raycast(transform.position, rb2d.velocity.normalized, 1f, blockMask))
        {
            Debug.DrawRay(transform.position, rb2d.velocity.normalized, Color.green);
            collided = true;
        }
        else
        {
            Debug.DrawRay(transform.position, rb2d.velocity.normalized, Color.red);
        }
    }

    public override void AiFrame()
    {
        CheckCollision();
        float pointDir = ManagingFunctions.PointToPivotUp(transform.position, GameManager.gameManagerReference.player.transform.position);
        pointDir += 90;
        if ((collided && Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position) > 15f) || transform.position.y < 0)
        {
            if (!GameManager.gameManagerReference.player.alive || GameManager.gameManagerReference.dayLuminosity > 0.5f) Kill(new string[] { "waitForDescent" });
            goDown = false;
            descentVelocity = 0;
            velocity = new Vector2(Mathf.Cos(pointDir * Mathf.Deg2Rad) * 30, Mathf.Sin(pointDir * Mathf.Deg2Rad) * 30);
        }

        if (!goDown)
            velocity = new Vector2(Mathf.Cos(pointDir * Mathf.Deg2Rad) * 30, Mathf.Sin(pointDir * Mathf.Deg2Rad) * 30);

        if (Vector2.Distance(transform.position, GameManager.gameManagerReference.player.transform.position) < 3 || goDown)
        {
            velocity.y = descentVelocity;
            velocity.x = velocity.x - Mathf.Clamp(velocity.x, -0.3f * Time.deltaTime, 0.3f * Time.deltaTime);
            descentVelocity -= 5 * Time.deltaTime;
            goDown = true;
        }

        rb2d.velocity = Vector2.Lerp(rb2d.velocity, velocity, 0.6f * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(transform.eulerAngles.z, ManagingFunctions.PointToPivotUp(Vector2.zero, rb2d.velocity), 0.1f));
        transform.GetChild(0).eulerAngles = new Vector3(0, 0, ManagingFunctions.PointToPivotUp(transform.position, GameManager.gameManagerReference.player.transform.position));
        if (Mathf.Repeat(GameManager.gameManagerReference.frameTimer, 140) == 0)
        {
            PROJECTILE_Laser.StaticSpawn(transform.GetChild(0).eulerAngles.z + 5, transform.GetChild(0).position, 0, GetComponent<EntityCommonScript>());
            PROJECTILE_Laser.StaticSpawn(transform.GetChild(0).eulerAngles.z - 5, transform.GetChild(0).position, 0, GetComponent<EntityCommonScript>());
            GameManager.gameManagerReference.soundController.PlaySfxSound(shoot, ManagingFunctions.VolumeDistance(Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position), 60));
        }
        if (Vector2.Distance(transform.position, GameManager.gameManagerReference.player.transform.position) < 1.5f)
            GameManager.gameManagerReference.player.LoseHp(15, GetComponent<EntityCommonScript>());


        Transform previousSegment = transform;
        for (int i = 1; i < transform.parent.childCount; i++)
        {
            Transform indexSegment = transform.parent.GetChild(i);
            float angle = ManagingFunctions.PointToPivotUp(indexSegment.position, previousSegment.position);
            indexSegment.eulerAngles = new Vector3(0, 0, angle);
            indexSegment.position = previousSegment.position;
            if (i == 1) indexSegment.position -= indexSegment.up * 1.6f;
            else if (i == transform.parent.childCount - 1) indexSegment.position -= indexSegment.up * 1.6f;
            else indexSegment.position -= indexSegment.up * 1.3f;
            if (i != transform.parent.childCount - 1)
            {
                indexSegment.GetChild(0).eulerAngles = new Vector3(0, 0, ManagingFunctions.PointToPivotUp(indexSegment.position, GameManager.gameManagerReference.player.transform.position));
                if (Random.Range(0, 5065) == 0)
                {
                    PROJECTILE_Laser.StaticSpawn(indexSegment.GetChild(0).eulerAngles.z, indexSegment.GetChild(0).position, 0, GetComponent<EntityCommonScript>());
                    GameManager.gameManagerReference.soundController.PlaySfxSound(shoot, ManagingFunctions.VolumeDistance(Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position), 60));
                }

                if (Vector2.Distance(indexSegment.position, GameManager.gameManagerReference.player.transform.position) < 1.5f)
                    GameManager.gameManagerReference.player.LoseHp(5, GetComponent<EntityCommonScript>());
            }
            else
            {
                indexSegment.GetChild(1).eulerAngles = new Vector3(0, 0, ManagingFunctions.PointToPivotUp(indexSegment.position, GameManager.gameManagerReference.player.transform.position));
                if (Vector2.Distance(GameManager.gameManagerReference.player.transform.position, indexSegment.GetChild(1).position) < 7)
                    if (Mathf.Repeat(GameManager.gameManagerReference.frameTimer, 13) == 0)
                    {
                        PROJECTILE_Laser.StaticSpawn(indexSegment.GetChild(1).eulerAngles.z, indexSegment.GetChild(1).position, 0, GetComponent<EntityCommonScript>());
                        GameManager.gameManagerReference.soundController.PlaySfxSound(shoot, ManagingFunctions.VolumeDistance(Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position), 60));
                    }
                if (Vector2.Distance(indexSegment.position, GameManager.gameManagerReference.player.transform.position) < 1.5f)
                    GameManager.gameManagerReference.player.LoseHp(16, GetComponent<EntityCommonScript>());
            }

            previousSegment = indexSegment;
        }
    }

    public void DescentIaFrame()
    {
        if (transform.position.y < 0 - (transform.parent.childCount + 10)) Kill(null);
        velocity.y = descentVelocity;
        velocity.x = velocity.x - Mathf.Clamp(velocity.x, -0.3f * Time.deltaTime, 0.3f * Time.deltaTime);
        descentVelocity -= 5 * Time.deltaTime;

        rb2d.velocity = Vector2.Lerp(rb2d.velocity, velocity, 0.1f);
        transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(transform.eulerAngles.z, ManagingFunctions.PointToPivotUp(Vector2.zero, rb2d.velocity), 0.1f));
        if (Vector2.Distance(transform.position, GameManager.gameManagerReference.player.transform.position) < 1.5f)
            GameManager.gameManagerReference.player.LoseHp(15, GetComponent<EntityCommonScript>());

        Transform previousSegment = transform;
        for (int i = 1; i < transform.parent.childCount; i++)
        {
            Transform indexSegment = transform.parent.GetChild(i);
            float angle = ManagingFunctions.PointToPivotUp(indexSegment.position, previousSegment.position);
            indexSegment.eulerAngles = new Vector3(0, 0, angle);
            indexSegment.position = previousSegment.position;
            if (i == 1) indexSegment.position -= indexSegment.up * 1.6f;
            else if (i == transform.parent.childCount - 1) indexSegment.position -= indexSegment.up * 1.6f;
            else indexSegment.position -= indexSegment.up * 1.3f;
            if (i != transform.parent.childCount - 1)
            {
                if (Vector2.Distance(indexSegment.position, GameManager.gameManagerReference.player.transform.position) < 1.5f)
                    GameManager.gameManagerReference.player.LoseHp(5, GetComponent<EntityCommonScript>());
            }
            else
            {
                if (Vector2.Distance(indexSegment.position, GameManager.gameManagerReference.player.transform.position) < 1.5f)
                    GameManager.gameManagerReference.player.LoseHp(16, GetComponent<EntityCommonScript>());
            }
            previousSegment = indexSegment;
        }
    }

    public override EntityBase Spawn(string[] args, Vector2 spawnPos)
    {
        transform.parent.SetParent(GameManager.gameManagerReference.entitiesContainer.transform);
        transform.position = spawnPos;
        transform.GetChild(1).GetComponent<DamagersCollision>().target = this;
        transform.GetChild(1).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();
        for (int i = 0; i < 50; i++)
        {
            GameObject clonedSegment = Instantiate(segment, spawnPos, Quaternion.identity, transform.parent);
            clonedSegment.transform.GetChild(1).GetComponent<DamagersCollision>().target = this;
            clonedSegment.transform.GetChild(1).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();
        }

        GameObject tailSegment = Instantiate(tail, spawnPos, Quaternion.identity, transform.parent);
        tailSegment.transform.GetChild(2).GetComponent<DamagersCollision>().target = this;
        tailSegment.transform.GetChild(2).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();

        DescentIa = false;
        IsIaActive = true;
        return this;
    }

    public override void Despawn()
    {
        Destroy(transform.parent.gameObject);
    }


    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        Hp = Hp - damageDeal;
        HealthBarManager.self.UpdateHealthBar(transform, HP, HpMax, Vector2.up);
    }

    public override void Kill(string[] args)
    {
        if (args != null)
        {
            if(args[0] == "waitForDescent")
            {
                IsIaActive = false;
                DescentIa = true;
            }
            if(args[0] == "dead")
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

                ManagingFunctions.DropItem(65, targetSegment.position, amount: Random.Range(20, 40));
                ManagingFunctions.DropItem(34, targetSegment.position, amount: Random.Range(5, 10));
                Despawn();
            }
        }
        else
            Despawn();
    }
}
