using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    int maxStacks = 9;


    void Start()
    {
        if (transform.parent.GetComponent<TileProperties>() == null)
            transform.parent.gameObject.AddComponent<TileProperties>().parentTile = 15;
    }

    public void ToggleItems()
    {
        List<string> items = transform.parent.GetComponent<TileProperties>().storedItems;
        if(StackBar.stackBarController.currentItem == 0 && items.Count > 0)
        {
            int[] data = ManagingFunctions.ConvertStringToIntArray(items[items.Count - 1].Split(':'));

            int tile = data[0];
            int tileAmount = data[1];


            while (tileAmount > 0)
                if (StackBar.AddItem(tile))
                    tileAmount--;
                else break;

            if (tileAmount > 0)
                GameManager.gameManagerReference.player.PlayerRelativeDrop(tile, tileAmount);

            items.RemoveAt(items.Count - 1);
        }
        else
        {
            if(StackBar.stackBarController.currentItem != 0 && items.Count <= maxStacks)
            {
                items.Add(StackBar.stackBarController.currentItem + ":" + StackBar.stackBarController.StackItemAmount[StackBar.stackBarController.idx]);
                StackBar.AsignNewStack(StackBar.stackBarController.idx, 0, 0);
            }
        }

        transform.parent.GetComponent<TileProperties>().CommitToChunk();
    }

    public bool AddItem(int item, int amount)
    {
        List<string> items = transform.parent.GetComponent<TileProperties>().storedItems;

        if (items.Count <= maxStacks)
        {
            items.Add(item + ":" + amount);
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

            return tile + ":" + tileAmount;
        }
        else return "null";
    }
}
