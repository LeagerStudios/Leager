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
    float x;
    float y;
    public bool canDropPlatform = false;

    private void OnValidate()
    {
        Debug.DrawLine(transform.position + (Vector3)(new Vector2(size.x / -2, size.y / -2) + offset), transform.position + (Vector3)(new Vector2(size.x / -2, size.y / 2) + offset), Color.green, 0.1f);
        Debug.DrawLine(transform.position + (Vector3)(new Vector2(size.x / 2, size.y / -2) + offset), transform.position + (Vector3)(new Vector2(size.x / 2, size.y / 2) + offset), Color.green, 0.1f);
        Debug.DrawLine(transform.position + (Vector3)(new Vector2(size.x / -2, size.y / 2) + offset), transform.position + (Vector3)(new Vector2(size.x / 2, size.y / 2) + offset), Color.green, 0.1f);
        Debug.DrawLine(transform.position + (Vector3)(new Vector2(size.x / -2, size.y / -2) + offset), transform.position + (Vector3)(new Vector2(size.x / 2, size.y / -2) + offset), Color.green, 0.1f);
    }

    void Start()
    {
        
    }

    public BlockCollisionDataOverview FixCollisions(float dx, float dy)
    {
        BlockCollisionDataOverview collisionDataOverview = new BlockCollisionDataOverview();
        worldHeight = GameManager.gameManagerReference.WorldHeight;
        UpdatePos();

        FixCollisionAtPoint(x, y, dx, dy, true, ref collisionDataOverview);

        return collisionDataOverview; 
    }

    public void UpdatePos()
    {
        x = transform.position.x + offset.x;
        y = transform.position.y + offset.y;
    }
    
    void FixCollisionAtPoint(float x, float y, float dx, float dy, bool isFeet, ref BlockCollisionDataOverview dataOverview)
    {
        print(GameManager.gameManagerReference.frameTimer);
        int block = GameManager.gameManagerReference.GetTileAt(Mathf.FloorToInt(x) * worldHeight + Mathf.FloorToInt(y));
        print(block + "b");
        int layer = GameManager.gameManagerReference.TileCollisionType[block];
        print(layer + "l");
        float modx = x % 1;
        float mody = y % 1;
        print(modx);
        print(mody);


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
            dataOverview.touchedLiquid = true;
            return;
        }

        dataOverview.touchedSolid = true;
        print("quak");

        if(dx > 0)
        {
            transform.Translate(new Vector2(-modx - 0.00001f, 0));
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
            print("quak1");
            UpdatePos();
        }
        else if (dx < 0)
        {
            transform.Translate(new Vector2(1 - modx, 0));
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
            UpdatePos();
            print("quak2");
        }

        if (dy > 0)
        {
            transform.Translate(new Vector2(0, -mody - 0.00001f));
            rb2D.velocity = new Vector2(rb2D.velocity.x, 0);
            UpdatePos();
            print("quak3");
        }
        else if (dy < 0)
        {
            transform.Translate(new Vector2(0, 1 - mody));
            rb2D.velocity = new Vector2(rb2D.velocity.x, 0);
            UpdatePos();

            if (isFeet)
                dataOverview.grounded = true;

            print("quak4");
        }
    }
}

public struct BlockCollisionDataOverview
{
    public bool touchedSolid;
    public bool touchedLiquid;
    public bool grounded;
}
