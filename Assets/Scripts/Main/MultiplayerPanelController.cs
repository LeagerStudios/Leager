using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerPanelController : MonoBehaviour
{
    [SerializeField] CanvasGroup panel;

    void Update()
    {
        if (Camera.main.GetComponent<MainCameraController>().Focus == "multiplayer")
        {
            panel.alpha += 3 * Time.deltaTime;
        }
        else
        {
            panel.alpha -= 3 * Time.deltaTime;
        }
    }
}
