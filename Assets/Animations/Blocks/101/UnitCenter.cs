using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCenter : MonoBehaviour
{
    SpriteRenderer fenceRend;
    Transform fence;

    void Start()
    {
        fence = transform.GetChild(0);
        fenceRend = fence.GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        fence.transform.localPosition = Vector2.down * (transform.localPosition.y / 2f);
        fenceRend.size = new Vector2(0.875f, transform.localPosition.y + 1);
    }
}
