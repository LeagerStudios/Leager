using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StackBarController : MonoBehaviour {

    public int idx = 1;
    public int currentItem = 0;
    public bool InventoryDeployed = false;
    public PackupMenuController planetaryLoading = null;
    public GameManager gameManager;
    public GameObject InventoryBar;
    public Text itemText;
    [SerializeField] public int[] StackBarGrid = new int[9];
    [SerializeField] public int[] StackItemAmount = new int[9];
    RectTransform rectTransform;
    RectTransform parentRectTransform;

    void Start () {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
        gameManager = GameManager.gameManagerReference;
        StackBarGrid = new int[9];
        StackItemAmount = new int[9];

        StackBar.LoadStackBar();

        gameManager.ImportResources();
    }
	
	void Update () {
        if (gameManager.InGame && gameManager.player.alive)
        {
            if (GInput.GetKeyDown(KeyCode.LeftArrow) || Input.mouseScrollDelta.y > 0 && !InventoryDeployed)
            {
                StackBar.ChangeStackBarIdx(idx - 1);
            }
            if (GInput.GetKeyDown(KeyCode.RightArrow) || Input.mouseScrollDelta.y < 0 && !InventoryDeployed)
            {
                StackBar.ChangeStackBarIdx(idx + 1);
            }

            if(int.TryParse(Input.inputString, out int key))
            {
                if (key != 0)
                {
                    StackBar.ChangeStackBarIdx(key - 1);
                }
            }

            if (GInput.GetKeyDown(KeyCode.E) && planetaryLoading == null)
            {
                if (InventoryDeployed)
                {
                    InventoryDeployed = false;
                    planetaryLoading = null;
                }
                else
                {
                    InventoryDeployed = true;
                }

                UpdateStacks();
            }
            transform.GetChild(0).GetComponent<Image>().enabled = !InventoryDeployed;
        }
        else if (gameManager.InGame)
        {
            InventoryDeployed = false;
        }


        if (gameManager.player.alive)
        {
            parentRectTransform.anchoredPosition = new Vector2(190, -135);
        }
        else
        {
            parentRectTransform.anchoredPosition = new Vector2(190, 999);
        }

        if (InventoryDeployed) InventoryBar.transform.GetChild(0).gameObject.SetActive(true);
        else InventoryBar.transform.GetChild(0).gameObject.SetActive(false);
        itemText.enabled = !InventoryDeployed;
        if (!InventoryDeployed)
        {
            itemText.text = gameManager.tileName[StackBarGrid[idx]];
        }

        //UpdateStacks();
    }

    public void UpdateStacks()
    {
        

        for(int i = 1;i < transform.childCount; i++)
        {
            GameObject selectedStack = rectTransform.GetChild(i).gameObject;
            RectTransform childTransform = selectedStack.GetComponent<RectTransform>();

            selectedStack.GetComponent<Image>().sprite = gameManager.tiles[StackBarGrid[i - 1]];

            if (gameManager.stackLimit[StackBarGrid[i - 1]] > 1)
            {
                childTransform.GetChild(0).gameObject.SetActive(true);
                childTransform.GetChild(0).GetComponent<Text>().text = StackItemAmount[i - 1] + "";
            }
            else childTransform.GetChild(0).gameObject.SetActive(false);


            if (idx + 1 == i && !InventoryDeployed)
            {
                { gameManager.canBuild = false; gameManager.canUseTool = false; gameManager.canAtack = false; gameManager.canEquip = false; gameManager.canConsume = false; }

                currentItem = StackBarGrid[i - 1];
                childTransform.localScale = new Vector3(1.3f, 1.3f, 1);
                rectTransform.GetChild(0).gameObject.GetComponent<RectTransform>().position = childTransform.position;
                if (gameManager.tileMainProperty[currentItem] == "placeable")
                {
                    gameManager.canBuild = true;
                    gameManager.chosenBrush = currentItem;
                }
                else if(gameManager.tileType[currentItem] == "tool")
                {
                    gameManager.canUseTool = true;
                    gameManager.toolUsing = gameManager.tileMainProperty[currentItem];
                    gameManager.toolUsingEfficency = gameManager.ToolEfficency[currentItem];
                }
                else if (gameManager.tileType[currentItem] == "arm")
                {
                    gameManager.canAtack = true;
                    gameManager.armUsing = gameManager.tileMainProperty[currentItem];
                    gameManager.armUsingDamageDeal = gameManager.ToolEfficency[currentItem];
                }
                else if(gameManager.tileType[currentItem] == "equip")
                {
                    gameManager.canEquip = true;
                    gameManager.equipType = gameManager.tileMainProperty[currentItem];
                }
                else if (gameManager.tileType[currentItem] == "item" && gameManager.tileMainProperty[currentItem] == "usable")
                {
                    gameManager.canConsume = true;
                }
            }
            else if(idx + 1 == i && InventoryDeployed)
            {
                { gameManager.canBuild = false; gameManager.canUseTool = false; gameManager.canAtack = false; gameManager.canEquip = false; }
                childTransform.localScale = new Vector3(1f, 1f, 1);
            }
            else
            {
                childTransform.localScale = new Vector3(1f, 1f, 1);
            }

        }
    }

}

