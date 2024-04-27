using System;
using UnityEngine;

public class KeepY : MonoBehaviour
{
    public float y = 0;

    public void LateUpdate()
    {
        transform.position = new Vector2(transform.position.x, y);
    }
}