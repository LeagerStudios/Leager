using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxMenu : MonoBehaviour
{
    [SerializeField] RectTransform innerViewport;
    [SerializeField] RectTransform outerViewport;
    [SerializeField] GameObject itemPrefab;

    public int[] localItems;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    GameObject MakeTile(int tile, int amount, Vector2 position, RectTransform viewport)
    {
        GameObject gameObject = Instantiate(itemPrefab, viewport);
        gameObject.GetComponent<RectTransform>().anchoredPosition = position;

        gameObject.transform.GetChild(0).GetComponent<Image>().sprite = null;//todo
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
}
