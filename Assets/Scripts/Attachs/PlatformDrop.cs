using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDrop : MonoBehaviour {

    bool canDrop = false;

    private void OnCollisionStay2D(Collision2D collision)
    {
        canDrop = false;
        foreach (Vector2 vector2 in GameManager.gameManagerReference.platformDrops)
        {
            if (Vector2.Distance(vector2, transform.position) < 1)
            {
                canDrop = true;
            }
        }

        if (GameManager.gameManagerReference.InGame && canDrop)
        {
            StartCoroutine(Drop());
        }
    }

    public IEnumerator Drop()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        GetComponent<BoxCollider2D>().enabled = true;
    }
}
