using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerBomb : MonoBehaviour
{
    [SerializeField] GameObject explosion;
    [SerializeField] AudioClip[] explosionSounds;
    public EntityCommonScript destroyer;
    [SerializeField] Sprite[] animationFrames;
    [SerializeField] SpriteRenderer spriteRenderer;

    void Update()
    {
        if(GameManager.gameManagerReference.frameTimer % 60 == 0)
        {
            spriteRenderer.sprite = animationFrames[0];
        }
        if ((GameManager.gameManagerReference.frameTimer + 30) % 60 == 0)
        {
            spriteRenderer.sprite = animationFrames[1];
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == 8)
        {
            Instantiate(explosion, transform.position, Quaternion.identity).transform.localScale = new Vector3(6f, 6f, 1f);
            GameManager.gameManagerReference.soundController.PlaySfxSound(explosionSounds[Random.Range(0, explosionSounds.Length)], ManagingFunctions.VolumeDistance(Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position), 100));

            foreach (EntityCommonScript entity in GameManager.gameManagerReference.entitiesContainer.GetComponentsInChildren<EntityCommonScript>())
            {
                if (Vector2.Distance(entity.transform.position, transform.position) < 4f)
                {
                    if (entity.gameObject.GetComponent<IDamager>() != null)
                    {
                        entity.gameObject.GetComponent<IDamager>().Hit(15, GetComponent<EntityCommonScript>(), true, 1.5f, true);
                    }
                }
            }

            foreach (EntityCommonScript entity in GameManager.gameManagerReference.dummyObjects.GetComponentsInChildren<EntityCommonScript>())
            {
                if (Vector2.Distance(entity.transform.position, transform.position) < 4f)
                {
                    if (entity.gameObject.GetComponent<PlayerController>() != null)
                    {
                        entity.gameObject.GetComponent<PlayerController>().LoseHp(15, GetComponent<EntityCommonScript>(), true, 1.5f, true);
                    }
                }
            }

            GameManager.gameManagerReference.TileExplosionAt((int)transform.position.x, (int)transform.position.y, 5, 5);
            Destroy(gameObject);
        }
    }
}
