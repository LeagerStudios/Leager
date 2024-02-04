using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UNITOOL_Shooter : MonoBehaviour
{
    public UnitBase unit;
    public bool controllingUnit = false;
    DamagersCollision target = null;
    public bool targetAlive = false;


    private void Start()
    {
        unit = GetComponentInParent<UnitBase>();
    }

    private void Update()
    {
        if (GameManager.gameManagerReference.InGame) AiFrame();
    }

    public void AiFrame()
    {
        if (target == null && !targetAlive)
        {
            DamagersCollision[] targets = GameManager.gameManagerReference.entitiesContainer.GetComponentsInChildren<DamagersCollision>();
            if (targets.Length != 0)
            {
                float nearest = 1000000f;
                DamagersCollision nearesT = null;
                ;
                foreach (DamagersCollision target in targets)
                {
                    float dis = Vector2.Distance(transform.position, target.transform.position);
                    if (dis < nearest && target.team != "ally")
                    {
                        nearest = dis;
                        nearesT = target;
                    }
                }

                if (nearesT != null)
                {
                    Vector2 targetPos = nearesT.transform.position;
                    transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPos);

                    if (nearest < 100f)
                    {
                        target = nearesT;
                        if (!unit.BeingControlled)
                        {
                            unit.SetTargetPosition(nearesT.transform.position);
                            unit.BeingControlled = true;
                            controllingUnit = true;
                        }
                    }
                }
            }
        }
        else if (targetAlive)
        {
            targetAlive = false;
            unit.BeingControlled = false;
            controllingUnit = false;
        }
        else
        {
            if (controllingUnit)
                unit.SetTargetPosition(target.transform.position);
            transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, target.transform.position);

            if (Mathf.Repeat(GameManager.gameManagerReference.frameTimer, 10) == 0)
            {
                if (Vector2.Distance(target.transform.position, transform.position) < 15)
                {
                    PROJECTILE_Arrow.StaticSpawn(-transform.eulerAngles.z, transform.position, 5, unit.GetComponent<EntityCommonScript>());
                }
            }
        }
    }
}

