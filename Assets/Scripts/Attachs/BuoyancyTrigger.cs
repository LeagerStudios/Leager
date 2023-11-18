using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyancyTrigger : MonoBehaviour {

    bool canFloat = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        canFloat = false;
        foreach (Vector2 vector2 in GameManager.gameManagerReference.buoyancyFloats)
        {
            if (Vector2.Distance(vector2, transform.position) < 1)
            {
                canFloat = true;
            }
        }

        if (GameManager.gameManagerReference.InGame)
        {
            if (canFloat)
            {
                GetComponent<BuoyancyEffector2D>().density = 10;
            }
            else
            {
                GetComponent<BuoyancyEffector2D>().density = 5;
            }
        }
    }
}
