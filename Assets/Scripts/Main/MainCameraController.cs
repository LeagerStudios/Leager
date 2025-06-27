using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCameraController : MonoBehaviour {

    public string Focus = "mainMenu";
    public float focusX = 0f;
    public float focusY = 0f;
    public Color skyboxColor;
    public AnimationCurve dayNightCycle;
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject backgroundTrees;
    [SerializeField] GameObject backgroundMountains;
    [SerializeField] Transform stars;
    [SerializeField] SpriteRenderer[] landscapes;

    void Update()
    {
        SetSkybox();

        if(Focus == "selectWorldOptionsExit")
        {
            if (WorldPanelController.worldPanelController.listOfLoadedWorlds.Count < 1)
            {
                Focus = "mainMenu";
            }
            else
            {
                Focus = "selectWorldOptions";
            }
        }

        if (WorldPanelController.worldPanelController.networkWorldSelection)
        {
            if(Focus != "selectWorldOptions")
            {
                WorldPanelController.worldPanelController.networkWorldSelection = false;
            }
        }

        switch (Focus)
        {
            case "mainMenu":
                FocusOnPosition(0f, 0f);
                break;
            case "options":
                FocusOnPosition(-700f, 0f);
                if (GInput.GetKeyDown(KeyCode.Escape) || GInput.GetKeyDown(KeyCode.B)) Focus = "mainMenu";
                break;
            case "multiplayer":
                FocusOnPosition(-1400f, 0f);
                if (GInput.GetKeyDown(KeyCode.Escape)) Focus = "mainMenu";
                break;
            case "selectWorldOptions":
                if(WorldPanelController.worldPanelController.listOfLoadedWorlds.Count < 1)
                {
                    Focus = "makeWorldOptions";
                    FocusOnPosition(1400f, 0f);
                }
                else
                {
                    FocusOnPosition(700f, 0f);
                    if (GInput.GetKeyDown(KeyCode.Escape) || GInput.GetKeyDown(KeyCode.B)) Focus = "mainMenu";
                }
                break;
            case "makeWorldOptions":
                FocusOnPosition(1400f, 0f);
                if (GInput.GetKeyDown(KeyCode.Escape) || (GInput.GetKeyDown(KeyCode.B) && !WorldPanelController.worldPanelController.newWorldSeed.GetComponent<InputField>().isFocused && !WorldPanelController.worldPanelController.newWorldName.GetComponent<InputField>().isFocused)) Focus = "selectWorldOptions";
                break;
            case "loginPanel":
                FocusOnPosition(2100f, 0f);
                if (GInput.GetKeyDown(KeyCode.Escape)) Focus = "mainMenu";
                break;
            case "signInPanel":
                FocusOnPosition(2800f, 0f);
                if (GInput.GetKeyDown(KeyCode.Escape)) Focus = "loginPanel";
                break;
        }

        focusY = canvas.GetComponent<RectTransform>().localPosition.y;
        canvas.GetComponent<RectTransform>().localPosition = Vector2.Lerp(canvas.GetComponent<RectTransform>().localPosition, new Vector2(focusX, focusY), 0.2f);
        backgroundTrees.transform.localPosition = canvas.GetComponent<RectTransform>().localPosition / 350f;
        backgroundMountains.transform.localPosition = canvas.GetComponent<RectTransform>().localPosition / 500f;
    }

    public void ChangeFocus(string newFocus)
    {
        Focus = newFocus;
    }

    public void FocusOnPosition(float x, float y)
    {
        focusX = x;
        focusY = y;
    }

    private void SetSkybox()
    {
        float dayLuminosity = 0;
        float value = 1;
        float time = Time.time;

        value = dayNightCycle.Evaluate(time % 120 / 120);
        stars.eulerAngles = Vector3.forward * ((time % 120 / 120) * -180 - 45f);

        dayLuminosity = Mathf.Clamp(value, 0.1f, 1);

        foreach(SpriteRenderer renderer in landscapes)
        {
            renderer.color = new Color(dayLuminosity * 3, dayLuminosity * 3, dayLuminosity * 3);
        }

        GetComponent<Camera>().backgroundColor = skyboxColor * dayLuminosity;
    }
}
