﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDrill : MonoBehaviour, INodeEndPoint
{
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


    void Start()
    {
        nodeInstance = GetComponent<NodeInstance>();
    }

    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
        {
            laserEnd.localScale = new Vector3(1f / laser.localScale.x, 1f, 1f);

            if (tileProperties == null)
                tileProperties = GetComponentInParent<TileProperties>();
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
                        selectedTile = GameManager.gameManagerReference.GetTileAt(ManagingFunctions.CreateIndex(Vector2Int.FloorToInt(target)));
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
                            focused = true;
                            GameManager.gameManagerReference.player.mainCamera.lerp = true;
                            GameManager.gameManagerReference.player.mainCamera.focus = gameObject;
                            GameManager.gameManagerReference.player.onControl = false;
                        }
                    }
            }


            if (target != Vector2.zero && lifetime > 0f && selectedTile > 0)
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
