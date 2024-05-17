using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlockCollider : MonoBehaviour
{
    public Vector2 size;
    public Vector2 offset;
    int worldHeight = 100;
    float x;
    float y;

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

    public BlockCollisionDataOverview FixCollisions()
    {
        BlockCollisionDataOverview collisionDataOverview = new BlockCollisionDataOverview();
        worldHeight = GameManager.gameManagerReference.WorldHeight;
        UpdatePos();

        FixCollisionAtPoint(x, y, true, collisionDataOverview);

        return collisionDataOverview; 
    }

    public void UpdatePos()
    {
        x = transform.position.x + offset.x;
        y = transform.position.y + offset.y;
    }
    
    BlockCollisionData FixCollisionAtPoint(float x, float y, bool isFeet, BlockCollisionDataOverview dataOverview)
    {
        BlockCollisionData data = new BlockCollisionData();
        int block = GameManager.gameManagerReference.GetTileAt(Mathf.FloorToInt(x) * worldHeight + Mathf.FloorToInt(y));

        

        return data;
    }
}

public struct BlockCollisionData
{
    public int block;
    public int blockType;
    public bool fire;
    public bool lava;
}

public struct BlockCollisionDataOverview
{
    public bool touchedSolid;
    public bool touchedLiquid;
}
