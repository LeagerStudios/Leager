using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Box : MonoBehaviour
{
    public int maxStacks = 9;
    public BoxMenu targetMenu;
    public bool firstFrame = false;

    void Start()
    {
        if (transform.parent.GetComponent<TileProperties>() == null)
            transform.parent.gameObject.AddComponent<TileProperties>().parentTile = System.Array.IndexOf(GameManager.gameManagerReference.tiles, transform.parent.GetComponent<SpriteRenderer>().sprite);
    }

    private void Update()
    {
        if (ManagingFunctions.InsideRanges(GameManager.gameManagerReference.mouseCurrentPosition, transform.position - (Vector3.one * 0.5f), transform.position + (Vector3.one * 0.5f)))
        {
            if (firstFrame && !GInput.GetMouseButton(0))
                firstFrame = false;

            if (GameManager.gameManagerReference.InGame && !StackBar.stackBarController.InventoryDeployed)
                if (targetMenu == null)
                {
                    targetMenu = Instantiate(MenuController.menuController.boxMenu, MenuController.menuController.uiMenus).GetComponent<BoxMenu>();
                    targetMenu.targetBox = this;
                    targetMenu.gameObject.name = "BoxMenu";
                    targetMenu.UpdatePos();
                    firstFrame = true;
                }
        }
        else
        {
            firstFrame = true;

            if (targetMenu != null)
            {
                targetMenu.targetBox = null;
                targetMenu = null;
            }
        }
    }

    public int[][] FetchItems()
    {
        List<string> items = transform.parent.GetComponent<TileProperties>().storedItems;
        List<int> retItems = new List<int>();
        List<int> retAmounts = new List<int>();

        for (int i = 0; i < items.Count; i++)
        {
            int[] data = ManagingFunctions.ConvertStringToIntArray(items[i].Split(':'));

            retItems.Add(data[0]);
            retAmounts.Add(data[1]);
        }

        return new int[][] { retItems.ToArray(), retAmounts.ToArray() };
    }

    public void ToggleItems()
    {
        List<string> items = transform.parent.GetComponent<TileProperties>().storedItems;
        bool condition = false;
        condition = StackBar.stackBarController.currentItem == 0 && items.Count > 0;

        if (!condition)
            if (items.Count > 0)
            {
                int[] data = ManagingFunctions.ConvertStringToIntArray(items[items.Count - 1].Split(':'));

                int tile = data[0];
                int tileAmount = data[1];


                condition = (tile == StackBar.stackBarController.currentItem) 
                    && (StackBar.stackBarController.StackItemAmount[StackBar.stackBarController.idx] < GameManager.gameManagerReference.stackLimit[StackBar.stackBarController.currentItem])
                    && (tileAmount < GameManager.gameManagerReference.stackLimit[tile]);
            }
                

        if (condition)
        {
            int[] data = ManagingFunctions.ConvertStringToIntArray(items[items.Count - 1].Split(':'));

            int tile = data[0];
            int tileAmount = data[1];


            while (tileAmount > 0)
                if (StackBar.AddItem(tile, StackBar.stackBarController.idx))
                    tileAmount--;
                else break;

            if (tileAmount > 0)
                GameManager.gameManagerReference.player.PlayerRelativeDrop(tile, tileAmount);

            items.RemoveAt(items.Count - 1);
            if (targetMenu != null) targetMenu.RemoveLast();
        }
        else
        {
            if(StackBar.stackBarController.currentItem != 0 && items.Count < maxStacks)
            {
                items.Add(StackBar.stackBarController.currentItem + ":" + StackBar.stackBarController.StackItemAmount[StackBar.stackBarController.idx]);
                if (targetMenu != null) targetMenu.Add(StackBar.stackBarController.currentItem, StackBar.stackBarController.StackItemAmount[StackBar.stackBarController.idx], false);
                StackBar.AsignNewStack(StackBar.stackBarController.idx, 0, 0);
            }
        }

        transform.parent.GetComponent<TileProperties>().CommitToChunk();
    }

    public bool AddItem(int item, int amount)
    {
        List<string> items = transform.parent.GetComponent<TileProperties>().storedItems;

        if (items.Count < maxStacks)
        {
            items.Add(item + ":" + amount);
            if (targetMenu != null) targetMenu.Add(item, amount, false);
            return true;
        }
        else return false;
    }

    public string GetItem()
    {
        List<string> items = transform.parent.GetComponent<TileProperties>().storedItems;

        if (items.Count > 0)
        {
            int[] data = ManagingFunctions.ConvertStringToIntArray(items[items.Count - 1].Split(':'));

            int tile = data[0];
            int tileAmount = data[1];

            items.RemoveAt(items.Count - 1);
            if (targetMenu != null) targetMenu.RemoveLast();

            return tile + ":" + tileAmount;
        }
        else return "null";
    }
}
