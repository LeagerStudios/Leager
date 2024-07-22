using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmorBarController : MonoBehaviour
{
    public InventoryArrowController inventoryArrow;
    public StackStorage stackStorage;
    public Image[] stacks;
    public Sprite[] previews;
    public RectTransform rectTransform;

    public void Initialize(int[] equips)
    {
        try
        {
            for(int i = 0; i < stackStorage.items.Length; i++)
            {
                stackStorage.SetAt(i, equips[i], equips[i] == 0 ? 0 : 1);
            } 
        }
        finally
        {
            GameManager.gameManagerReference.equipedArmor = stackStorage.items;
        }
    }

    public void Click(int idx)
    {
        string type = "equip";
        switch (idx)
        {
            case 0:
                type = "helmet";
                break;
            case 1:
                type = "chestplate";
                break;
            case 2:
                type = "boots";
                break;
            case 3:
                type = "helmetAcc";
                break;
            case 4:
                type = "chestplateAcc";
                break;
            case 5:
                type = "bootsAcc";
                break;
        }

        inventoryArrow.ArmorPickup(idx, stackStorage, type);
    }

    void Update()
    {
        if (StackBar.stackBarController.InventoryDeployed && GameManager.gameManagerReference.player.alive)
        {
            rectTransform.anchoredPosition = new Vector2(0, 0);
            for (int i = 0; i < stacks.Length; i++)
            {
                int tile = stackStorage.items[i];
                if (tile == 0)
                    stacks[i].sprite = previews[i];
                else
                    stacks[i].sprite = GameManager.gameManagerReference.tiles[tile];

                Color color = stacks[i].color;
                color.a = tile == 0 ? 0.5f : 1f;
                stacks[i].color = color;
            }
        }
        else
        {
            rectTransform.anchoredPosition = new Vector2(0, 3000);
        }
    }
}
