using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalseChunkController : MonoBehaviour
{

    public ChunkController rootChunk;

    void Start()
    {
        if (rootChunk.transform.position.x < GameManager.gameManagerReference.WorldWidth * 16 / 2)
        {
            transform.position = rootChunk.transform.position + new Vector3(GameManager.gameManagerReference.WorldWidth * 16, 0);
        }
        else
        {
            transform.position = rootChunk.transform.position - new Vector3(GameManager.gameManagerReference.WorldWidth * 16, 0);
        }

        for (int e = 0; e < rootChunk.TileGrid.Length; e++)
        {
            GameObject emptyTile = Instantiate(GameManager.gameManagerReference.emptyTile, transform);
            emptyTile.GetComponent<SpriteRenderer>().sprite = GameManager.gameManagerReference.tiles[rootChunk.TileGrid[e]];
            DivisionResult divisionResult = ManagingFunctions.EntireDivision(e, GameManager.gameManagerReference.WorldHeight);
            emptyTile.transform.localPosition = new Vector2(divisionResult.cocient, divisionResult.rest);
            emptyTile.GetComponent<BoxCollider2D>().enabled = true;
            string cT = GameManager.gameManagerReference.TileCollisionType[rootChunk.TileGrid[e]];

            switch (cT)
            {
                case "#":
                    emptyTile.layer = 8;
                    break;

                case "":
                    emptyTile.layer = 9;
                    break;

                case "~":
                    emptyTile.layer = 10;
                    break;

                case "=":
                    emptyTile.layer = 8;
                    break;
            }
        }
    }

    void Update()
    {
        if (rootChunk.gameObject.activeInHierarchy)
            for (int e = 0; e < rootChunk.TileGrid.Length; e++)
            {
                GameObject tile = transform.GetChild(e).gameObject;
                tile.GetComponent<SpriteRenderer>().sprite = GameManager.gameManagerReference.tiles[rootChunk.TileGrid[e]];
                tile.transform.eulerAngles = rootChunk.TileObject[e].transform.eulerAngles;
                string cT = GameManager.gameManagerReference.TileCollisionType[rootChunk.TileGrid[e]];

                switch (cT)
                {
                    case "#":
                        tile.layer = 8;
                        break;

                    case "":
                        tile.layer = 9;
                        break;

                    case "~":
                        tile.layer = 10;
                        break;

                    case "=":
                        tile.layer = 8;
                        break;
                }
            }
    }
}
