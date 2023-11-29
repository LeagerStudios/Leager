using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackupMenuController : MonoBehaviour
{
    [SerializeField] GameObject itemPackedPrefab;
    [SerializeField] RectTransform viewport;
    [SerializeField] RectTransform coreReporter;
    [SerializeField] RectTransform blockedViewport;
    [SerializeField] RectTransform eToLaunch;
    [SerializeField] public RectTransform planetPanel;

    List<string> items = new List<string>();

    int currentCore = 0;
    int coreCapacity = 0;

    public void InvokePanel(List<string> items)
    {
        this.items = items;

        StackBar.stackBarController.InventoryDeployed = true;
        StackBar.stackBarController.planetaryLoading = this;

        RefreshPackedItems();
    }


    void Update()
    {
        if ((!StackBar.stackBarController.InventoryDeployed && StackBar.stackBarController.planetaryLoading == null) || GInput.GetKeyDown(KeyCode.Q) || planetPanel.GetComponent<PlanetMenuController>().targetResourceLauncher == null)
        {
            if (currentCore != 0)
            {
                if (!StackBar.AddItem(currentCore)) GameManager.gameManagerReference.player.PlayerRelativeDrop(currentCore, 1);

                if (planetPanel.GetComponent<PlanetMenuController>().targetResourceLauncher != null)
                {
                    foreach (string item in items)
                    {
                        string text = item;
                        text = text.Remove(item.Length - 1);
                        int[] datas = ManagingFunctions.ConvertStringToIntArray(text.Split(':'));

                        int toDrop = datas[1];
                        for (int i = 0; i < datas[1]; i++)
                        {
                            bool booleanBool = StackBar.AddItem(datas[0]);
                            if (!booleanBool)
                            {
                                break;
                            }
                            toDrop--;
                        }

                        if (toDrop > 0)
                        {
                            GameManager.gameManagerReference.player.PlayerRelativeDrop(datas[0], toDrop);
                        }
                    }
                    planetPanel.GetComponent<PlanetMenuController>().Items = null;
                }
            }

            currentCore = 0;
            StackBar.stackBarController.InventoryDeployed = false;
            StackBar.stackBarController.planetaryLoading = null;
            gameObject.SetActive(false);
        }

        if(currentCore == 0)
        {
            blockedViewport.gameObject.SetActive(true);
            eToLaunch.gameObject.SetActive(false);
            coreReporter.GetChild(0).GetComponent<Image>().sprite = GameManager.gameManagerReference.tiles[0];
            coreReporter.GetChild(1).GetComponent<Text>().text = "[No core selected]";
        }
        else
        {
            blockedViewport.gameObject.SetActive(false);
            eToLaunch.gameObject.SetActive(true);

            if (GInput.GetKeyDown(KeyCode.E))
            {
                Close();
            }

            coreReporter.GetChild(0).GetComponent<Image>().sprite = GameManager.gameManagerReference.tiles[currentCore];
            coreReporter.GetChild(1).GetComponent<Text>().text = GameManager.gameManagerReference.tileName[currentCore];
        }
    }

    public void Close()
    {
        StackBar.stackBarController.InventoryDeployed = false;
        StackBar.stackBarController.planetaryLoading = null;
        planetPanel.gameObject.SetActive(true);
        planetPanel.GetComponent<PlanetMenuController>().GoToPlanet();
        gameObject.SetActive(false);
    }

    public void MovePackedItems(int move)
    {
        viewport.anchoredPosition = new Vector2(0, Mathf.Clamp(viewport.anchoredPosition.y + (move * 15), 0, -viewport.GetChild(viewport.childCount - 1).GetComponent<RectTransform>().anchoredPosition.y));
    }

    public string AddToPackup(int item, int amount)
    {

        if (GameManager.gameManagerReference.tileType[item] == "core")
        {
            int previousCore = currentCore;
            currentCore = item;
            return previousCore + ":1";
        }
        else if (currentCore != 0)
        {
            if (currentCore != 0)
            {
                int finalAmount = amount;
                items.Add(item + ":" + finalAmount + ";");
                planetPanel.GetComponent<PlanetMenuController>().Items = items;
                RefreshPackedItems();
                return "0:0";
            }
            else
            {
                return item + ":" + amount;
            }
        }

        return item + ":" + amount;
    }

    public void RefreshPackedItems()
    {
        for (int i = 1; i < viewport.childCount; i++)
        {
            Destroy(viewport.GetChild(i).gameObject);
        }
        Invoke("UpdatePackedItems", Time.deltaTime);
    }

    public void UpdatePackedItems()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i > viewport.childCount - 2)
            {
                string text = items[i];
                text = text.Remove(text.Length - 1);
                int tile = 0;
                int tileAmount = 0;

                tile = System.Convert.ToInt32(text.Split(':')[0]);
                tileAmount = System.Convert.ToInt32(text.Split(':')[1]);

                if (tileAmount > 0)
                {
                    GameObject gameObject = Instantiate(itemPackedPrefab, viewport);
                    gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (i * -50) + 147.5f);
                    gameObject.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.gameManagerReference.tiles[tile];
                    gameObject.transform.GetChild(1).GetComponent<Text>().text = GameManager.gameManagerReference.tileName[tile];
                    gameObject.transform.GetChild(2).GetComponent<Text>().text = tileAmount + "";
                }
            }
        }
    }
}
