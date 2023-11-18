using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    public static MinimapController minimapController;
    float mapSize = 12;
    public int minMapSize = 12;
    public int maxMapSize = 55;
    public GameObject minimapCamera;

    void Start()
    {
        mapSize = Mathf.Clamp(mapSize, minMapSize, maxMapSize);
        minimapCamera.GetComponent<Camera>().orthographicSize = mapSize;
        minimapController = this;
    }

    public void Zoom()
    {
        mapSize -= 1;
        mapSize = Mathf.Clamp(mapSize, minMapSize, maxMapSize);
        minimapCamera.GetComponent<Camera>().orthographicSize = mapSize;
        LightController.lightController.AddRenderQueue(GameManager.gameManagerReference.player.transform.position);
    }

    public void AZoom()
    {
        mapSize += 1;
        mapSize = Mathf.Clamp(mapSize, minMapSize, maxMapSize);
        minimapCamera.GetComponent<Camera>().orthographicSize = mapSize;
        LightController.lightController.AddRenderQueue(GameManager.gameManagerReference.player.transform.position);
    }
}