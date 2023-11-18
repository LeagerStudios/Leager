using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionEnterDetector : MonoBehaviour {

    IOnCollisionEnterReporter target;
    string tag = "";

    public void InvokeObj(IOnCollisionEnterReporter self, Vector2 size, string t)
    {
        target = self;
        tag = t;
        GetComponent<BoxCollider2D>().size = size;
        Rigidbody2D rb2D = gameObject.AddComponent<Rigidbody2D>();
        rb2D.bodyType = RigidbodyType2D.Static;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.gameObject.CompareTag(tag))
        {
            target.Call(collision);
            Destroy(gameObject);
        }
    }
}

public interface IOnCollisionEnterReporter
{
    void Call(Collider2D collision);
}
