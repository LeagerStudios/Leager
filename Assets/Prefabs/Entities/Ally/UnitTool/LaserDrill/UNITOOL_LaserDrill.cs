using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UNITOOL_LaserDrill : MonoBehaviour
{
    public UnitBase unit;
    public bool controllingUnit = false;
    public int resourceToMine = -1;
    public Vector2Int targetPos;
    public bool blockAlive = false;


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
        if (resourceToMine != -1)
        {
            if (targetPos == Vector2Int.one*-1)
            {
                DamagersCollision[] targets = GameManager.gameManagerReference.entitiesContainer.GetComponentsInChildren<DamagersCollision>();
                if (targets.Length != 0)
                {
                    float nearest = 1000000f;
                    DamagersCollision nearesT = null;

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
                        Vector2Int targetPosi = Vector2Int.RoundToInt(nearesT.transform.position);
                        transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPosi);

                        if (nearest < 100f)
                        {
                            targetPos = targetPosi;
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

            else
            {
                if (!blockAlive)
                {
                    targetPos = Vector2Int.one * -1;
                    unit.BeingControlled = false;
                    controllingUnit = false;
                }
                else
                {
                    if (controllingUnit)
                        unit.SetTargetPosition((Vector3)(Vector2)targetPos - unit.transform.forward);
                    transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPos);

                    if (Vector2.Distance(targetPos, transform.position) < 5)
                    {

                    }
                }
            }
        }
    }
}
