using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlanetMenuController : MonoBehaviour {

    public ResourceLauncher targetResourceLauncher;

    RectTransform rectTransform;
    [SerializeField] RectTransform planetPanelRectTransform;
    [SerializeField] RectTransform planetPanelViewportRectTransform;
    [SerializeField] RectTransform planetPanelPropertiesRectTransform;
    [SerializeField] RectTransform planetPanelButtonsRectTransform;
    [SerializeField] public RectTransform subPanel;

    [SerializeField] GameObject goToPlanetButton;
    [SerializeField] GameObject connectButton;
    [SerializeField] GameObject connectButtonText;

    [SerializeField] GameObject planetPrefab;
    public List<PlanetData> planets = new List<PlanetData>();
    public List<string> lorePlanets = new List<string> { "Korenz", "Dua" };
    public List<string> Items
    {
        get
        {
            return targetResourceLauncher.Items;
        }
        set
        {
            targetResourceLauncher.Items = value;
        }
    }

    public bool planetSelectionFocused = true;
    public int planetFocused = -1;

	void Start ()
    {
        rectTransform = GetComponent<RectTransform>();

        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldRootName + @"/planets.lgrsd"))
        {
            planets.Add(new PlanetData("Korenz", ManagingFunctions.HexToColor("25FF00FF"), 50));
            planets.Add(new PlanetData("Dua", ManagingFunctions.HexToColor("#04CAD1"), 250));
            DataSaver.SerializeAt(planets, Application.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldRootName + @"/planets.lgrsd");
        }
        else
        {
            planets = DataSaver.DeSerializeAt<List<PlanetData>>(Application.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldRootName + @"/planets.lgrsd");

            foreach(PlanetData planet in planets)
            {
                if (!lorePlanets.Contains(planet.planetName))
                {
                    RectTransform newPlanet = Instantiate(planetPrefab, planetPanelRectTransform.GetChild(0)).GetComponent<RectTransform>();
                    newPlanet.anchoredPosition = new Vector2(0f, 197.5f - (newPlanet.GetSiblingIndex() * 50f));
                    int idx = newPlanet.GetSiblingIndex();
                    newPlanet.GetComponent<Button>().onClick.AddListener(() => FocusPlanet(idx));

                    PlanetData planetData = new PlanetData(planet.planetName, planet.planetColor.Color, planet.chunkSize);
                    planetData.ApplyToButton(newPlanet);
                }
            }
        }
    }

    void Update()
    {
        if (GInput.GetKeyDown(KeyCode.Escape))
        {
            GameManager.gameManagerReference.InGame = true;
            transform.gameObject.SetActive(false);
            return;
        }

        if (planetSelectionFocused)
            if (planetPanelViewportRectTransform.childCount < 9)
            {
                planetPanelViewportRectTransform.anchoredPosition = Vector2.zero;
            }
            else
            {
                planetPanelViewportRectTransform.anchoredPosition = new Vector2(0, Mathf.Clamp(planetPanelViewportRectTransform.anchoredPosition.y + (Input.mouseScrollDelta.y * 10), 0, (planetPanelViewportRectTransform.childCount - 8) * 50 + 5));
            }

        planetPanelPropertiesRectTransform.gameObject.SetActive(!planetSelectionFocused);
    }

    public void FocusPlanet(int idx)
    {
        planetFocused = idx;

        planetSelectionFocused = false;

        bool isThis = GameManager.gameManagerReference.currentPlanetName == planets[idx].planetName;
        bool isExplored;
        if (planets[idx].planetName == "Korenz")
        {
            isExplored = true;
        }
        else
            isExplored = DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldRootName + @"/" + planets[idx].planetName);

        RectTransform planetDataRectTransform = planetPanelPropertiesRectTransform.GetChild(1).GetComponent<RectTransform>();

        planetDataRectTransform.GetChild(0).GetComponent<Image>().color = planets[idx].planetColor.Color;
        planetDataRectTransform.GetChild(1).GetComponent<Text>().text = planets[idx].planetName;
        planetDataRectTransform.GetChild(1).GetComponent<Text>().color = planets[idx].planetColor.Color;


        RectTransform propertiesRectTransform = planetPanelPropertiesRectTransform.GetChild(2).GetComponent<RectTransform>();


        propertiesRectTransform.GetChild(0).gameObject.SetActive(isThis);
        connectButtonText.gameObject.SetActive(!isExplored);
        connectButton.GetComponent<Button>().interactable = isExplored && planets[planetFocused].planetName != GameManager.gameManagerReference.currentPlanetName;
        goToPlanetButton.GetComponent<Button>().interactable = planets[planetFocused].planetName != GameManager.gameManagerReference.currentPlanetName;
        propertiesRectTransform.GetChild(1).GetComponent<Text>().text = "Size: " + planets[idx].wordSize;

    }

    public void UnFocusPlanet()
    {
        planetSelectionFocused = true;
        planetFocused = -1;
    }

    public void OpenPackagerPanel()
    {
        GameManager.gameManagerReference.InGame = true;
        subPanel.gameObject.SetActive(true);
        subPanel.GetComponent<PackupMenuController>().InvokePanel(Items);
        gameObject.SetActive(false);
    }

    public void GoToPlanet()
    {
        GameManager.gameManagerReference.InGame = true;
        Camera.main.transform.GetComponent<CameraController>().focus = targetResourceLauncher.transform.parent.gameObject;
        GameManager.gameManagerReference.player.onControl = false;

        string resources = "";
        if (Items.Count < 1)
        {
            resources = "null";
        }
        else
            for (int i = 0; i < Items.Count; i++)
            {
                resources = resources + Items[i];
            }

        MenuController.menuController.PlanetaryTravel(planets[planetFocused], resources, targetResourceLauncher, GameManager.gameManagerReference.tiles[subPanel.GetComponent<PackupMenuController>().currentCore]);
        gameObject.SetActive(false);
    }

    public void SpawnPlanet()
    {
        string planetName = ManagingFunctions.GetRandomStringUpper(4) + "-" + ManagingFunctions.GetRandomStringNumbers(2);
        bool canSpawn = true;
        for(int i = 0; i < planets.Count; i++)
        {
            if (planets[i].planetName == planetName)
            {
                canSpawn = false;
                break;
            }
        }

        if (canSpawn)
        {
            RectTransform newPlanet = Instantiate(planetPrefab, planetPanelRectTransform.GetChild(0)).GetComponent<RectTransform>();
            newPlanet.anchoredPosition = new Vector2(0f, 197.5f - (newPlanet.GetSiblingIndex() * 50f));
            int idx = newPlanet.GetSiblingIndex();
            newPlanet.GetComponent<Button>().onClick.AddListener(() => FocusPlanet(idx));
            int size = Random.Range(20, 250);

            PlanetData planetData = new PlanetData(planetName, new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f)), size);
            planetData.ApplyToButton(newPlanet);
            planets.Add(planetData);

            DataSaver.SerializeAt(planets, Application.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldRootName + @"/planets.lgrsd");
        }
    }
}


[System.Serializable]
public class PlanetData
{
    public string planetName;
    public string planetNickname = "";
    public SerializableColor planetColor = new SerializableColor();
    public int chunkSize;
    public string wordSize;

    public PlanetData(string name, Color color, int sizeInChunks)
    {
        planetName = name;
        planetColor.AssignColor(color);
        chunkSize = sizeInChunks;

        if (sizeInChunks < 50)
        {
            wordSize = "Small";
        }
        else if (sizeInChunks < 75)
        {
            wordSize = "Medium";
        }
        else if (sizeInChunks < 120)
        {
            wordSize = "Big";
        }
        else if (sizeInChunks < 190)
        {
            wordSize = "Massive";
        }
        else
        {
            wordSize = "Extremely Massive";
        }
    }

    public string ColorToHex()
    {
        return ColorUtility.ToHtmlStringRGB(planetColor.Color);
    }

    public void ApplyToButton(RectTransform rectTransform)
    {
        rectTransform.GetChild(0).GetComponent<Image>().color = planetColor.Color;

        rectTransform.GetChild(1).GetComponent<Text>().text = planetName;
        rectTransform.GetChild(1).GetComponent<Text>().color = planetColor.Color;
    }
}