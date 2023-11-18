using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackupMenuController : MonoBehaviour, IOnCollisionEnterReporter
{
    [SerializeField] GameObject collisionDetector;
    [SerializeField] GameObject itemPackedPrefab;
    [SerializeField] GameObject arrowButtons;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] RectTransform canvasRect;
    [SerializeField] RectTransform viewport;
    Transform follower;
    PlanetMenuController invoker;

    List<string> items = new List<string>();


    public void InvokeMenu(Transform followTo, PlanetMenuController self, List<string> items)
    {
        rectTransform = GetComponent<RectTransform>();
        viewport = (RectTransform)rectTransform.GetChild(1).GetChild(1).GetChild(1);
        canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
        follower = followTo;
        invoker = self;
        this.items = items;

        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(follower.transform.position);
        Vector2 FollowerScreenPosition = new Vector2((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                                                     (ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));

        UpdatePackedItems();
        rectTransform.anchoredPosition = FollowerScreenPosition;
    }


    void Update()
    {
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(follower.transform.position);
        Vector2 FollowerScreenPosition = new Vector2((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                                                     (ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));

        rectTransform.anchoredPosition = FollowerScreenPosition;

        if (follower.GetComponent<SpriteRenderer>().sprite != GameManager.gameManagerReference.tiles[89])
        {
            Close();
        }

        arrowButtons.SetActive(items.Count > 3);
    }

    public void Close()
    {
        invoker.gameObject.SetActive(true);
        Destroy(gameObject);
    }

    public void MovePackedItems(int move)
    {
        viewport.anchoredPosition = new Vector2(0, Mathf.Clamp(viewport.anchoredPosition.y + (move * 15), 0, -viewport.GetChild(viewport.childCount - 1).GetComponent<RectTransform>().anchoredPosition.y));
    }

    public void Packup()
    {
        GameObject e = Instantiate(collisionDetector, follower.transform.position - Vector3.up, Quaternion.identity);
        e.GetComponent<OnCollisionEnterDetector>().InvokeObj(this, Vector2.one, "DroppedItem");
    }

    public void AddToPackup(string stuff)
    {
        items.Add(stuff);
        invoker.items = items;

        UpdatePackedItems();
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

    public void Call(Collider2D collision)
    {
        DroppedItemController item = collision.transform.GetComponent<DroppedItemController>();
        AddToPackup(System.Array.IndexOf(GameManager.gameManagerReference.tiles, item.GetComponent<SpriteRenderer>().sprite) + ":" + item.amount + ";");
        item.amount = 0;
    }
}
