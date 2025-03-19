using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class PlanetMenuController : MonoBehaviour, IDraggable
{

    public ResourceLauncher targetResourceLauncher;
    public static PlanetData currentPlanet;
    public static PlanetMenuController planetMenu;
    public float timewarp = 1f;

    RectTransform rectTransform;
    [SerializeField] public RectTransform planetPanelRectTransform;
    [SerializeField] public RectTransform planetPanelViewportRectTransform;
    [SerializeField] RectTransform planetPanelPropertiesRectTransform;
    [SerializeField] RectTransform planetPanelButtonsRectTransform;
    [SerializeField] public RectTransform subPanel;
    [SerializeField] public RectTransform timewarpPanel;

    [SerializeField] GameObject goToPlanetButton;
    [SerializeField] GameObject connectButton;
    [SerializeField] GameObject connectButtonText;

    [SerializeField] GameObject planetPrefab;
    public List<PlanetData> planets = new List<PlanetData>();
    public List<string> lorePlanets = new List<string> { "Korenz", "Dua", "Intersection", "Nheo", "Lurp", "Krylo" };
    public Color[] lorePlanetsColor;

    public Sprite starSprite;
    public Sprite planetSprite;
    public Sprite moonSprite;

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

    public void Startt()
    {
        rectTransform = GetComponent<RectTransform>();

        if (GameManager.gameManagerReference.isNetworkClient)
        {
            planets = Client.planetsLoad;
        }
        else if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldRootName + @"/planets.lgrsd"))
        {
            planets.Add(new PlanetData("Sun", ManagingFunctions.HexToColor("#FFFE00"), 2700, 0, 0, 0, 0) { canGo = false });
            planets.Add(new PlanetData("Korenz", ManagingFunctions.HexToColor("#25FF00"), 300, 100f, 90f, -90f, -4f) { parent = planets[0] });
            planets.Add(new PlanetData("Dua", ManagingFunctions.HexToColor("#04CAD1"), 140, 10f, 10f, 0, -105f) { parent = planets[1] });
            planets.Add(new PlanetData("Intersection", ManagingFunctions.HexToColor("#EBD33D"), 250, 20f, 15f, 0, -80f) { parent = planets[1] });
            planets.Add(new PlanetData("Lurp", ManagingFunctions.HexToColor("#878787"), 70, 20f, 20f, 0, 96f) { parent = planets[0] });
            planets.Add(new PlanetData("Nheo", ManagingFunctions.HexToColor("#FFFE00"), 235, 80f, 30f, -90f, 3f) { parent = planets[0] });
            planets.Add(new PlanetData("Krylo", ManagingFunctions.HexToColor("#252525"), 564, 380f, 120f, 76f, -24f) { parent = planets[0] });

            foreach(PlanetData planet in planets)
            {
                planet.oTime = Random.Range(0, planet.Apoapsis * planet.Periapsis);
                planet.rTime = Random.Range(0, planet.revolutionTime);
            }

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
            newPlanet.GetChild(1).GetComponent<Button>().onClick.AddListener(() => FocusPlanet(idx));

            planet.ApplyToButton(newPlanet);

            planet.Recalculate();
            planet.CalculateOrbitRender(75);

            if (planet.planetName == "Sun")
            {
                newPlanet.GetChild(1).GetChild(1).GetComponent<Image>().sprite = starSprite;
            }

            if(planet.planetName == GameManager.gameManagerReference.currentPlanetName)
            {
                currentPlanet = planet;
            }
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

        int previousSize = 0;
        for (int i = 0; i < timewarpPanel.childCount; i++)
        {
            if (i != timewarpPanel.childCount - 1)
            {
                int size = System.Convert.ToInt32(timewarpPanel.GetChild(i).name);

                float colorValue = (timewarp - previousSize) / (size - previousSize);

                timewarpPanel.GetChild(i).GetComponent<Image>().color = new Color(0, colorValue, 0, colorValue);

                previousSize = size;
            }
            else
            {
                timewarpPanel.GetChild(i).GetComponent<Text>().text = "x" + Mathf.Floor(timewarp);
            }
        }

        planetPanelPropertiesRectTransform.gameObject.SetActive(!planetSelectionFocused);
    }

    public void CalibratePlanetRotation()
    {
        currentPlanet.rTime = (-ManagingFunctions.PointToPivotUp(currentPlanet.FindPoint(currentPlanet.oTime), Vector2.zero)) / 360f * currentPlanet.revolutionTime + (currentPlanet.revolutionTime * (GameManager.gameManagerReference.player.transform.position.x / (GameManager.gameManagerReference.WorldWidth * 16)));
    }

    public void Simulate(float stepTime, bool drawOrbits)
    {
        if (GameManager.gameManagerReference.isNetworkHost) NetworkController.networkController.UpdateTime(stepTime, timewarp);

        RectTransform viewport = planetPanelRectTransform.GetChild(0).GetComponent<RectTransform>();

        if (gameObject.activeInHierarchy)
            zoom += GInput.Zoom * 5;
        zoom = Mathf.Clamp(zoom, 2.4f, 100);
        viewport.sizeDelta = Vector2.Lerp(viewport.sizeDelta, Vector2.one * zoom, Time.deltaTime * 10);

        foreach (PlanetData planet in planets)
        {
            planet.Step(stepTime);
            Vector2 point = planet.FindPoint(planet.oTime);
            RectTransform planetObject = viewport.GetChild(planets.IndexOf(planet)).GetComponent<RectTransform>();
            UILineRenderer orbit = viewport.GetChild(planets.IndexOf(planet)).GetChild(0).GetComponent<UILineRenderer>();
            if (drawOrbits)
            {
                orbit.points = (Vector2[])planet.puntos_de_orbita_dos_puntos_D.Clone();
                orbit.color = Color.white * (100 - viewport.sizeDelta.x) / 100;
                orbit.thickness = viewport.sizeDelta.x / 4;
            }

            planetObject.sizeDelta = viewport.sizeDelta * (planet.chunkSize / 120f);

            if (planet.revolutionTime != 0)
                planetObject.GetChild(1).GetChild(1).GetComponent<RectTransform>().eulerAngles = Vector3.forward * -((float)planet.rTime % planet.revolutionTime / planet.revolutionTime * 360);

            if (planet.parent != null)
            {
                Vector2 parentPosition = viewport.GetChild(planets.IndexOf(planet.parent)).GetComponent<RectTransform>().anchoredPosition;

                planetObject.anchoredPosition = (point * viewport.sizeDelta.x) + parentPosition;

                if (drawOrbits)
                    for (int i = 0; i < orbit.points.Length; i++)
                    {
                        orbit.points[i] = orbit.points[i] * viewport.sizeDelta.x - (planetObject.anchoredPosition - parentPosition);
                    }
            }
            else
            {
                if (drawOrbits)
                    for (int i = 0; i < orbit.points.Length; i++)
                    {
                        orbit.points[i] = orbit.points[i] * viewport.sizeDelta.x - planetObject.anchoredPosition;
                    }
            }

            orbit.SetAllDirty();

            if (planet.planetName == GameManager.gameManagerReference.currentPlanetName)
            {
                viewport.anchoredPosition = (-planetObject.anchoredPosition);

                if (targetResourceLauncher != null)
                {
                    planetObject.GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(true);
                    planetObject.GetChild(1).GetChild(1).GetChild(0).localEulerAngles = Vector3.forward * (targetResourceLauncher.transform.position.x / (GameManager.gameManagerReference.WorldWidth * 16) * 360);
                    planetObject.GetChild(1).GetChild(1).GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta = viewport.sizeDelta * (planet.chunkSize / 840f);
                }
            }
            else
            {
                planetObject.GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(false);
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

        planetDataRectTransform.GetChild(0).GetComponent<Image>().color = planets[idx].planetColor.GetColor();
        planetDataRectTransform.GetChild(1).GetComponent<Text>().text = planets[idx].planetName;

        Color.RGBToHSV(planets[idx].planetColor.GetColor(), out float h, out float s, out float v);
        if(v > 0.5f)
            planetDataRectTransform.GetChild(1).GetComponent<Text>().color = planets[idx].planetColor.GetColor();
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

        MenuController.menuController.PlanetaryTravel(planets[planetFocused], resources, targetResourceLauncher, GameManager.gameManagerReference.tiles[subPanel.GetComponent<PackupMenuController>().currentCore], subPanel.GetComponent<PackupMenuController>().currentCore);
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
        }
    }
}

