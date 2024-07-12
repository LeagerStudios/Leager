using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapPiece : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public bool touched = false;
    public float deleteTime = 25f;
    void Start()
    {
        
    }

    void Update()
    {
        if (touched)
        {
            Color color = spriteRenderer.color;
            color.a = deleteTime / 5f;
            spriteRenderer.color = color;
            deleteTime -= Time.deltaTime;
        }
        if(deleteTime < 0)
        {
            Destroy(gameObject);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == 8)
        {
            touched = true;
        }
    }
}
