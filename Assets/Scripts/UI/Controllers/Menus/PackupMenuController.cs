using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackupMenuController : MonoBehaviour
{
    [SerializeField] GameObject itemPackedPrefab;
    [SerializeField] RectTransform viewport;
    [SerializeField] RectTransform coreReporter;
    [SerializeField] public RectTransform planetPanel;

    List<string> items = new List<string>();

    int currentCore = 0;

    public void InvokePanel(List<string> items)
    {
        this.items = items;

        UpdatePackedItems();
    }


    void Update()
    {
        if (GInput.GetKeyDown(KeyCode.E))
        {
            Close();
        }
    }

    public void Close()
    {
        StackBar.stackBarController.InventoryDeployed = false;
        planetPanel.gameObject.SetActive(true);
        planetPanel.GetComponent<PlanetMenuController>().GoToPlanet();
        gameObject.SetActive(false);
    }

    public void MovePackedItems(int move)
    {
        viewport.anchoredPosition = new Vector2(0, Mathf.Clamp(viewport.anchoredPosition.y + (move * 15), 0, -viewport.GetChild(viewport.childCount - 1).GetComponent<RectTransform>().anchoredPosition.y));
    }

    public void AddToPackup(string stuff, int item)
    {
        if (GameManager.gameManagerReference.tileType[item] == "core")
        {

        }
        else
        {
            items.Add(stuff);
            planetPanel.GetComponent<PlanetMenuController>().items = items;
            UpdatePackedItems();
        }
    }

    public void UpdatePackedItems()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i > viewport.childCount - 1)
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
                    gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (i * -30) + 35f);
                    gameObject.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.gameManagerReference.tiles[tile];
                    gameObject.transform.GetChild(1).GetComponent<Text>().text = tileAmount + "";
                }
            }
        }
    }
}
