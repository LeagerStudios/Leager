using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UNITOOL_LaserDrill : UnitTool
{
    public UnitBase unit;
    public int resourceToMine = -1;
    public Vector2Int targetPos;
    public bool blockAlive = false;
    public int resourceAmount = 0;
    public ParticleSystem particleSystem;

    private void Start()
    {
        unit = GetComponentInParent<UnitBase>();
        particleSystem = transform.GetChild(1).GetChild(0).GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (GameManager.gameManagerReference.InGame) AiFrame();
    }

    public void AiFrame()
    {
        if (unit.Control == null)
        {
            unit.Control = this;
        }

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
                        if (unit.Control == null)
                        {
                            unit.SetTargetPosition(nearesT);
                            unit.Control = this;
                        }
                        blockAlive = true;
                    }
                }

                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;

                if (!particleSystem.isStopped)
                {
                    particleSystem.Stop();
                }
            }
            else
            {
                if (!blockAlive)
                {
                    targetPos = Vector2Int.one * -1;

                    if (unit.Control == this)
                    {
                        unit.Control = null;
                    }

                    transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                    transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;

                    if (!particleSystem.isStopped)
                    {
                        particleSystem.Stop();
                    }
                }
                else
                {
                    if (unit.Control == this)
                    {
                        if (Vector2.Distance(targetPos, unit.transform.position) > 2.5f)
                        {
                            Vector2 targetPosition = new Vector2(targetPos.x, targetPos.y) - new Vector2(Mathf.Sin(unit.transform.eulerAngles.z * Mathf.Deg2Rad) * -2, Mathf.Cos(unit.transform.eulerAngles.z * Mathf.Deg2Rad) * 2);
                            unit.SetTargetPosition(targetPosition);
                        }

                        unit.transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(unit.transform.position, targetPos);
                        transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPos);
                    }
                    else
                    {
                        transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, targetPos);

                        if (Vector2.Distance(targetPos, unit.transform.position) > 5f)
                        {
                            blockAlive = false;
                        }
                    }

                    if (Vector2.Distance(targetPos, transform.position) < 5)
                    {
                        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                        transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = true;
                        transform.GetChild(0).localScale = new Vector3(1, Vector2.Distance(transform.GetChild(0).position, targetPos) - 0.5f);
                        transform.GetChild(1).localPosition = new Vector2(0, Vector2.Distance(transform.GetChild(0).position, targetPos) - 0.121f);

                        if (!particleSystem.isPlaying)
                        {
                            particleSystem.Play();
                        }
                    }
                    else
                    {
                        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                        transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;

                        if (!particleSystem.isStopped)
                        {
                            particleSystem.Stop();
                        }
                    }
                }
            }
        }
        else
        {
            targetPos = Vector2Int.one * -1;

            if (unit.Control == this)
            {
                unit.Control = null;
            }

            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;

            if (!particleSystem.isStopped)
            {
                particleSystem.Stop();
            }
        }
    }
}
