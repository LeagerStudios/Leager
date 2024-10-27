using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDrill : MonoBehaviour, INodeEndPoint
{
    public Transform support;
    public Transform drill;
    public Transform laser;
    public Transform targetPresence;

    public Transform laserEnd;
    public NodeInstance nodeInstance;
    public TileProperties tileProperties;
    public Vector2 target = Vector2.zero;
    public bool focused = false;

    public AudioSource audioSource;

    public float lifetime = 0f;
    public int selectedTile = 0;
    public float miningProgress;

    public Sprite[] selected;
    public Sprite[] notSelected;

    public SpriteRenderer laserRenderer;
    public SpriteRenderer laserEndRenderer;
    public ParticleSystem particles;

    public List<int> mineable = new List<int>(new int[] { 11, 10, 9, 8, 12 });

    void Start()
    {
        nodeInstance = GetComponent<NodeInstance>();
        if(GameManager.gameManagerReference.GetTileAt(ManagingFunctions.CreateIndex(Vector2Int.FloorToInt((Vector2)transform.position - Vector2.up))) == 125)
        {
            support.localPosition = new Vector2(0.22f, 0);
        }
    }

    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
        {
            laserEnd.localScale = new Vector3(1f / laser.localScale.x, 1f, 1f);

            if (tileProperties == null)
            {
                tileProperties = GetComponentInParent<TileProperties>();

                if (tileProperties.info.Count > 0)
                {
                    int[] data = ManagingFunctions.ConvertStringToIntArray(tileProperties.info[0].Split(':'));

                    target = new Vector2(data[0], data[1]);
                    targetPresence.localPosition = target;
                    drill.eulerAngles = Vector3.forward * (ManagingFunctions.PointToPivotUp(drill.position, targetPresence.position) + 90);
                }
            }

            targetPresence.localPosition = target;

            if (focused)
            {
                targetPresence.gameObject.SetActive(true);
                tileProperties.info = new List<string>();
                tileProperties.info.Add(target.x + ":" + target.y);
                tileProperties.CommitToChunk();

                if (StackBar.stackBarController.InventoryDeployed || !GameManager.gameManagerReference.InGame || Vector2.Distance(transform.position, GameManager.gameManagerReference.player.transform.position) > 5)
                {
                    focused = false;
                    GameManager.gameManagerReference.player.mainCamera.focus = null;
                    miningProgress = 0f;
                }
                else if (GInput.GetKeyDown(KeyCode.Return) || (GInput.leagerInput.platform == "Mobile" && GInput.GetMouseButtonDown(0) && !GameManager.gameManagerReference.cancelPlacing))
                {
                    focused = false;
                    GameManager.gameManagerReference.player.mainCamera.focus = null;
                    miningProgress = 0f;
                }
                else
                {
                    if ((GInput.leagerInput.platform != "Mobile" && GInput.GetKeyDown(KeyCode.W)) || (GInput.leagerInput.platform == "Mobile" && GInput.GetKeyDown(KeyCode.Space)))
                    {
                        if (target.y < 5)
                            target += Vector2.up;
                    }
                    if (GInput.GetKeyDown(KeyCode.A))
                    {
                        if (target.x > -5)
                            target += Vector2.left;
                    }
                    if (GInput.GetKeyDown(KeyCode.S))
                    {
                        if (target.y > -5)
                            target += Vector2.down;
                    }
                    if (GInput.GetKeyDown(KeyCode.D))
                    {
                        if (target.x < 5)
                            target += Vector2.right;
                    }
                }
            }
            else
            {
                targetPresence.gameObject.SetActive(false);
                if (GInput.GetMouseButtonDown(1) || (GInput.leagerInput.platform == "Mobile" && GInput.GetMouseButtonDown(0) && !GameManager.gameManagerReference.cancelPlacing))
                    if (GameManager.gameManagerReference.player.onControl && ManagingFunctions.InsideRanges(GameManager.gameManagerReference.mouseCurrentPosition, transform.position - (Vector3.one * 0.5f), transform.position + (Vector3.one * 0.5f)))
                    {
                        if (!StackBar.stackBarController.InventoryDeployed)
                        {
                            if (tileProperties.storedItems.Count > 0)
                            {
                                string item = tileProperties.storedItems[0];
                                tileProperties.storedItems.RemoveAt(0);

                                int[] data = ManagingFunctions.ConvertStringToIntArray(item.Split(':'));
                                int amount = data[1];

                                for(int i = 0; i < data[1]; i++)
                                {
                                    if (!StackBar.AddItemInv(data[0]))
                                    {
                                        break;
                                    }
                                    amount--;
                                }

                                if(amount > 0)
                                {
                                    GameManager.gameManagerReference.player.PlayerRelativeDrop(data[0], amount);
                                }
                                    
                            }
                            else
                            {
                                focused = true;
                                GameManager.gameManagerReference.player.mainCamera.lerp = true;
                                GameManager.gameManagerReference.player.mainCamera.focus = gameObject;
                                GameManager.gameManagerReference.player.onControl = false;
                            }
                        }
                    }
            }

            selectedTile = GameManager.gameManagerReference.GetTileAt(ManagingFunctions.CreateIndex(Vector2Int.FloorToInt(target + (Vector2)transform.position)));
            bool canMine = mineable.Contains(selectedTile);

            lifetime -= Time.deltaTime;
            lifetime = Mathf.Clamp(lifetime, 0f, 1.5f);

            if (focused)
                if (canMine)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        targetPresence.GetChild(i).GetComponent<SpriteRenderer>().sprite = selected[i];
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        targetPresence.GetChild(i).GetComponent<SpriteRenderer>().sprite = notSelected[i];
                    }
                }

            if (target != Vector2.zero && lifetime > 0f && canMine && !focused)
            {
                float targetProgress = Mathf.Pow(mineable.IndexOf(selectedTile) + 2, 2);

                miningProgress += Time.deltaTime;

                if (!audioSource.isPlaying)
                    audioSource.Play();

                drill.eulerAngles = Vector3.forward * (ManagingFunctions.PointToPivotUp(drill.position, targetPresence.position) + 90);
                laser.localScale = new Vector3(Mathf.Clamp(Vector2.Distance(laser.position, targetPresence.position) - 0.5f, 0.1f, 10f), 1, 1);

                if(miningProgress > targetProgress)
                {
                    miningProgress = 0;

                    if(selectedTile == 11 && GameManager.gameManagerReference.GetTileAt(ManagingFunctions.CreateIndex(Vector2Int.FloorToInt((Vector2)transform.position - Vector2.up))) == 125)
                    {
                        GameManager.gameManagerReference.GetTileObjectAt(ManagingFunctions.CreateIndex(Vector2Int.FloorToInt((Vector2)transform.position - Vector2.up))).transform.GetComponentInChildren<EnergyGenerator>().shock = true;
                    }
                    else
                    {
                        int endingTile = GameManager.gameManagerReference.SwitchTroughBlockBroke(selectedTile, Vector2Int.RoundToInt(targetPresence.position), false);

                        if (tileProperties.storedItems.Count > 0)
                        {
                            int[] data = ManagingFunctions.ConvertStringToIntArray(tileProperties.storedItems[tileProperties.storedItems.Count - 1].Split(':'));
                            
                            if (data[0] != endingTile || data[1] > GameManager.gameManagerReference.stackLimit[data[0]])
                            {
                                tileProperties.storedItems.Add(endingTile + ":" + 1);
                            }
                            else
                            {
                                tileProperties.storedItems.RemoveAt(tileProperties.storedItems.Count - 1);
                                tileProperties.storedItems.Add(data[0] + ":" + (data[1] + 1));
                            }
                        }
                        else
                        {
                            tileProperties.storedItems.Add(endingTile + ":" + 1);
                        }

                        tileProperties.CommitToChunk();
                    }
                }

                HealthBarManager.self.UpdateHealthBar(transform, miningProgress, targetProgress, Vector2.up * 0.5f);
            }
            else
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
                lifetime = 0f;
            }

            ParticleSystem.MainModule main = particles.main;
            main.startColor = new Color(1, 1, 1, lifetime);
            laserRenderer.color = Color.white * lifetime;
            laserEndRenderer.color = Color.white * lifetime;
            audioSource.volume = lifetime;
        }

    }

    public void UpdateEndPoint(EndPointNode endPoint)
    {
        if (endPoint.Power > 0f)
            lifetime = 1.5f;
    }
}
