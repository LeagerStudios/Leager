using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftMenuController : MonoBehaviour
{
    [SerializeField] GameObject arrowButtons;
    RectTransform rectTransform;
    RectTransform canvasRect;
    RectTransform viewport;
    int tileSelected = 0;
    int tileSelectedAmount = 0;
    Transform follower;
    GameObject UiMenu;
    Dropdown dropdown;
    bool canCraft = false;

    List<int> currentTiles = new List<int>();
    List<int> currentTileAmounts = new List<int>();
    List<int> avaliableTiles = new List<int>();
    List<string> availableRecipes = new List<string>();

    public void InvokeMenu(Transform followTo)
    {
        rectTransform = GetComponent<RectTransform>();
        viewport = (RectTransform)rectTransform.GetChild(1).GetChild(3).GetChild(1);
        canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
        follower = followTo;
        List<Sprite> options = new List<Sprite>();
        string[] text = DataSaver.ReadTxt(Application.persistentDataPath + @"/packages/LeagerStudios/crafts.txt");
        int LIT = DataSaver.LinesInTxt(Application.persistentDataPath + @"/packages/LeagerStudios/crafts.txt");

        for (int i = 0; i < LIT; i++)
        {

            List<char> tileList = new List<char>();

            for (int i2 = 0; text[i][i2] != '{'; i2++)
            {
                tileList.Add(text[i][i2]);
            }
            int tile = System.Convert.ToInt32(new string(tileList.ToArray()));

            //if (TechManager.techTree.unlockedItems.Contains(tile))
            //{
            availableRecipes.Add(text[i]);
            avaliableTiles.Add(tile);
            options.Add(GameManager.gameManagerReference.tiles[System.Convert.ToInt32(new string(tileList.ToArray()))]);
            //}
        }

        UiMenu = transform.parent.gameObject;
        transform.GetChild(1).GetChild(0).GetComponent<Dropdown>().ClearOptions();
        transform.GetChild(1).GetChild(0).GetComponent<Dropdown>().AddOptions(options);
        transform.GetChild(1).GetChild(0).GetComponent<Dropdown>().RefreshShownValue();

        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(follower.transform.position);
        Vector2 FollowerScreenPosition = new Vector2((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                                                     (ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));

        rectTransform.anchoredPosition = FollowerScreenPosition;
        dropdown = transform.GetChild(1).GetChild(0).GetComponent<Dropdown>();

        BlockSelected(dropdown.value);
    }


    void Update()
    {
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(follower.transform.position);
        Vector2 FollowerScreenPosition = new Vector2((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                                                     (ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));

        rectTransform.anchoredPosition = FollowerScreenPosition;
        Debug.DrawRay(follower.position, Vector3.up, Color.black);

        if (follower.GetComponent<SpriteRenderer>().sprite != GameManager.gameManagerReference.tiles[16])
        {
            Close();
        }


        {   //Check can craft
            canCraft = true;
            for (int i = 0; i < currentTiles.Count; i++)
            {
                if (StackBar.Search(currentTiles[i], currentTileAmounts[i]) == -1)
                {
                    if (InventoryBar.Search(currentTiles[i], currentTileAmounts[i]) == -1)
                    {
                        canCraft = false;
                    }
                }
            }

            transform.GetChild(1).GetChild(2).GetComponent<Button>().interactable = canCraft;
        }

        if (GInput.GetKeyDown(KeyCode.E)) Close();

    }

    public void Close()
    {
        StartCoroutine(IEClose());
    }

    public IEnumerator IEClose()
    {
        dropdown.Hide();
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    public void BlockSelected(int blockIdx)
    {
        string line = availableRecipes[blockIdx];
        {
            bool canRead = false;
            bool onKeys = false;
            List<char> tile = new List<char>();
            List<char> tileAmm = new List<char>();

            for (int e = 0; line[e] != ':'; e++)
            {

                canRead = true;
                if (onKeys && line[e] == '}')
                {
                    onKeys = false;
                    canRead = false;
                }
                if (!onKeys && line[e] == '{')
                {
                    onKeys = true;
                    canRead = false;
                }

                if (canRead && onKeys)
                {
                    tileAmm.Add(line[e]);
                }
                if (canRead && !onKeys)
                {
                    tile.Add(line[e]);
                }
            }

            tileSelectedAmount = System.Convert.ToInt32(new string(tileAmm.ToArray()));
            tileSelected = System.Convert.ToInt32(new string(tile.ToArray()));
        }


        currentTiles = new List<int>();
        currentTileAmounts = new List<int>();

        viewport.localPosition = new Vector2(0, 0);
        RectTransform itemsReq = (RectTransform)rectTransform.GetChild(1).GetChild(3);
        GameObject itemReqPrefab = itemsReq.GetChild(0).gameObject;

        for (int i = 0; i < viewport.childCount; i++)
        {
            Destroy(viewport.GetChild(i).gameObject);
        }


        string textExport = "";
        List<char> textExportChars = new List<char>();

        bool exportFase = false;
        for (int i = 0; i < line.Length; i++)
        {
            if (exportFase == false)
            {
                if (line[i] == ':') exportFase = true;
            }
            else
            {
                textExportChars.Add(line[i]);
            }
        }

        textExport = new string(textExportChars.ToArray());

        List<string> items = new List<string>();
        List<char> item = new List<char>();

        for (int i = 0; i < textExport.Length; i++)
        {
            item.Add(textExport[i]);

            if (textExport[i] == ';')
            {
                items.Add(new string(item.ToArray()));
                item = new List<char>();
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            string text = items[i];
            int tile = 1;
            int tileAmount = 15;
            List<char> prevTile = new List<char>();
            List<char> prevTileAmmount = new List<char>();
            bool onKeys = false;

            for (int e = 0; text[e] != ';'; e++)
            {
                bool canRead = true;
                if (onKeys)
                {
                    if (text[e] == '}')
                    {
                        onKeys = false;
                        canRead = false;
                    }
                }
                else
                {
                    if (text[e] == '{')
                    {
                        onKeys = true;
                        canRead = false;
                    }
                }

                if (canRead)
                {
                    char value = text[e];
                    if (onKeys) prevTileAmmount.Add(value);
                    else prevTile.Add(value);
                }
            }

            tile = System.Convert.ToInt32(new string(prevTile.ToArray()));
            tileAmount = System.Convert.ToInt32(new string(prevTileAmmount.ToArray()));

            currentTiles.Add(tile);
            currentTileAmounts.Add(tileAmount);

            GameObject gameObject = Instantiate(itemReqPrefab, viewport);
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (i * -30) + 18f);
            gameObject.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.gameManagerReference.tiles[tile];
            gameObject.transform.GetChild(1).GetComponent<Text>().text = tileAmount + "";
        }

        arrowButtons.SetActive(items.Count > 2);
    }

    public void MoveBlocksReq(int move)
    {
        RectTransform rectViewport = (RectTransform)rectTransform.GetChild(1).GetChild(3).GetChild(1);
        rectViewport.anchoredPosition = new Vector2(0, Mathf.Clamp(rectViewport.anchoredPosition.y + (move * 15), 0, -rectViewport.GetChild(viewport.childCount - 1).GetComponent<RectTransform>().anchoredPosition.y));
    }

    public void Craft()
    {
        for (int i = 0; i < currentTiles.Count; i++)
        {
            int idx = 0;

            if (StackBar.Search(currentTiles[i], currentTileAmounts[i]) == -1)
            {
                idx = InventoryBar.Search(currentTiles[i], currentTileAmounts[i]);
                InventoryBar.QuitItems(idx, currentTileAmounts[i]);
            }
            else
            {
                idx = StackBar.Search(currentTiles[i], currentTileAmounts[i]);
                StackBar.QuitItems(idx, currentTileAmounts[i]);
            }
        }
        int temp = tileSelectedAmount;

        while (tileSelectedAmount > 0)
            if (StackBar.AddItem(tileSelected))
                tileSelectedAmount--;
            else break;

        if (tileSelectedAmount > 0)
            GameManager.gameManagerReference.player.PlayerRelativeDrop(tileSelected, tileSelectedAmount);

        tileSelectedAmount = temp;

        TechManager.techTree.UnlockBlock(tileSelected, true);
    }
}
