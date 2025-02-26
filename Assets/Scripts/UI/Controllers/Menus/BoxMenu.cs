using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxMenu : MonoBehaviour
{
    [SerializeField] RectTransform innerViewport;
    [SerializeField] RectTransform outerViewport;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Text text;

    [SerializeField] GameObject itemPrefab;
    public Box targetBox;
    public RectTransform rectTransform;
    public RectTransform canvasRect;


    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = MenuController.menuController.canvas.GetComponent<RectTransform>();

        int[][] datas = targetBox.FetchItems();

        for (int i = 0; i < datas[0].Length; i++)
        {
            Add(datas[0][i], datas[1][i]);
        }

        innerViewport.anchoredPosition = Vector2.right * 35 * (datas[0].Length - 1);
    }

    public void UpdatePos()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = MenuController.menuController.canvas.GetComponent<RectTransform>();

        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(targetBox.transform.position);
        Vector2 FollowerScreenPosition = new Vector2((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                                                     (ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));

        rectTransform.anchoredPosition = FollowerScreenPosition;

        innerViewport.anchoredPosition += new Vector2((35 * (innerViewport.childCount - 1) - innerViewport.anchoredPosition.x) * Time.deltaTime * 5f, 0f);
        text.text = innerViewport.childCount + "/" + targetBox.maxStacks;
    }

    void Update()
    {
        if (targetBox != null)
        {
            UpdatePos();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    GameObject MakeTile(int tile, int amount, Vector2 position)
    {
        GameObject gameObject = Instantiate(itemPrefab, innerViewport);
        gameObject.GetComponent<RectTransform>().anchoredPosition = position;

        gameObject.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.gameManagerReference.tiles[tile];
        if (GameManager.gameManagerReference.stackLimit[tile] > 1)
        {
            gameObject.transform.GetChild(1).GetComponent<Text>().text = amount + "";
        }
        else
        {
            gameObject.transform.GetChild(1).GetComponent<Text>().text = "";
        }
        

        return gameObject;
    }


    public void Add(int tile, int amount, bool insta = true)
    {
        GameObject tileObject = MakeTile(tile, amount, Vector2.right * (-35 * innerViewport.childCount));

        if (!insta)
        {

        }
    }


    public void RemoveLast()
    {
        RectTransform child = innerViewport.GetChild(innerViewport.childCount - 1).GetComponent<RectTransform>();
        Vector2 pos = child.position;
        child.SetParent(outerViewport, false);
        child.position = pos;
        StartCoroutine(FadeOut(child));
    }

    IEnumerator FadeOut(RectTransform tile)
    {
        CanvasGroup group = tile.GetComponent<CanvasGroup>();

        while(group.alpha > 0)
        {
            tile.localScale += Vector3.one * 2 * Time.deltaTime;
            group.alpha -= Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }

        Destroy(tile.gameObject);
    }
}
