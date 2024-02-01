using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UNITOOL_Shooter : MonoBehaviour
{
    public UnitBase unit;
    public bool controllingUnit = false;
    DamagersCollision target = null;


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
        if(target == null)
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
                    if (dis < nearest && target != unit.transform.GetChild(0).GetComponent<DamagersCollision>())
                    {
                        nearest = dis;
                        nearesT = target;
                    }
                }

                if (nearesT != null)
                {
                    Vector2 targetPos = nearesT.transform.position;
                    transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPos);

                    if (nearest < 20f)
                    {
                        unit.SetTargetPosition(nearesT.transform.position);
                        target = nearesT;
                    }
                }
            }
        }
        else
        {
            unit.SetTargetPosition(target.transform.position);
        }
    }
}

