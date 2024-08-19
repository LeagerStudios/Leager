using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlockCollider : MonoBehaviour
{
    public Rigidbody2D rb2D;
    public Vector2 size;
    public Vector2 offset;
    int worldHeight = 100;
    public bool canDropPlatform = false;
    public bool updateCollision;

    private void OnValidate()
    {
        Debug.DrawLine(transform.position + (Vector3)(new Vector2(size.x / -2, size.y / -2) + offset), transform.position + (Vector3)(new Vector2(size.x / -2, size.y / 2) + offset), Color.green, 0.1f);
        Debug.DrawLine(transform.position + (Vector3)(new Vector2(size.x / 2, size.y / -2) + offset), transform.position + (Vector3)(new Vector2(size.x / 2, size.y / 2) + offset), Color.green, 0.1f);
        Debug.DrawLine(transform.position + (Vector3)(new Vector2(size.x / -2, size.y / 2) + offset), transform.position + (Vector3)(new Vector2(size.x / 2, size.y / 2) + offset), Color.green, 0.1f);
        Debug.DrawLine(transform.position + (Vector3)(new Vector2(size.x / -2, size.y / -2) + offset), transform.position + (Vector3)(new Vector2(size.x / 2, size.y / -2) + offset), Color.green, 0.1f);
    }

    void Start()
    {
        rb2D = GetComponentInParent<Rigidbody2D>();
    }

    public void FixCollisions()
    {

        worldHeight = GameManager.gameManagerReference.WorldHeight;

        FixCollisionAtPoint(rb2D.transform.position, new Vector2(0, -size.y / 2), rb2D.velocity.x, rb2D.velocity.y, true);
    }

    
    void FixCollisionAtPoint(Vector2 position, Vector2 offset, float dx, float dy, bool isFeet)
    {
        print("Check");
        print(position.x + "-" + position.y);
        int block = GameManager.gameManagerReference.GetTileAt(Mathf.RoundToInt(position.x + offset.x) * worldHeight + Mathf.RoundToInt(position.y + offset.y));
        print(block + "b");
        int layer = GameManager.gameManagerReference.TileCollisionType[block];
        print(layer + "l");


        if(layer == 0)
            return;
        if (layer == 2)
        {
            if (!isFeet)
                return;
            if (canDropPlatform)
                return;
        }
        if (layer == 3)
        {
            return;
        }


        if(dx > 0)
        {
            
        }
        else if (dx < 0)
        {
            
        }

        if (dy > 0)
        {
            
        }
        else if (dy < 0)
        {
            transform.position = new Vector2(position.x, Mathf.RoundToInt(position.y + offset.y) + 1.01f);
            rb2D.velocity = new Vector2(rb2D.velocity.x, 0);
        }
    }

    public void FixedUpdate()
    {
        if (rb2D.simulated)
            updateCollision = true;
    }

    public void LateUpdate()
    {
        if (updateCollision)
        {
            FixCollisions();
            updateCollision = false;
        }
    }
}