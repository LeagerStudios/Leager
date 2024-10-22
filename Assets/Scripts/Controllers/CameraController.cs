using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] public GameObject focus;
    [SerializeField] float delay = 0f;
    public bool lerp = false;
    public bool transparentSkybox = false;
    public bool supportsBackgrounds = false;
    public string currentBackground = "";
    public Transform mainStuff;

    void Start()
    {
        Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreen);
    }
    void Update()
    {
        if (focus != null)
        {
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

            if (!lerp)
            {
                transform.position = new Vector3(focus.transform.position.x, focus.transform.position.y, -10f);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(focus.transform.position.x, focus.transform.position.y, -10f), Time.deltaTime * 10);

                if (Vector2.Distance(transform.position, focus.transform.position) < 0.05f)
                {
                    lerp = false;
                }
            }
        }
        else
        {
            focus = GameManager.gameManagerReference.player.gameObject;

            focus.GetComponent<PlayerController>().onControl = true;
            MenuController.menuController.UIActive = true;
            lerp = true;
        }

        if (supportsBackgrounds)
        {
            SpriteRenderer[] backgrounds = GetComponentsInChildren<SpriteRenderer>();
            int i = 0;

            foreach(SpriteRenderer background in backgrounds)
            {
                if (background.gameObject.name.Contains("#"))
                {
                    Color color = background.color;

                    if (i > 0)
                    {
                        background.size = Vector2.one * LightController.lightController.lightDist;
                    }
                    else
                    {
                        background.size = new Vector2(LightController.lightController.lightDist, 5);
                    }

                    if (background.gameObject.name.Contains(currentBackground))
                    {
                        color.a = Mathf.Clamp01(color.a + Time.deltaTime);
                    }
                    else
                    {
                        color.a = Mathf.Clamp01(color.a - Time.deltaTime);
                    }

                    background.enabled = color.a != 0f;
                    background.color = color;
                    i++;
                }
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
