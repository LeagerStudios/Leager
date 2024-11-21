using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlanetMenuController : MonoBehaviour, IDraggable
{

    public ResourceLauncher targetResourceLauncher;
    public static PlanetMenuController planetMenu;

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
    public List<string> lorePlanets = new List<string> { "Korenz", "Dua", "Intersection", "Nheo", "Lurp", "Krylo" };
    public Color[] lorePlanetsColor; 
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
    public float zoom = 100f;

    public bool CanDrag { get => planetSelectionFocused; }

    private void Awake()
    {
        planetMenu = this;
    }

    void Start ()
    {
        rectTransform = GetComponent<RectTransform>();

        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldRootName + @"/planets.lgrsd"))
        {
            planets.Add(new PlanetData("Sun", ManagingFunctions.HexToColor("#FFFE00"), 700, 0, 0) { canGo = false });
            planets.Add(new PlanetData("Korenz", ManagingFunctions.HexToColor("#25FF00"), 300, 100f, 90f, -90f) { parent = planets[0] });
            planets.Add(new PlanetData("Dua", ManagingFunctions.HexToColor("#04CAD1"), 140, 10f, 10f) { parent = planets[1] });
            planets.Add(new PlanetData("Intersection", ManagingFunctions.HexToColor("#EBD33D"), 250, 20f, 15f) { parent = planets[1] });
            planets.Add(new PlanetData("Lurp", ManagingFunctions.HexToColor("#878787"), 70, 20f, 20f) { parent = planets[0] });
            planets.Add(new PlanetData("Nheo", ManagingFunctions.HexToColor("#FFFE00"), 235, 80f, 30f, -90f) { parent = planets[0] });
            planets.Add(new PlanetData("Krylo", ManagingFunctions.HexToColor("#252525"), 564, 380f, 120f, 76f) { parent = planets[0] });
            DataSaver.SerializeAt(planets, Application.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldRootName + @"/planets.lgrsd");
        }
        else
        {
            planets = DataSaver.DeSerializeAt<List<PlanetData>>(Application.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldRootName + @"/planets.lgrsd");
        }

        foreach (PlanetData planet in planets)
        {
                RectTransform newPlanet = Instantiate(planetPrefab, planetPanelRectTransform.GetChild(0)).GetComponent<RectTransform>();
                newPlanet.anchoredPosition = new Vector2(0f, -25f - (newPlanet.GetSiblingIndex() * 50f));
                int idx = planets.IndexOf(planet);
                newPlanet.GetChild(0).GetComponent<Button>().onClick.AddListener(() => FocusPlanet(idx));

                planet.ApplyToButton(newPlanet);
        }
    }

    void Update()
    {
        if (GInput.GetKeyDown(KeyCode.Escape) || targetResourceLauncher == null)
        {
            planetFocused = -1;
            targetResourceLauncher = null;
            planetSelectionFocused = true;
            planetPanelPropertiesRectTransform.gameObject.SetActive(false);
            planetPanelRectTransform.gameObject.SetActive(false);

            GameManager.gameManagerReference.InGame = true;
            transform.gameObject.SetActive(false);
            return;
        }

        Drag();

        planetPanelPropertiesRectTransform.gameObject.SetActive(!planetSelectionFocused);

        if (planetSelectionFocused)
        {

            RectTransform viewport = planetPanelRectTransform.GetChild(0).GetComponent<RectTransform>();
            zoom += Input.mouseScrollDelta.y * 5;
            zoom = Mathf.Clamp(zoom, 10, 100);
            viewport.sizeDelta = Vector2.Lerp(viewport.sizeDelta, Vector2.one * zoom, Time.deltaTime * 10);

            foreach (PlanetData planet in planets)
            {
                Vector2 point = planet.FindPoint(Time.time);
                RectTransform planetObject = viewport.GetChild(planets.IndexOf(planet)).GetComponent<RectTransform>();

                planetObject.sizeDelta = viewport.sizeDelta * (planet.chunkSize / 120);
                if(planet.parent != null)
                {
                    planetObject.anchoredPosition = (point * viewport.sizeDelta.x) + viewport.GetChild(planets.IndexOf(planet.parent)).GetComponent<RectTransform>().anchoredPosition;
                }
                else
                {
                    planetObject.anchoredPosition = (point * viewport.sizeDelta.x);
                }

                if(planet.planetName == GameManager.gameManagerReference.currentPlanetName)
                {
                    viewport.anchoredPosition = (-planetObject.anchoredPosition);
                }
            }
        }
    }

    public void Drag()
    {
        
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
        

        RectTransform planetDataRectTransform = planetPanelPropertiesRectTransform.GetChild(0).GetComponent<RectTransform>();

        planetDataRectTransform.GetChild(0).GetComponent<Image>().color = planets[idx].planetColor.Color;
        planetDataRectTransform.GetChild(1).GetComponent<Text>().text = planets[idx].planetName;

        Color.RGBToHSV(planets[idx].planetColor.Color, out float h, out float s, out float v);
        if(v > 0.5f)
        planetDataRectTransform.GetChild(1).GetComponent<Text>().color = planets[idx].planetColor.Color;
        else
            planetDataRectTransform.GetChild(1).GetComponent<Text>().color = Color.white;


        RectTransform propertiesRectTransform = planetPanelPropertiesRectTransform.GetChild(1).GetComponent<RectTransform>();


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
        subPanel.GetComponent<PackupMenuController>().currentCore = 0;
        Items = new List<string>();

        gameObject.SetActive(false);
    }

    public void SpawnPlanet()
    {
        int letters = Random.Range(2, 5);
        int numbers = 6 - letters;
        string planetName = "null-22";
        planetName = ManagingFunctions.GetRandomStringUpper(letters) + "-" + ManagingFunctions.GetRandomStringNumbers(numbers);

        bool canSpawn = true;
        for (int i = 0; i < planets.Count; i++)
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
            newPlanet.anchoredPosition = new Vector2(0f, -25f - (newPlanet.GetSiblingIndex() * 100f));
            int idx = planets.Count;
            newPlanet.GetChild(0).GetComponent<Button>().onClick.AddListener(() => FocusPlanet(idx));
            int size = Random.Range(20, 250);

            PlanetData planetData = new PlanetData(planetName, Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0.7f, 1f), Random.Range(0.8f, 1f)), size, Random.Range(3f, 6f), Random.Range(1f, 3f));
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
    public float apoapsis = Random.Range(3f, 6f);
    public float periapsis = Random.Range(1f, 3f);
    public float rotation = 0;
    public bool canGo = true;
    public PlanetData parent;

    public PlanetData(string name, Color color, int sizeInChunks, float apo, float per, float rot = 0)
    {
        planetName = name;
        planetColor.AssignColor(color);
        chunkSize = sizeInChunks;

        if (sizeInChunks < 25)
        {
            wordSize = "Planetary Fortress";
        }
        else if (sizeInChunks < 150)
        {
            wordSize = "Small";
        }
        else if (sizeInChunks < 275)
        {
            wordSize = "Medium";
        }
        else if (sizeInChunks < 320)
        {
            wordSize = "Big";
        }
        else if (sizeInChunks < 490)
        {
            wordSize = "Very Big";
        }
        else if (sizeInChunks < 520)
        {
            wordSize = "Massive";
        }
        else
        {
            wordSize = "Extremely Massive";
        }

        apoapsis = apo;
        periapsis = per;
        rotation = rot;
    }

    public string ColorToHex()
    {
        return ColorUtility.ToHtmlStringRGB(planetColor.Color);
    }

    public void ApplyToButton(RectTransform rectTransform)
    {
        Color.RGBToHSV(planetColor.Color, out float h, out float s, out float v);
        rectTransform.GetChild(0).GetComponent<Image>().color = planetColor.Color;
        rectTransform.GetChild(0).GetComponent<Button>().interactable = canGo;

        rectTransform.GetChild(0).GetChild(0).GetComponent<Text>().text = planetName;

        if (v > 0.4f)
            rectTransform.GetChild(0).GetChild(0).GetComponent<Text>().color = planetColor.Color;
        else
            rectTransform.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.white;
    }

    public Vector2 FindPoint(float time)
    {
        if (apoapsis == 0 && periapsis == 0)
        {
            return Vector2.zero;
        }
        else
        {
            Vector2 point = new Vector2();
            float timePerRevolution = apoapsis * periapsis;
            float rotVal = time % timePerRevolution;
            rotVal *= 2;
            rotVal /= timePerRevolution;
            bool inverse = false;
            if (rotVal > 1)
            {
                rotVal = 0 - (rotVal - 2);
                inverse = true;
            }

            float orbit = periapsis + apoapsis;

            float halfOrbit = Mathf.Lerp(apoapsis, periapsis, 0.8f);


            point = new Vector2(Mathf.Sin(rotVal * Mathf.PI) * halfOrbit * (inverse ? 1 : -1), Mathf.Cos(rotVal * Mathf.PI) * (orbit / 2));
            point += Vector2.up * ((apoapsis - periapsis) / 2);
            if (rotation != 0)
            {
                point = Quaternion.AngleAxis(rotation, Vector3.forward) * point;
            }

            return point;
        }
    }
}