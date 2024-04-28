using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCursorController : MonoBehaviour {

    GameManager gameManager;
    [SerializeField] Sprite[] tileBrokeSequence;


	void Start () {
        gameManager = GameManager.gameManagerReference;
    }

    void Update()
    {
        if (gameManager.InGame)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), -1);

            for (int i = 1; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            if (gameManager.building)
            {
                GetComponent<SpriteRenderer>().enabled = true;
                Color newColor = GetComponent<SpriteRenderer>().color;
                newColor.a = 0.2f;
                GetComponent<SpriteRenderer>().color = newColor;
                if (gameManager.tileBuildPrev[gameManager.chosenBrush] == null)
                {
                    GetComponent<SpriteRenderer>().sprite = gameManager.tiles[gameManager.chosenBrush];
                }
                else
                {
                    GetComponent<SpriteRenderer>().sprite = gameManager.tileBuildPrev[gameManager.chosenBrush];
                }
            }
            else if (gameManager.usingTool && gameManager.tileBreaking != -1 && !gameManager.cancelPlacing)
            {
                GetComponent<SpriteRenderer>().enabled = true;
                Color newColor = GetComponent<SpriteRenderer>().color;
                newColor.a = 1;
                GetComponent<SpriteRenderer>().color = newColor;
                if (Mathf.Floor(gameManager.breakingTime / 20) - 1 >= 0)
                {
                    GetComponent<SpriteRenderer>().sprite = tileBrokeSequence[Mathf.FloorToInt(Mathf.Clamp(gameManager.breakingTime / 20, 0, 4))];
                }
                else
                {
                    GetComponent<SpriteRenderer>().sprite = tileBrokeSequence[0];
                }


                if(gameManager.tileBreaking != -1)
                {
                    Vector4 tileProp = gameManager.tileSize[gameManager.tileBreaking];
                    List<Vector2> tiles = new List<Vector2>();

                    for (int i = 0; i < tileProp.x; i++)
                        for (int i2 = 0; i2 < tileProp.y; i2++)
                            if(new Vector2(i - tileProp.z, i2 - tileProp.w) != Vector2.zero)
                            tiles.Add(new Vector2(i - tileProp.z, i2 - tileProp.w));

                    foreach(Vector2 tilePos in tiles)
                    {
                        GameObject breakTile = Instantiate(gameManager.emptySprite, transform);
                        breakTile.transform.localPosition = tilePos;
                        breakTile.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
                    }

                }
                
                gameManager.player.UseToolAnim();
            }
            else
            {
                GetComponent<SpriteRenderer>().enabled = false;
            }

            transform.eulerAngles = new Vector3(0, 0, gameManager.buildRotation);
        }
    }
}
