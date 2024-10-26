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

    public Sprite[] selected;
    public Sprite[] notSelected;

    public SpriteRenderer laserRenderer;
    public SpriteRenderer laserEndRenderer;
    public ParticleSystem particles;

    public List<int> mineable = new List<int>(new int[] { 8, 9, 10, 11, 12 });

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
                
            lifetime -= Time.deltaTime;
            lifetime = Mathf.Clamp(lifetime, 0f, 1.5f);


            ParticleSystem.MainModule main = particles.main;
            main.startColor = Color.white * lifetime;
            laserRenderer.color = Color.white * lifetime;
            laserEndRenderer.color = Color.white * lifetime;
            audioSource.volume = lifetime;

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
                }
                else if (GInput.GetKeyDown(KeyCode.Return) || (GInput.leagerInput.platform == "Mobile" && GInput.GetMouseButtonDown(0) && !GameManager.gameManagerReference.cancelPlacing))
                {
                    focused = false;
                    GameManager.gameManagerReference.player.mainCamera.focus = null;
                }
                else
                {
                    if (GInput.GetKeyDown(KeyCode.W))
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
                if (!audioSource.isPlaying)
                    audioSource.Play();

                drill.eulerAngles = Vector3.forward * (ManagingFunctions.PointToPivotUp(drill.position, targetPresence.position) + 90);
                laser.localScale = new Vector3(Mathf.Clamp(Vector2.Distance(laser.position, targetPresence.position) - 0.5f, 0.1f, 10f), 1, 1);
            }
            else
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
            }
        }

    }

    public void UpdateEndPoint(EndPointNode endPoint)
    {
        if (endPoint.Power > 0f)
            lifetime = 1.5f;
    }
}
