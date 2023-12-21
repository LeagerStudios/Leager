using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionIconController : MonoBehaviour
{

    [SerializeField] Sprite[] icons;
    GameManager manager;
    MainSoundController soundController;
    [SerializeField] PlayerController player;
    Vector3 origPos;

    void Start()
    {
        manager = GameManager.gameManagerReference;
        soundController = GameObject.Find("Audio").GetComponent<MainSoundController>();
        origPos = GetComponent<RectTransform>().anchoredPosition;
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
        else if (manager.usingArm)
        {
            GetComponent<Image>().sprite = icons[2];
        }
        else if (manager.building)
        {
            GetComponent<Image>().sprite = icons[1];
        }
        else if (manager.usingTool)
        {
            GetComponent<Image>().sprite = icons[4];
        }
        else
        {
            GetComponent<Image>().sprite = icons[0];
        }

        if(lastSprite != GetComponent<Image>().sprite && lastSprite != icons[3])
        {
            StartCoroutine(ChangeIconShake());
        }
    }

    IEnumerator ChangeIconShake()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = origPos;
        soundController.PlaySfxSound(SoundName.select);

        Vector3 posVector = rectTransform.position;
        for (int frame = 0; frame < 2; frame++)
        {
            posVector.y += 320 * Time.deltaTime;
            rectTransform.position = posVector;
            yield return new WaitForSeconds(0.016f);
        }
        for (int frame = 0; frame < 4; frame++)
        {
            posVector.y -= 320 * Time.deltaTime;
            rectTransform.position = posVector;
            yield return new WaitForSeconds(0.016f);
        }
        for (int frame = 0; frame < 2; frame++)
        {
            posVector.y += 160 * Time.deltaTime;
            rectTransform.position = posVector;
            yield return new WaitForSeconds(0.016f);
        }

        rectTransform.anchoredPosition = origPos;
    }
}
