using System;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float y = 0;
    public Transform target;

    public void LateUpdate()
    {
        transform.position = new Vector2(target.position.x, y);
    }
}