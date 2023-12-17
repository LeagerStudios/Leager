using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanDoActionIconController : MonoBehaviour
{

    [SerializeField] Sprite[] icons;
    GameManager manager;
    [SerializeField] PlayerController player;
    Vector3 origScale;

    void Start()
    {
        manager = GameManager.gameManagerReference;
        origScale = GetComponent<RectTransform>().localScale;
    }

    void Update()
    {

        if (manager == null) manager = GameManager.gameManagerReference;

        SetIcon();

    }

    private void SetIcon()
    {
        Sprite lastSprite = GetComponent<Image>().sprite;

        if (!player.alive)
        {
            GetComponent<Image>().sprite = icons[3];
        }
        else if (manager.canAtack && !manager.usingArm)
        {
            GetComponent<Image>().sprite = icons[2];
        }
        else if (manager.canBuild && !manager.building)
        {
            GetComponent<Image>().sprite = icons[1];
        }
        else if (manager.canUseTool && !manager.usingTool)
        {
            GetComponent<Image>().sprite = icons[4];
        }
        else if (manager.canEquip)
        {
            GetComponent<Image>().sprite = icons[5];
        }
        else if (manager.canConsume)
        {
            GetComponent<Image>().sprite = icons[6];
        }
        else
        {
            GetComponent<Image>().sprite = icons[0];
        }

        if(lastSprite != GetComponent<Image>().sprite)
        {
            StartCoroutine(ChangeIconShake());
        }
    }

    IEnumerator ChangeIconShake()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = origScale;
        
        Vector3 posVector = rectTransform.localScale;
        for (int frame = 0; frame < 2; frame++)
        {
            posVector.x += 4 * Time.deltaTime;
            posVector.y += 4 * Time.deltaTime;
            rectTransform.localScale = posVector;
            yield return new WaitForSeconds(0.01666f);
        }
        for (int frame = 0; frame < 2; frame++)
        {
            posVector.x -= 4 * Time.deltaTime;
            posVector.y -= 4 * Time.deltaTime;
            rectTransform.localScale = posVector;
            yield return new WaitForSeconds(0.01666f);
        }
        rectTransform.localScale = origScale;
    }
}
