using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryArrowController : MonoBehaviour {

    GameManager gameManager;

    public int idxPos = 0;
    public int tilePorting = -1;
    public int tilePortingAmount;

	void Start () {
        gameManager = GameManager.gameManagerReference;
	}
	
	void Update () {
        idxPos = Mathf.Clamp(idxPos, 0, 44);

        Vector2 dir = Vector2.zero;
        if(gameManager.InGame && gameManager.player.alive)
        {
            if (GInput.GetKeyDown(KeyCode.RightArrow)) dir = Vector2.right;
            else if (GInput.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2.left;
            else if (GInput.GetKeyDown(KeyCode.UpArrow)) dir = Vector2.up;
            else if (GInput.GetKeyDown(KeyCode.DownArrow)) dir = Vector2.down;
            
            if(StackBar.stackBarController.InventoryDeployed && GInput.GetKeyDown(KeyCode.Q))
            {
                if(tilePorting != -1)
                {
                    gameManager.player.PlayerRelativeDrop(tilePorting, tilePortingAmount);

                    tilePorting = -1;
                    GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().enabled = false;
                    GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().color = Color.white;
                    GetComponent<RectTransform>().GetChild(1).GetComponent<Image>().sprite = gameManager.tiles[0];
                }
            }
        }

        UpdatePosition(dir);

        if (GInput.GetKeyDown(KeyCode.Return) && StackBar.stackBarController.InventoryDeployed && gameManager.InGame && gameManager.player.alive)
        {
            if (StackBar.stackBarController.planetaryLoading == null)
            {
                DragSelectedItem();
            }
            else
            {
                if (idxPos < 9)
                {
                    tilePorting = StackBar.stackBarController.StackBarGrid[idxPos];
                    tilePortingAmount = StackBar.stackBarController.StackItemAmount[idxPos];
                }
                else
                {
                    tilePorting = InventoryBar.inventoryBarController.InventoryBarGrid[idxPos - 9];
                    tilePortingAmount = InventoryBar.inventoryBarController.InventoryItemAmount[idxPos - 9];
                }

                string returnn = StackBar.stackBarController.planetaryLoading.AddToPackup(tilePorting, tilePortingAmount);
                string[] decomposed = returnn.Split(':');
                int finalItem = System.Convert.ToInt32(decomposed[0]);
                int finalAmount = System.Convert.ToInt32(decomposed[1]);

                if (finalAmount == 0)
                    finalItem = 0;

                if (idxPos < 9)
                {
                    StackBar.AsignNewStack(idxPos, finalItem, finalAmount);
                }
                else
                {
                    InventoryBar.AsignNewStack(idxPos - 9, finalItem, finalAmount);
                }
                tilePorting = -1;
                tilePortingAmount = 0;
            }
        }
	}

    public void UpdatePosition(Vector2 dir)
    {
        if (StackBar.stackBarController.InventoryDeployed)
        {
            if (dir == Vector2.up  && idxPos > 8)
            {
                idxPos = Mathf.Clamp(idxPos - 9, 0, 44);
            }
            else if (dir == Vector2.down && idxPos < 36)
            {
                idxPos = Mathf.Clamp(idxPos + 9, 0, 44);
            }
            else if (dir == Vector2.left && Mathf.Repeat(idxPos, 9) != 0)
            {
                idxPos = Mathf.Clamp(idxPos - 1, 0, 44);
            }
            else if (dir == Vector2.right && Mathf.Repeat(idxPos, 9) != 8)
            {
                idxPos = Mathf.Clamp(idxPos + 1, 0, 44);
            }

            DivisionResult nextPos = ManagingFunctions.EntireDivision(idxPos, 9);

            GetComponent<RectTransform>().localPosition = new Vector2(-370, 170);
            if(nextPos.cocient == 0)
            {
                GetComponent<RectTransform>().position = StackBar.stackBarController.GetComponent<RectTransform>().GetChild((int)nextPos.rest + 1).position;
            }
            else
            {
                GetComponent<RectTransform>().position = GetComponent<RectTransform>().parent.GetChild(1).GetChild(0).GetChild((int)nextPos.cocient - 1).GetChild((int)nextPos.rest).position;
            }
            GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().enabled = true;
        }
        else if (tilePorting != -1)
        {
            gameManager.player.PlayerRelativeDrop(tilePorting, tilePortingAmount);
            
            tilePorting = -1;
            GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().enabled = false;
            GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().color = Color.white;
            GetComponent<RectTransform>().GetChild(1).GetComponent<Image>().sprite = gameManager.tiles[0];
        }
        else GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().enabled = false;

    }

    public void DragSelectedItem()
    {
        if(tilePorting == -1)
        {
            bool dragged = false;

            if(idxPos < 9 && StackBar.stackBarController.StackBarGrid[idxPos] != 0)
            {
                tilePorting = StackBar.stackBarController.StackBarGrid[idxPos];
                tilePortingAmount = StackBar.stackBarController.StackItemAmount[idxPos];
                StackBar.AsignNewStack(idxPos, 0, 1);
                dragged = true;
            }
            else if (idxPos > 8 && InventoryBar.inventoryBarController.InventoryBarGrid[idxPos - 9] != 0)
            {
                tilePorting = InventoryBar.inventoryBarController.InventoryBarGrid[idxPos - 9];
                tilePortingAmount = InventoryBar.inventoryBarController.InventoryItemAmount[idxPos - 9];
                InventoryBar.AsignNewStack(idxPos - 9, 0, 1);
                dragged = true;
            }

            if (dragged)
            {
                transform.GetChild(0).GetComponent<Image>().color = Color.yellow;
                transform.GetChild(1).GetComponent<Image>().enabled = true;
                transform.GetChild(1).GetComponent<Image>().sprite = gameManager.tiles[tilePorting];
            }
            
        }
        else
        {
            bool dragged = false;

            if (idxPos < 9)
            {
                if(StackBar.stackBarController.StackBarGrid[idxPos] == 0)
                {
                    StackBar.AsignNewStack(idxPos, tilePorting, tilePortingAmount);
                    tilePorting = -1;
                    dragged = true;
                }
                else if(StackBar.stackBarController.StackBarGrid[idxPos] == tilePorting && StackBar.stackBarController.StackItemAmount[idxPos] < 99 && GameManager.gameManagerReference.tileType[tilePorting] != "tool" && StackBar.stackBarController.StackItemAmount[idxPos] < 99 && GameManager.gameManagerReference.tileType[tilePorting] != "arm" && GameManager.gameManagerReference.tileType[tilePorting] != "equip" && GameManager.gameManagerReference.tileType[tilePorting] != "core")
                {
                    StackBar.AsignNewStack(idxPos, tilePorting, tilePortingAmount + StackBar.stackBarController.StackItemAmount[idxPos]);

                    if(StackBar.stackBarController.StackItemAmount[idxPos] > 99)
                    {
                        tilePortingAmount = StackBar.stackBarController.StackItemAmount[idxPos] - 99;
                        StackBar.AsignNewStack(idxPos, tilePorting, 99);
                        tilePorting = StackBar.stackBarController.StackBarGrid[idxPos];

                    }
                    else
                    {
                        tilePorting = -1;
                        dragged = true;
                    }

                }
                else
                {
                    int actualTilePorting = tilePorting;
                    int actualTilePortingAmmount = tilePortingAmount;
                    int actualStackItem = StackBar.stackBarController.StackBarGrid[idxPos];
                    int actualStackItemAmount = StackBar.stackBarController.StackItemAmount[idxPos];

                    StackBar.AsignNewStack(idxPos, actualTilePorting, actualTilePortingAmmount);

                    tilePorting = actualStackItem;
                    tilePortingAmount = actualStackItemAmount;
                    transform.GetChild(1).GetComponent<Image>().sprite = gameManager.tiles[tilePorting];

                }
                
                
            }
            else
            {
                if (InventoryBar.inventoryBarController.InventoryBarGrid[idxPos - 9] == 0)
                {
                    InventoryBar.AsignNewStack(idxPos - 9, tilePorting, tilePortingAmount);
                    tilePorting = -1;
                    dragged = true;
                }
                else if (InventoryBar.inventoryBarController.InventoryBarGrid[idxPos - 9] == tilePorting && InventoryBar.inventoryBarController.InventoryItemAmount[idxPos - 9] < 99 && GameManager.gameManagerReference.tileType[tilePorting] != "tool" && GameManager.gameManagerReference.tileType[tilePorting] != "arm" && GameManager.gameManagerReference.tileType[tilePorting] != "equip" && GameManager.gameManagerReference.tileType[tilePorting] != "core")
                {
                    InventoryBar.AsignNewStack(idxPos - 9, tilePorting, tilePortingAmount + InventoryBar.inventoryBarController.InventoryItemAmount[idxPos - 9]);

                    if (InventoryBar.inventoryBarController.InventoryItemAmount[idxPos - 9] > 99)
                    {
                        tilePortingAmount = InventoryBar.inventoryBarController.InventoryItemAmount[idxPos - 9] - 99;
                        InventoryBar.AsignNewStack(idxPos - 9, tilePorting, 99);
                        tilePorting = InventoryBar.inventoryBarController.InventoryBarGrid[idxPos - 9];

                    }
                    else
                    {
                        tilePorting = -1;
                        dragged = true;
                    }

                }
                else
                {
                    int actualTilePorting = tilePorting;
                    int actualTilePortingAmmount = tilePortingAmount;
                    int actualStackItem = InventoryBar.inventoryBarController.InventoryBarGrid[idxPos - 9];
                    int actualStackItemAmount = InventoryBar.inventoryBarController.InventoryItemAmount[idxPos - 9];

                    InventoryBar.AsignNewStack(idxPos - 9, actualTilePorting, actualTilePortingAmmount);

                    tilePorting = actualStackItem;
                    tilePortingAmount = actualStackItemAmount;
                    transform.GetChild(1).GetComponent<Image>().sprite = gameManager.tiles[tilePorting];

                }
            }

            if (dragged)
            {
                transform.GetChild(0).GetComponent<Image>().color = Color.white;
                transform.GetChild(1).GetComponent<Image>().enabled = false;
                transform.GetChild(1).GetComponent<Image>().sprite = gameManager.tiles[0];
            }
        }
    }
}
