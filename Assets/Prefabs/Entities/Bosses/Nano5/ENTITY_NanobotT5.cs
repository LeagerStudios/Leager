using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENTITY_NanobotT5 : EntityBase, IDamager
{
    public Transform head;
    public Transform pulse;
    public Transform legs;
    public GameObject leg;
    public GameObject feet;
    public Transform laser;
    public Transform laserDepresion;
    public Transform laserTail;
    public SpriteRenderer laserImpact;
    public EntityCommonScript entityScript;

    public int legCount = 4;
    public int legSegmentCount = 4;
    public float legLength = 0.79f;
    public float legSpeed = 3f;
    public float speed = 3f;
    public Vector2[] legTargets;
    public EntityCommonScript target;
    public LayerMask block;
    public float legRange;

    int legIterator = 0;
    public bool laserActive = false;

    float HpMax = 10f;
    float HP = 10f;


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

    public override string[] GenerateArgs()
    {
        return null;
    }

    public static EntityBase StaticSpawn(string[] args, Vector2 spawnPos)
    {
        return Instantiate(GameManager.gameManagerReference.EntitiesGameObject[(int)Entities.NanoBotT5], Vector2.zero, Quaternion.identity).GetComponent<ENTITY_NanobotT5>().Spawn(args, spawnPos);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
        {
            AiFrame();
        }
    }

    public override void AiFrame()
    {
        int legsOnTarget = 0;
        Vector2 lastPositon = head.position;

        if (target)
        {
            head.position = ManagingFunctions.MoveTowardsTarget(head.position, target.transform.position, speed * Time.deltaTime);

            if (GameManager.gameManagerReference.frameTimer % 600 == 0)
            {
                laserActive = true;
            }

            if (GameManager.gameManagerReference.frameTimer % 720 == 0)
            {
                laserActive = false;
            }

            for (int i = 0; i < legs.childCount; i++)
            {
                Transform leg = legs.GetChild(i);
                Transform foot = leg.GetChild(legSegmentCount);

                Vector2 startPosition = head.position + leg.localPosition;
                foot.position = ManagingFunctions.MoveTowardsTarget(foot.position, legTargets[i], (legSpeed + speed) * Time.deltaTime);
                Vector2 footLocalPosition = (Vector2)foot.position - startPosition;
                Vector2 footNewLocalPosition = Vector2.ClampMagnitude(footLocalPosition, legRange);
                foot.position = footNewLocalPosition + startPosition;
                if (footLocalPosition == footNewLocalPosition && (Vector2)foot.position == legTargets[i]) legsOnTarget++;
                Vector3 dir = Vector3.forward * ManagingFunctions.PointToPivotDown(foot.position, startPosition);
                foot.eulerAngles = dir;
                float juan2 = Vector2.Distance(startPosition, foot.position);

                for (int e = leg.childCount - 1; e >= 0; e--)
                {
                    Transform indexSegment = leg.GetChild(e);

                    if (e != legSegmentCount)
                    {
                        float juan = 1f / legSegmentCount * e;
                        indexSegment.position = Vector2.Lerp(startPosition, foot.position, juan);
                        indexSegment.eulerAngles = dir;
                        indexSegment.localScale = new Vector3(1, juan2 / legRange, 1);
                    }
                }
            }

            laser.gameObject.SetActive(laserDepresion.localScale.x != 0);

            if (laserActive)
            {
                if (laserDepresion.localScale.x < 1f)
                {
                    laserDepresion.localScale = new Vector2(Mathf.Clamp(laserDepresion.localScale.x + Time.deltaTime * 3, 0f, 1f), 1f);
                }

                laserDepresion.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(laserDepresion.position, target.transform.position);
                float laserLength = 100f;
                RaycastHit2D rayHit = Physics2D.Raycast(laserTail.position, target.transform.position - laser.position, 100f, block);
                if (rayHit)
                {
                    laserLength = rayHit.distance;
                    laserImpact.transform.position = rayHit.point;
                    laserImpact.transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(Vector2.zero, rayHit.normal);
                    laserImpact.enabled = true;
                }
                else
                {
                    laserImpact.enabled = false;
                }

                laserTail.localScale = new Vector2(1f, laserLength);
            }
            else
            {
                if (laserDepresion.localScale.x > 0f)
                {
                    laserDepresion.localScale = new Vector2(Mathf.Clamp(laserDepresion.localScale.x - Time.deltaTime * 3, 0f, 1f), 1f);
                }
            }
            laserImpact.transform.localScale = laserDepresion.localScale;

            if (legsOnTarget == 0)
            {
                head.position = lastPositon;
            }

            if (GameManager.gameManagerReference.frameTimer % (int)(15 * speed) == 0)
            {
                RaycastHit2D rayHit = Physics2D.Raycast(head.position, target.transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f)) - head.position, legRange + 0.55f, block);

                if (rayHit)
                {
                    legTargets[legIterator] = rayHit.point;
                    legIterator++;
                    if (legIterator == legCount)
                        legIterator = 0;

                }
            }
        }
        else
        {
            laserActive = false;
        }
    }

    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        Hp = Hp - damageDeal;
    }

    public override void Kill(string[] args)
    {
        Despawn();
    }

    public override void Despawn()
    {
        Destroy(gameObject);
    }

    public override EntityBase Spawn(string[] args, Vector2 spawnPos)
    {
        head.GetComponent<DamagersCollision>().target = this;
        //transform.GetChild(0).GetComponent<DamagersCollision>().entity = GetComponent<EntityCommonScript>();
        transform.SetParent(GameManager.gameManagerReference.entitiesContainer.transform);
        head.position = spawnPos;
        entityScript = GetComponent<EntityCommonScript>();
        BuildLegs();
        target = GameManager.gameManagerReference.player.entityScript;
        Active = true;
        return this;
    }

    void BuildLegs()
    {
        float degreesPerLeg = 360 / legCount;
        legTargets = new Vector2[legCount];

        for(int i = 0; i < legCount; i++)
        {
            float direction = degreesPerLeg * i - 45f;
            float x = Mathf.Clamp(-Mathf.Sin(Mathf.Deg2Rad * direction), -0.2f, 0.2f);
            float y = Mathf.Clamp(Mathf.Cos(Mathf.Deg2Rad * direction), -0.35f, 0.35f);

            GameObject newLeg = new GameObject("Leg" + i);
            newLeg.transform.parent = legs;
            newLeg.transform.localPosition = new Vector2(x, y);
            Transform lastLeg = null;

            for(int e = 0; e < legSegmentCount; e++)
            {
                Vector2 legPlacePos = newLeg.transform.position;
                if (lastLeg != null) legPlacePos = lastLeg.GetChild(0).position;
                GameObject legSegment = Instantiate(leg, newLeg.transform, false);
                legSegment.transform.position = legPlacePos;
                legSegment.transform.eulerAngles = Vector3.forward * direction;
                legSegment.SetActive(true);
                lastLeg = legSegment.transform;
            }

            Vector2 placePos = lastLeg.GetChild(0).position;
            GameObject feetSegment = Instantiate(feet, newLeg.transform, false);
            feetSegment.transform.position = placePos;
            feetSegment.transform.eulerAngles = Vector3.forward * direction;
            feetSegment.SetActive(true);
        }

        legRange = legSegmentCount * legLength;
    }
}
