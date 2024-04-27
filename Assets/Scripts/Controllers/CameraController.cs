using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] public GameObject focus;
    [SerializeField] float delay = 0f;
    public bool transparentSkybox = false;
    public bool supportsBackgrounds = false;
    public string currentBackground = "";

    Vector2 actualCameraPos = new Vector2();
    Vector2 nextCameraPos = new Vector2();

    void Start()
    {
        actualCameraPos = new Vector2(transform.position.x, transform.position.y);
        Screen.SetResolution ((int)Screen.width, (int)Screen.height, true);
    }
    void Update()
    {
        if (focus != null)
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
        else
        {
            focus = GameObject.Find("Lecter");

            focus.GetComponent<PlayerController>().onControl = true;
            MenuController.menuController.UIActive = true;
        }

        if (supportsBackgrounds)
        {
            SpriteRenderer[] backgrounds = GetComponentsInChildren<SpriteRenderer>();

            foreach(SpriteRenderer background in backgrounds)
            {
                Color color = background.color;

                if(background.gameObject.name.Contains(currentBackground))
                {
                    color.a = Mathf.Clamp01(color.a + Time.deltaTime);
                }
                else
                {
                    color.a = Mathf.Clamp01(color.a - Time.deltaTime);
                }

                background.color = color;
            }
        }
    }

    public void Turn()
    {
        if(Random.Range(0,2) == 0)
        {
            transform.eulerAngles = Vector3.forward * -15;
        }
        else
        {
            transform.eulerAngles = Vector3.forward * 15;
        }
    }
}
