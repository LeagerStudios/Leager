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

    void Update()
    {
        if (GInput.GetKey(KeyCode.Period))
        {
            Zoom();
        }
        if (GInput.GetKey(KeyCode.Minus))
        {
            UnZoom();
        }
    }

    public void Zoom()
    {
        mapSize -= 5 * Time.deltaTime;
        mapSize = Mathf.Clamp(mapSize, minMapSize, maxMapSize);
        minimapCamera.GetComponent<Camera>().orthographicSize = mapSize;
    }

    public void UnZoom()
    {
        mapSize += 5 * Time.deltaTime;
        mapSize = Mathf.Clamp(mapSize, minMapSize, maxMapSize);
        minimapCamera.GetComponent<Camera>().orthographicSize = mapSize;
    }
}