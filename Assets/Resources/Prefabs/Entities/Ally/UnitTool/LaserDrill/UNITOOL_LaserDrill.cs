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
    public int resourceAmount = 0;


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
            if (blockAlive)
            {
                if (targetPos == Vector2Int.one * -1)
                    blockAlive = false;
                else
                {
                    blockAlive = GameManager.gameManagerReference.GetTileAt(targetPos.x * GameManager.gameManagerReference.WorldHeight + targetPos.y) == resourceToMine;
                }
            }

            if (targetPos == Vector2Int.one * -1)
            {
                int[] grid = (int[])GameManager.gameManagerReference.GetTileObjectAt((int)transform.position.x * GameManager.gameManagerReference.WorldHeight + (int)transform.position.y).transform.parent.GetComponent<ChunkController>().TileGrid.Clone();
                int chunkx = (int)GameManager.gameManagerReference.GetTileObjectAt((int)transform.position.x * GameManager.gameManagerReference.WorldHeight + (int)transform.position.y).transform.parent.position.x;
                float nearest = 1000000f;
                Vector2 nearesT = Vector2Int.one*-1;

                for (int x = 0; x < 16; x++)
                {
                    for (int y = 0; y < GameManager.gameManagerReference.WorldHeight; y++)
                    {
                        
                        int index = x * GameManager.gameManagerReference.WorldHeight + y;
                        float distance = Vector2.Distance(transform.position, new Vector2(x + chunkx, y));

                        if (grid[index] == resourceToMine)
                        {
                            if (distance < nearest)
                            {
                                nearest = distance;
                                nearesT = new Vector2Int(x + chunkx, y);
                            }
                        }
                    }
                }

                if (nearesT != Vector2Int.one*-1)
                {
                    Vector2Int targetPosi = Vector2Int.RoundToInt(nearesT);
                    transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPosi);

                    if (nearest < 100f)
                    {
                        targetPos = targetPosi;
                        if (!unit.PositionControlled)
                        {
                            unit.SetTargetPosition(nearesT);
                            unit.PositionControlled = true;
                            unit.RotationControlled = true;
                            controllingUnit = true;
                            blockAlive = true;
                        }
                    }
                }
            }
            else
            {
                if (!blockAlive)
                {
                    targetPos = Vector2Int.one * -1;

                    if (controllingUnit)
                    {
                        unit.PositionControlled = false;
                        unit.RotationControlled = false;
                        controllingUnit = false;
                    }
                }
                else
                {
                    if (controllingUnit)
                    {
                        unit.SetTargetPosition((Vector3)(Vector2)targetPos - Vector3.left * 2f);
                        unit.transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(unit.transform.position, targetPos);
                        transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPos);
                    }
                    else
                    {
                        transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPos);

                        transform.GetChild(0).localScale = new Vector3(1, Vector2.Distance(transform.GetChild(0).position, targetPos));
                        transform.GetChild(1).localPosition = new Vector2(0, Vector2.Distance(transform.GetChild(0).position, targetPos) + 0.421f);
                    }

                    if (Vector2.Distance(targetPos, transform.position) < 5)
                    {
                        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                        transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = true;
                        transform.GetChild(0).localScale = new Vector3(1, Vector2.Distance(transform.GetChild(0).position, targetPos));
                        transform.GetChild(1).localPosition = new Vector2(0, Vector2.Distance(transform.GetChild(0).position, targetPos) + 0.421f);
                    }
                    else
                    {
                        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                        transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }
        else
        {
            targetPos = Vector2Int.one * -1;

            if (controllingUnit)
            {
                unit.PositionControlled = false;
                unit.RotationControlled = false;
                controllingUnit = false;
            }
        }
    }
}