public static class StackBar
{
    public static StackBarController stackBarController = GameObject.Find("StackBar").GetComponent<StackBarController>();

    public static void Reset()
    {
        stackBarController = null;
        stackBarController = GameObject.Find("StackBar").GetComponent<StackBarController>();
    }

    public static void LoadStackBar()
    {
        if (!GameManager.gameManagerReference.isNetworkClient)
        {
            int[] sg = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/sbg.lgrsd").SavedData);
            int[] sa = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/sba.lgrsd").SavedData); ;
            stackBarController.StackBarGrid = sg;
            stackBarController.StackItemAmount = sa;
        }
        else
        {
            stackBarController.StackBarGrid = new int[] { 22, 23, 24, 0, 0, 0, 0, 0, 16 };
            stackBarController.StackItemAmount = new int[] { 1, 1, 1, 0, 0, 0, 0, 0, 1 };
        }

        stackBarController.UpdateStacks();
    }

    public static void SaveStackBar()
    {
        if (!GameManager.gameManagerReference.isNetworkClient)
        {
            int[] sg = stackBarController.StackBarGrid;
            int[] sa = stackBarController.StackItemAmount;

            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(sg), GameManager.gameManagerReference.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldName + @"/sbg.lgrsd");
            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(sa), GameManager.gameManagerReference.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldName + @"/sba.lgrsd");
        }
    }

    public static void ChangeStackBarIdx(int i)
    {
        stackBarController.idx = Mathf.Clamp(i, 0, 8);
        stackBarController.UpdateStacks();
    }

    public static void AsignNewStack(int index, int tile, int amountOfTiles)
    {
        stackBarController.StackBarGrid[index] = tile;
        stackBarController.StackItemAmount[index] = amountOfTiles;
        stackBarController.UpdateStacks();
    }

    public static void LoseItem()
    {
        if(stackBarController.StackItemAmount[stackBarController.idx] - 1 >= 1)
        {
            stackBarController.StackItemAmount[stackBarController.idx] -= 1;
            stackBarController.UpdateStacks();
        }
        else
        {
            AsignNewStack(stackBarController.idx, 0, 1);
        }
        
    }

    public static void LoseItem(int idx)
    {
        if (stackBarController.StackItemAmount[idx] - 1 >= 1)
        {
            stackBarController.StackItemAmount[idx] -= 1;
            stackBarController.UpdateStacks();
        }
        else
        {
            AsignNewStack(idx, 0, 1);
        }

    }

    public static bool AddItem(int item)
    {
        bool typeAlreadyExists = false;
        int existingIdx = -1;
        {
            for(int i = 0; i < 9; i++)
            {
                if (stackBarController.StackBarGrid[i] == item && stackBarController.StackItemAmount[i] < GameManager.gameManagerReference.stackLimit[stackBarController.StackBarGrid[i]])
                {
                    typeAlreadyExists = true;
                    existingIdx = i;
                }

            }
        }

        if (typeAlreadyExists)
        {
            stackBarController.StackItemAmount[existingIdx] += 1;
            stackBarController.UpdateStacks();
            return true;
        }
        else
        {
            bool nullStack = false;
            int nullStackIdx = 0;
            for(int i = 0; i < 9; i++)
            {
                if (stackBarController.StackBarGrid[i] == 0)
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
        for (int i = 0; i < Amount; i++)
        {
            if (stackBarController.StackItemAmount[idx] > 0) LoseItem(idx);
        }
    }

    public static int Search(int tile, int minAmount)
    {
        int idx = -1;

        for (int i = 0; i < stackBarController.StackBarGrid.Length; i++)
        {
            if (stackBarController.StackBarGrid[i] == tile && stackBarController.StackItemAmount[i] >= minAmount)
            {
                idx = i;
                break;
            }
        }

        return idx;
    }
}
