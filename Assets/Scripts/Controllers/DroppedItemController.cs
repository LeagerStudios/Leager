using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemController : MonoBehaviour, IDamager {

    private float seconds = 0;
    public int amount = 1;
    public float imunityGrab = 0;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (imunityGrab <= 0f)
        {
            if (collision.gameObject.CompareTag("Player") && GameManager.gameManagerReference.player.alive)
            {
                int itemAdd = System.Array.IndexOf(GameManager.gameManagerReference.tiles, GetComponent<SpriteRenderer>().sprite);
                int itemReturn = 0;
                for (int i = 0; i < amount; i++)
                {
                    if (!StackBar.AddItem(itemAdd)) itemReturn++;
                }
                if (itemReturn == 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    amount = itemReturn;
                }
            }
        }
       
    }

    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        if (damageDeal > 0)
            Destroy(gameObject);
    }

    private void Update()
    {
        if (amount == 0 || GetComponent<EntityCommonScript>().entityStates.Contains(EntityState.OnFire)) Destroy(gameObject);

        if (GameManager.gameManagerReference.InGame)
        {
            seconds += Time.deltaTime;

            if (imunityGrab >= 0f)
            {
                imunityGrab -= Time.deltaTime;
            }

            if (transform.position.y < -10)
            {
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

            if (imunityGrab <= 0f)
            {
                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    DroppedItemController itemDrop = transform.parent.GetChild(i).GetComponent<DroppedItemController>();
                    if (itemDrop != this)
                    {
                        if (itemDrop.imunityGrab < 0f)
                        {
                            if (Vector2.Distance(itemDrop.transform.position, transform.position) < 1.3f)
                            {
                                if (itemDrop.GetComponent<SpriteRenderer>().sprite == GetComponent<SpriteRenderer>().sprite)
                                {
                                    amount += itemDrop.amount;
                                    itemDrop.amount = 0;
                                    imunityGrab = (itemDrop.imunityGrab + imunityGrab) / 2f;
                                    transform.position = Vector2.Lerp(transform.position, itemDrop.transform.position, 0.5f);
                                    seconds = 0;
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
