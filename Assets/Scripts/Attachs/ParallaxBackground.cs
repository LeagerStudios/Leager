using System;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float y = 0;
    SpriteRenderer spriteRenderer;
    public Transform target;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void LateUpdate()
    {
        transform.position = new Vector2(target.position.x, y);
    }
}