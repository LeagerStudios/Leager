using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemController : MonoBehaviour, IDamager
{
    public int item = 0;
    public int amount = 1;
    public float imunityGrab = 0;
    public bool gettingEnabled = true;
    public bool floating = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(floating)
            if(collision.gameObject.layer == 10)
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 1f);
            }

        if (gettingEnabled)
            if (!GameManager.gameManagerReference.isNetworkClient)
                if (imunityGrab <= 0f)
                {
                    if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<PlayerController>().alive)
                    {
                        int itemReturn = 0;

                        if (collision.gameObject.GetComponent<PlayerController>() == GameManager.gameManagerReference.player)
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                if (!StackBar.AddItemInv(item)) itemReturn++;
                            }
                            if (itemReturn == 0)
                            {
                                NetworkController.networkController.UndropItem(gameObject.name);
                                gettingEnabled = false;
                                Destroy(gameObject);
                            }
                            else
                            {
                                amount = itemReturn;
                            }
                        }
                        else
                        {
                            gettingEnabled = false;
                            NetworkController.networkController.DropRequest(item, amount, gameObject.name, collision.gameObject.name);
                        }
                        TechManager.techTree.UnlockBlock(item, true);
                    }
                }

    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.layer == 8)
        {
            GetComponent<EntityCommonScript>().swimming = 1f;
            floating = true;
        }
    }

    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        if (!GameManager.gameManagerReference.isNetworkClient)
            if (damageDeal > 0)
            {
                NetworkController.networkController.UndropItem(gameObject.name);
                Destroy(gameObject);
            }
    }

    private void OnDestroy()
    {
        GameManager.OnWorldRounding -= UpdatePosition;
    }

    private void Start()
    {
        GameManager.OnWorldRounding += UpdatePosition;
    }

    private void Update()
    {
        if (!GameManager.gameManagerReference.isNetworkClient)
            if (amount == 0 || GetComponent<EntityCommonScript>().entityStates.Contains(EntityState.OnFire) || GetComponent<EntityCommonScript>().entityStates.Contains(EntityState.Burning))
            {
                NetworkController.networkController.UndropItem(gameObject.name);
                Destroy(gameObject);
            }

        if (GameManager.gameManagerReference.InGame)
        {

            if (imunityGrab >= 0f)
            {
                imunityGrab -= Time.deltaTime;
            }

            if (transform.position.y < -10)
            {
                NetworkController.networkController.UndropItem(gameObject.name);
                Destroy(gameObject);
            }


            if (GameManager.gameManagerReference.ChunkActive(Mathf.RoundToInt(ManagingFunctions.ClampX(transform.position.x))))
            {
                GetComponent<Rigidbody2D>().simulated = true;
            }
            else
            {
                GetComponent<Rigidbody2D>().simulated = false;
            }

            if (!GameManager.gameManagerReference.isNetworkClient && gettingEnabled)
                if (imunityGrab <= 0f)
                {
                    for (int i = 0; i < transform.parent.childCount; i++)
                    {
                        DroppedItemController itemDrop = transform.parent.GetChild(i).GetComponent<DroppedItemController>();
                        if (itemDrop != this)
                        {
                            if (itemDrop.item == item)
                            {
								if(itemDrop.gettingEnabled)
								{	
									if (itemDrop.imunityGrab < 0f)
									{
										if (Vector2.Distance(itemDrop.transform.position, transform.position) < 1.3f)
										{
											amount += itemDrop.amount;
											itemDrop.amount = 0;
											itemDrop.gettingEnabled = false;
											imunityGrab = (itemDrop.imunityGrab + imunityGrab) / 2f;
											transform.position = Vector2.Lerp(transform.position, itemDrop.transform.position, 0.5f);
											NetworkController.networkController.MoveItem(gameObject.name, transform.position);
                                            NetworkController.networkController.UndropItem(itemDrop.gameObject.name);
											Destroy(itemDrop.gameObject);
										}
									}
								}
                            }
                        }
                    }
                }
        }
        else
        {
            GetComponent<Rigidbody2D>().simulated = false;
        }
    }

    public void UpdatePosition(int i)
    {
        transform.position += new Vector3(i * GameManager.gameManagerReference.WorldWidth * 16, 0);
    }
}
