using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour {

    [SerializeField] public GameObject focus;
    [SerializeField] float delay = 0f;
    public bool transparentSkybox = false;
    public bool supportsBackgrounds = false;
    public PostProcessProfile blurProfile;
    public string currentBackground = "";

    Vector2 actualCameraPos = new Vector2();
    Vector2 nextCameraPos = new Vector2();

    void Start()
    {
        actualCameraPos = new Vector2(transform.position.x, transform.position.y);
        //Screen.SetResolution ((int)Screen.width, (int)Screen.height, true);
    }
    void Update()
    {
        if (focus != null)
        {
            nextCameraPos = new Vector2(focus.transform.position.x, focus.transform.position.y);

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
            focus = GameManager.gameManagerReference.player.gameObject;

            focus.GetComponent<PlayerController>().onControl = true;
            MenuController.menuController.UIActive = true;
        }

        if(blurProfile != null)
        {
            if(blurProfile.TryGetSettings(out DepthOfField depth))
            {
                if (GameManager.gameManagerReference.InGame)
                {
                    depth.focusDistance.value += Time.deltaTime * 2;
                }
                else
                {
                    depth.focusDistance.value -= Time.deltaTime;
                }

                depth.focusDistance.value = Mathf.Clamp01(depth.focusDistance);
            }
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

                background.enabled = color.a != 0f;
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
