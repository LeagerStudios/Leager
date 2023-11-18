using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidPhysicsController : MonoBehaviour {

    [SerializeField] Sprite[] liquidFrames;
    public ChunkController chunk;
    public int idx;
    public int tile;
    public int fluidness;

	public void StartPhy (Sprite[] frames)
    {
        liquidFrames = frames;
        idx = transform.GetSiblingIndex();
        chunk = transform.parent.GetComponent<ChunkController>();
        tile = chunk.TileGrid[idx];
        fluidness = GameManager.gameManagerReference.ToolEfficency[tile];
    }
	
	void Update ()
    {
        if (Mathf.Repeat(GameManager.gameManagerReference.frameTimer, fluidness) == 0)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            int up = GameManager.gameManagerReference.GetTileAt(chunk.tilesToChunk + idx + 1);
            int down = GameManager.gameManagerReference.GetTileAt(chunk.tilesToChunk + idx - 1);
            int right = GameManager.gameManagerReference.GetTileAt(chunk.tilesToChunk + idx + GameManager.gameManagerReference.WorldHeight);
            int left = GameManager.gameManagerReference.GetTileAt(chunk.tilesToChunk + idx - GameManager.gameManagerReference.WorldHeight);
            spriteRenderer.flipX = false;

            if(up == 0)
            {
                if(left == 0 && right == 0)
                {
                    if(down == 0 || down == tile)
                    {
                        spriteRenderer.sprite = liquidFrames[10];
                    }
                    else
                    {
                        spriteRenderer.sprite = liquidFrames[0];
                    }
                }
                else if(left == 0 || left == tile)
                {
                    GameObject leftO = GameManager.gameManagerReference.GetTileObjectAt(chunk.tilesToChunk + idx - GameManager.gameManagerReference.WorldHeight);
                    Sprite sprite = leftO.GetComponent<SpriteRenderer>().sprite;
                    int indexOf = System.Array.IndexOf(liquidFrames, leftO.GetComponent<SpriteRenderer>().sprite);

                    if (indexOf < 8 && indexOf != -1)
                    {
                        spriteRenderer.sprite = liquidFrames[indexOf + 1];
                    }
                    else if(indexOf == 8)
                    {
                        chunk.TileGrid[idx] = 0;
                        spriteRenderer.sprite = GameManager.gameManagerReference.tiles[0];
                    }
                    else
                    {
                        spriteRenderer.sprite = liquidFrames[0];
                    }
                }
                else if(right == 0 || right == tile)
                {
                    GameObject rightO = GameManager.gameManagerReference.GetTileObjectAt(chunk.tilesToChunk + idx + GameManager.gameManagerReference.WorldHeight);

                    Sprite sprite = rightO.GetComponent<SpriteRenderer>().sprite;
                    int indexOf = System.Array.IndexOf(liquidFrames, rightO.GetComponent<SpriteRenderer>().sprite);

                    if (indexOf < 8 && indexOf != -1)
                    {
                        spriteRenderer.sprite = liquidFrames[indexOf + 1];
                    }
                    else if (indexOf == 8)
                    {
                        chunk.TileGrid[idx] = 0;
                        spriteRenderer.sprite = GameManager.gameManagerReference.tiles[0];
                    }
                    else
                    {
                        spriteRenderer.sprite = liquidFrames[0];
                    }
                    spriteRenderer.flipX = true;
                }
                else
                {
                    spriteRenderer.sprite = liquidFrames[13];
                }
            }
            else if(up == tile)
            {
                if (left == 0 && right == 0)
                {
                    if (down == 0 || down == tile)
                    {
                        spriteRenderer.sprite = liquidFrames[9];
                    }
                    else
                    {
                        spriteRenderer.sprite = liquidFrames[0];
                    }
                }
                else
                    spriteRenderer.sprite = liquidFrames[0];
            }
            else
            {
                if (left == 0 && right == 0)
                {
                    if (down == 0 || down == tile)
                    {
                        spriteRenderer.sprite = liquidFrames[9];
                    }
                    else
                    {
                        spriteRenderer.sprite = liquidFrames[0];
                    }
                }
                else if (left == 0)
                {

                }
                else if (right == 0)
                {

                }
                else
                {
                    spriteRenderer.sprite = liquidFrames[0];
                }
            }
        }
    }
}
