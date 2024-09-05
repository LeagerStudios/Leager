using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxMenu : MonoBehaviour
{
    [SerializeField] RectTransform innerViewport;
    [SerializeField] CanvasGroup canvasGroup;
    
    [SerializeField] GameObject itemPrefab;
    public Box targetBox;
    public RectTransform rectTransform;
    public RectTransform canvasRect;
    public bool closing = false;


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

    void Update()
    {
        if(targetBox != null && !closing)
        {
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(targetBox.transform.position);
            Vector2 FollowerScreenPosition = new Vector2((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                                                         (ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));

            rectTransform.anchoredPosition = FollowerScreenPosition;

            innerViewport.anchoredPosition += new Vector2((35 * (innerViewport.childCount - 1) - innerViewport.anchoredPosition.x) * Time.deltaTime * 5f , 0f);
        }
        else
        {
            closing = true;

            canvasGroup.alpha -= Time.deltaTime * 2f;
            if(canvasGroup.alpha <= 0f)
            {
                Destroy(gameObject);
            }
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
        Destroy(innerViewport.GetChild(innerViewport.childCount - 1).gameObject);
    }
}
