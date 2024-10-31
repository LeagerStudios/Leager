using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundEffect : MonoBehaviour
{
    void Update()
    {
        transform.localPosition = Vector3.zero;
        transform.position *= 0.9f;
        transform.localPosition = new Vector2(transform.localPosition.x % 1, transform.localPosition.y % 1);
    }
}
