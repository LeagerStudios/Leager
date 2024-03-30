using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemController : MonoBehaviour, IDamager
{
    public int amount = 1;
    public float imunityGrab = 0;
    public bool gettingEnabled = true;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (gettingEnabled)
            if (!GameManager.gameManagerReference.isNetworkClient)
                if (imunityGrab <= 0f)
                {
                    if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<PlayerController>().alive)
                    {
                        int itemAdd = System.Array.IndexOf(GameManager.gameManagerReference.tiles, GetComponent<SpriteRenderer>().sprite);
                        int itemReturn = 0;

                        if (collision.gameObject.GetComponent<PlayerController>() == GameManager.gameManagerReference.player)
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                if (!StackBar.AddItem(itemAdd)) itemReturn++;
                            }
                            if (itemReturn == 0)
                            {
                                NetworkController.networkController.UndropItem(gameObject.name);
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
                            NetworkController.networkController.DropRequest(itemAdd, amount, gameObject.name, collision.gameObject.name);
                        }
                    }
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


    private void Update()
    {
        if (!GameManager.gameManagerReference.isNetworkClient)
            if (amount == 0 || GetComponent<EntityCommonScript>().entityStates.Contains(EntityState.OnFire))
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

            if (Mathf.Abs(GameManager.gameManagerReference.player.transform.position.x - transform.position.x) < MenuController.menuController.chunkLoadDistance)
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
                            if (itemDrop.GetComponent<SpriteRenderer>().sprite == GetComponent<SpriteRenderer>().sprite)
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
}
