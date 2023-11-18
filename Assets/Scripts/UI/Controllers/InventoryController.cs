using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{

    public GameManager gameManager;
    public GameObject StackBar;
    [SerializeField] public int[] InventoryBarGrid = new int[36];
    [SerializeField] public int[] InventoryItemAmount = new int[36];

    void Start()
    {
        gameManager = GameManager.gameManagerReference;
        InventoryBarGrid = new int[36];
        InventoryItemAmount = new int[36];

        InventoryBar.LoadInventory();
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        UpdateStacks();
    }

    public void UpdateStacks()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        for (int i = 1; i < (rectTransform.GetChild(0).childCount * 9) + 1; i++)
        {
            GameObject selectedStack = gameObject;
            RectTransform childTransform = rectTransform;

            selectedStack = rectTransform.GetChild(0).GetChild((int)ManagingFunctions.EntireDivision(i - 1, 9).cocient).GetChild((int)ManagingFunctions.EntireDivision(i - 1, 9).rest).gameObject;
            childTransform = selectedStack.GetComponent<RectTransform>();
            

            selectedStack.GetComponent<Image>().sprite = gameManager.tiles[InventoryBarGrid[i - 1]];

            if (!(gameManager.tileType[System.Array.IndexOf(gameManager.tiles, childTransform.GetComponent<Image>().sprite)] == "tool" || gameManager.tileType[System.Array.IndexOf(gameManager.tiles, childTransform.GetComponent<Image>().sprite)] == "arm" || gameManager.tileType[System.Array.IndexOf(gameManager.tiles, childTransform.GetComponent<Image>().sprite)] == "equip") && !(gameManager.tileType[System.Array.IndexOf(gameManager.tiles, childTransform.GetComponent<Image>().sprite)] == ""))
            {
                childTransform.GetChild(0).gameObject.SetActive(true);
                childTransform.GetChild(0).GetComponent<Text>().text = InventoryItemAmount[i - 1] + "";
            }
            else childTransform.GetChild(0).gameObject.SetActive(false);
        }


    }
}

public static class InventoryBar
{
    public static InventoryController inventoryBarController = GameObject.Find("InventoryBar").GetComponent<InventoryController>();

    public static void Reset()
    {
        inventoryBarController = null;
        inventoryBarController = GameObject.Find("InventoryBar").GetComponent<InventoryController>();
    }

    public static void LoadInventory()
    {
        if (!GameManager.gameManagerReference.isNetworkClient)
        {
            int[] sg = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/ig.lgrsd").SavedData);
            int[] sa = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/ia.lgrsd").SavedData); ;
            inventoryBarController.InventoryBarGrid = sg;
            inventoryBarController.InventoryItemAmount = sa;
        }
        else
        {
            inventoryBarController.InventoryBarGrid = new int[36];
            inventoryBarController.InventoryItemAmount = new int[36];
        }
    }

    public static void SaveInventory()
    {
        if (!GameManager.gameManagerReference.isNetworkClient)
        {
            int[] sg = inventoryBarController.InventoryBarGrid;
            int[] sa = inventoryBarController.InventoryItemAmount;

            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(sg), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/ig.lgrsd");
            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(sa), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/ia.lgrsd");
        }
    }

    public static void AsignNewStack(int index, int tile, int amountOfTiles)
    {
        inventoryBarController.InventoryBarGrid[index] = tile;
        inventoryBarController.InventoryItemAmount[index] = amountOfTiles;
        inventoryBarController.UpdateStacks();
    }

    public static void LoseItem(int idx)
    {
        if (inventoryBarController.InventoryItemAmount[idx] - 1 >= 1)
        {
            inventoryBarController.InventoryItemAmount[idx] -= 1;
            inventoryBarController.UpdateStacks();
        }
        else
        {
            AsignNewStack(idx, 0, 1);
        }

    }

    public static bool AddItem(int item)
    {
        bool typeAlreadyExists = false;
        {//check if type already exists
            for (int i = 0; i < 36; i++)
            {
                if (inventoryBarController.InventoryBarGrid[i] == item) typeAlreadyExists = true;
            }
        }

        if (typeAlreadyExists && inventoryBarController.InventoryItemAmount[System.Array.IndexOf(inventoryBarController.InventoryBarGrid, item)] < 99)
        {
            inventoryBarController.InventoryItemAmount[System.Array.IndexOf(inventoryBarController.InventoryBarGrid, item)] += 1;
            inventoryBarController.UpdateStacks();
            return true;
        }
        else
        {
            bool nullStack = false;
            int nullStackIdx = 0;
            for (int i = 0; i < 36; i++)
            {
                if (inventoryBarController.InventoryBarGrid[i] == 0)
                {
                    if (!nullStack)
                    {
                        nullStack = true;
                        nullStackIdx = i;
                    }

                }
            }
            if (nullStack)
            {
                AsignNewStack(nullStackIdx, item, 1);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static void QuitItems(int idx, int Amount)
    {
        for(int i = 0; i < Amount; i++)
        {
            if(inventoryBarController.InventoryItemAmount[idx] > 0) LoseItem(idx);
        }
    }

    public static int Search(int tile, int minAmount)
    {
        int idx = -1;
        
        for (int i = 0;i< inventoryBarController.InventoryBarGrid.Length; i++)
        {
            if(inventoryBarController.InventoryBarGrid[i] == tile && inventoryBarController.InventoryItemAmount[i] >= minAmount)
            {
                idx = i;
                break;
            }
        }

        return idx;
    }
}
