using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSand : MonoBehaviour
{
    public bool started = false;
    public bool actived = true;

    private void Update()
    {
        int idx = (int)transform.position.x * GameManager.gameManagerReference.WorldHeight + (int)transform.position.y;

        if (!started)
        {
            GetComponent<Rigidbody2D>().simulated = true;
            started = true;
        }
        else if (actived)
        {
            if (GameManager.gameManagerReference.tileDefaultBrokeTool[GameManager.gameManagerReference.GetTileAt(idx)] != "replace")
                if (GameManager.gameManagerReference.tileDefaultBrokeTool[GameManager.gameManagerReference.GetTileAt(idx + 1)] == "replace")
                {
                    GameManager.gameManagerReference.SetTileAt(idx + 1, 113, false, true);
                    actived = false;
                    Destroy(gameObject);
                }
        }
    }
}