[JsonObject(IsReference = true)]
[System.Serializable]
public class PlanetData
{
    public string planetName;
    public string planetNickname = "";
    public SerializableColor planetColor = new SerializableColor();
    public int chunkSize;
    public string wordSize;
    [SerializeField] private float apoapsis = Random.Range(3f, 6f);
    [SerializeField] private float periapsis = Random.Range(1f, 3f);
    public float rotation = 0;
    public float revolutionTime = 4f;
    public double oTime;
    public double rTime;
    public bool canGo = true;
    public PlanetData parent;

    public float orbitalPeriod;
    [System.NonSerialized()] public RectTransform physicalPlanet;
    [System.NonSerialized()] public Vector2[] puntos_de_orbita_dos_puntos_D;

    public float Apoapsis
    {
        get { return apoapsis; }
        set 
        {
            apoapsis = value;
            Recalculate();
            CalculateOrbitRender(75);
        }
    }

    public float Periapsis
    {
        get { return periapsis; }
        set
        {
            periapsis = value;
            Recalculate();
            CalculateOrbitRender(75);
        }
    }

    public float semiMajorAxis = 0;
    public float eccentricity = 0;
    public float meanMotion = 0;


    public PlanetData(string name, Color color, int sizeInChunks, float apo, float per, float rot = 0, float rev = 4f)
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
        revolutionTime = rev;
    }

    public string ColorToHex()
    {
        return ColorUtility.ToHtmlStringRGB(planetColor.GetColor());
    }

    public void Step(float time)
    {
        if (orbitalPeriod > 0)
        {
            oTime += time;
            rTime += time;
            //rTime = (-ManagingFunctions.PointToPivotUp(FindPoint(oTime), Vector2.zero)) / 360f * revolutionTime;

            if (oTime > orbitalPeriod)
                oTime = oTime % orbitalPeriod;

            if (rTime > revolutionTime)
                rTime = rTime % revolutionTime;
        }
    }

    public void ApplyToButton(RectTransform rectTransform)
    {
        physicalPlanet = rectTransform;
        Color.RGBToHSV(planetColor.GetColor(), out float h, out float s, out float v);
        rectTransform.GetChild(1).GetChild(1).GetComponent<Image>().color = planetColor.GetColor();
        rectTransform.GetChild(1).GetComponent<Button>().interactable = canGo;

        rectTransform.GetChild(1).GetChild(0).GetComponent<Text>().text = planetName;

        if (v > 0.4f)
            rectTransform.GetChild(1).GetChild(0).GetComponent<Text>().color = planetColor.GetColor();
        else
            rectTransform.GetChild(1).GetChild(0).GetComponent<Text>().color = Color.white;


    }

    public void CalculateOrbitRender(int points)
    {
        if (apoapsis > 0 && periapsis > 0)
        {
            List<Vector2> pointList = new List<Vector2>();

            for (int i = 0; i < points; i++)
            {
                float time = (orbitalPeriod / points) * i;
                pointList.Add(FindPoint(time));
            }

            pointList.Add(FindPoint(0));
            pointList.Add(FindPoint(orbitalPeriod / points));

            puntos_de_orbita_dos_puntos_D = pointList.ToArray();
        }
        else
        {
            puntos_de_orbita_dos_puntos_D = new Vector2[0];
        }


    }

    public void Recalculate()
    {
        if (apoapsis > 0 && periapsis > 0)
        {
            semiMajorAxis = (apoapsis + periapsis) / 2;
            eccentricity = (apoapsis - periapsis) / (apoapsis + periapsis);
            meanMotion = Mathf.Sqrt(1 / (semiMajorAxis * semiMajorAxis * semiMajorAxis));

            orbitalPeriod = Mathf.Sqrt(Mathf.PI * Mathf.PI * 4 * Mathf.Pow(semiMajorAxis, 3));
        }
        else
        {
            semiMajorAxis = 0;
            eccentricity = 0;
            meanMotion = 0;

            orbitalPeriod = 0;
        }
    }

    public Vector2 FindPoint(double time)
    {
        if (apoapsis == 0 && periapsis == 0)
        {
            return Vector2.zero;
        }
        else
        {
            double meanAnomaly = meanMotion * time;

            double eccentricAnomaly = meanAnomaly;
            for (int i = 0; i < 5; i++)
            {
                eccentricAnomaly = meanAnomaly + eccentricity * Mathf.Sin((float)eccentricAnomaly);
            }

            float x = semiMajorAxis * (Mathf.Cos((float)eccentricAnomaly) - eccentricity);
            float y = semiMajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity) * -Mathf.Sin((float)eccentricAnomaly);

            Vector2 point = new Vector2(-y, -x);

            if (rotation != 0)
            {
                point = Quaternion.AngleAxis(rotation, Vector3.forward) * point;
            }

            return point;
        }
    }
}
