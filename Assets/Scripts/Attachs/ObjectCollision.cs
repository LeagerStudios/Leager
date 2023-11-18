using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObjectCollision : MonoBehaviour {

    public ICollisionNotifier target;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        target.OnHitEnter(collision);
    }
}

public interface ICollisionNotifier
{
    void OnHitEnter(Collider2D collision);
}
