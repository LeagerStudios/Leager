using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryArrowController : MonoBehaviour
{

    GameManager gameManager;
    RectTransform rectTransform;
    Vector2 offsetConstant;
    Vector2 centerConstant;

    public int idxPos = 0;
    public int tilePorting = -1;
    public int tilePortingAmount;

    void Start()
    {
        gameManager = GameManager.gameManagerReference;
        rectTransform = GetComponent<RectTransform>();
        offsetConstant = new Vector2(-160, 80);
        centerConstant = new Vector2(20, 20);
    }

    void Update()
    {
        UpdatePosition();

        if (GInput.GetMouseButtonDown(0) && StackBar.stackBarController.InventoryDeployed && gameManager.InGame && gameManager.player.alive)
        {
            if (StackBar.stackBarController.planetaryLoading == null)
            {
                DragSelectedItem();
            }
            else
            {
                if (idxPos != -1)
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
    }

    public void UpdatePosition()
    {
        if (StackBar.stackBarController.InventoryDeployed)
        {
            GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().enabled = true;

            if (!gameManager.cancelPlacing)
            {
                // Get the position of the mouse cursor in screen space
                Vector3 mousePosition = Input.mousePosition;

                // Convert the screen space position to canvas space
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, mousePosition, null, out Vector2 localPoint);

                Vector2 originalPosition = rectTransform.localPosition;
                // Set the position of the UI element to the converted canvas space position
                rectTransform.localPosition = localPoint;
                Vector2 rawPosition = rectTransform.anchoredPosition - offsetConstant + centerConstant;
                rawPosition = Vector2Int.FloorToInt(rawPosition / 40) * 40;
                rectTransform.localPosition = originalPosition;

                Vector2Int indexPosition = Vector2Int.FloorToInt(rawPosition / 40);
                bool insideLimits = indexPosition.x >= 0 && indexPosition.x < 9 && indexPosition.y <= 0 && indexPosition.y > -5;

                int index = -1;
                if (insideLimits) index = indexPosition.y * -9 + indexPosition.x;

                idxPos = index;

                if (insideLimits)
                    rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, rawPosition + offsetConstant, Time.deltaTime * 30);
                else
                {
                    rectTransform.anchoredPosition = localPoint;
                    GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().enabled = tilePorting != -1;
                }
            }
        }
        else if (tilePorting != -1)
        {
            gameManager.player.PlayerRelativeDrop(tilePorting, tilePortingAmount);

            tilePorting = -1;
            rectTransform.GetChild(0).GetComponent<Image>().enabled = false;
            rectTransform.GetChild(0).GetComponent<Image>().color = Color.white;
            rectTransform.GetChild(1).GetComponent<Image>().sprite = gameManager.tiles[0];
        }
        else rectTransform.GetChild(0).GetComponent<Image>().enabled = false;

    }

    public void DragSelectedItem()
    {
        if (idxPos != -1)
        {
            if (tilePorting == -1)
            {
                bool dragged = false;

                if (idxPos < 9 && StackBar.stackBarController.StackBarGrid[idxPos] != 0)
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
                    if (StackBar.stackBarController.StackBarGrid[idxPos] == 0)
                    {
                        StackBar.AsignNewStack(idxPos, tilePorting, tilePortingAmount);
                        tilePorting = -1;
                        dragged = true;
                    }
                    else if (StackBar.stackBarController.StackBarGrid[idxPos] == tilePorting && StackBar.stackBarController.StackItemAmount[idxPos] < 99 && GameManager.gameManagerReference.tileType[tilePorting] != "tool" && StackBar.stackBarController.StackItemAmount[idxPos] < 99 && GameManager.gameManagerReference.tileType[tilePorting] != "arm" && GameManager.gameManagerReference.tileType[tilePorting] != "equip" && GameManager.gameManagerReference.tileType[tilePorting] != "core")
                    {
                        StackBar.AsignNewStack(idxPos, tilePorting, tilePortingAmount + StackBar.stackBarController.StackItemAmount[idxPos]);

                        if (StackBar.stackBarController.StackItemAmount[idxPos] > 99)
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
        else if (tilePorting != -1)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (gameManager.tileType[tilePorting] == "equip" && Vector2.Distance(mouseWorldPos, gameManager.player.transform.position) < 0.76f)
            {
                tilePorting = gameManager.EquipItem(tilePorting, gameManager.tileMainProperty[tilePorting]);
                rectTransform.GetChild(1).GetComponent<Image>().sprite = gameManager.tiles[tilePorting];

                if(tilePorting == 0)
                {
                    rectTransform.GetChild(0).GetComponent<Image>().enabled = false;
                    rectTransform.GetChild(0).GetComponent<Image>().color = Color.white;
                    tilePorting = -1;
                }
            }
            else
            {
                gameManager.player.GetComponent<SpriteRenderer>().flipX = mouseWorldPos.x < gameManager.player.transform.position.x;
                gameManager.player.PlayerRelativeDrop(tilePorting, tilePortingAmount);
                tilePorting = -1;
                rectTransform.GetChild(1).GetComponent<Image>().sprite = gameManager.tiles[0];
                rectTransform.GetChild(0).GetComponent<Image>().enabled = false;
                rectTransform.GetChild(0).GetComponent<Image>().color = Color.white;
            }
        }
    }
}
