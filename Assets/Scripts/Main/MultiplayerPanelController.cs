using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class MultiplayerPanelController : MonoBehaviour
{
    private const int ServerDiscoveryPort = 7777;
    [SerializeField] CanvasGroup panel;
    [SerializeField] RectTransform hostedPanel;

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
