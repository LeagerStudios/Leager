using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] public GameObject focus;
    [SerializeField] float delay = 0f;
    public bool transparentSkybox = false;
    Vector2 actualCameraPos = new Vector2();
    Vector2 nextCameraPos = new Vector2();

    void Start()
    {
        actualCameraPos = new Vector2(transform.position.x, transform.position.y);
    }
    void Update()
    {

        nextCameraPos = new Vector2(focus.transform.position.x, focus.transform.position.y);
        //Vector2 camPosition = Vector2.Lerp(actualCameraPos, nextCameraPos, delay);
        //camPosition.x = nextCameraPos.x;
        if (!transparentSkybox)
        {
            Color color = GameManager.gameManagerReference.daytimeUpdatedSkyboxColor;
            color.a = 1;
            GetComponent<Camera>().backgroundColor = color;
        }
        else
        {
            GetComponent<Camera>().backgroundColor = GameManager.gameManagerReference.daytimeUpdatedSkyboxColor;
        }
        transform.position = new Vector3(nextCameraPos.x, nextCameraPos.y, -10f);
        actualCameraPos = transform.position;
    }
}
