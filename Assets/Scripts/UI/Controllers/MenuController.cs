using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    public static MenuController menuController;
    public bool deployed = false;
    public bool UIActive = true;
    public bool miniMapOn = true;
    public bool fpsOn = false;
    public bool vSync = true;
    public int chunkLoadDistance;
    public int chunksOnEachSide;
    public bool devInterface = false;
    public Resolution[] resolutions;
    [SerializeField] GameObject menu;
    [SerializeField] public GameObject planetMenu;
    [SerializeField] public Canvas canvas;
    [SerializeField] public Text ChunkUpdateNotifier;
    [SerializeField] public Dropdown ChunkRenderDropdown;
    [SerializeField] public Slider ChunkUpdateSlider;
    [SerializeField] public Dropdown ResolutionDropdown;
    [SerializeField] public GameObject settingsMenu;
    [SerializeField] GameObject MiniMap;

    [SerializeField] GameObject MiniMapCamera;
    [SerializeField] GameObject MiniMapButton;
    [SerializeField] GameObject FpsButton;
    [SerializeField] Dropdown FPSDropdown;
    [SerializeField] GameObject FpsText;
    [SerializeField] GameObject VSyncButton;
    [SerializeField] GameObject LoadingPlanetScreen;
    [SerializeField] DeathScreenController deathScreenController;
    [SerializeField] InputField ipHost;
    [SerializeField] InputField portHost;
    [SerializeField] InputField usernameHost;

    GameManager gameManager;
    public Coroutine travelCoroutine;
    private void Awake()
    {
        menuController = this;
    }

    void Start () {
        if(GameObject.Find("SaveObject") == null)
        {
            GameManager.gameManagerReference = null;
            SceneManager.LoadScene("MainMenu");
        }

        menuController = this;
        deployed = false;
        ChunkRenderDropdown.RefreshShownValue();
        UpdateRenderChunkDistance(2);
        resolutions = Screen.resolutions;
        ResolutionDropdown.ClearOptions();
        SetResolutionDropdown();
        gameManager = GameManager.gameManagerReference;

        if (GInput.leagerInput.platform != "Mobile")
            if (DataSaver.LoadStats(Application.persistentDataPath + @"/settings/minimap.lgrsd").SavedData[0] == "on")
            {
                miniMapOn = true;
            }
            else
            {
                miniMapOn = false;
            }
        else miniMapOn = false;

        if (DataSaver.LoadStats(Application.persistentDataPath + @"/settings/fps.lgrsd").SavedData[0] == "on")
        {
            fpsOn = true;
        }
        else
        {
            fpsOn = false;
        }

        if (DataSaver.LoadStats(Application.persistentDataPath + @"/settings/vsync.lgrsd").SavedData[0] == "true")
        {
            vSync = true;
        }
        else
        {
            vSync = false;
        }
        QualitySettings.vSyncCount = System.Convert.ToInt32(vSync);

        ChunkUpdateNotifier.text = System.Convert.ToString(ChunkUpdateSlider.value);
        Application.targetFrameRate = 60;
    }

	void Update () {

        if (GInput.GetKeyDown(KeyCode.Escape))
        {
            if (deployed)
            {
                deployed = false;
                GameManager.gameManagerReference.InGame = true;
            }
            else if(gameManager.InGame)
            {
                deployed = true;
                GameManager.gameManagerReference.InGame = false;
            }
        }
        menu.SetActive(deployed);

        if (UIActive) canvas.targetDisplay = 0;
        else canvas.targetDisplay = 7;

        if (GInput.GetKeyDown(KeyCode.F3))
        {
            devInterface = !devInterface;
            Debug.Log("==DEV INTERFACE TRIGERED TO: " + devInterface + "==");
        }

        if (GInput.GetKeyDown(KeyCode.F11))
        {
            ToggleFullScreen();
        }

        if (devInterface)
        {
            
        }
        else
        {

        }

        if(GInput.GetKeyDown(KeyCode.F5))
        {
            UIActive = !UIActive;
            Debug.Log("==UI ACTIVE TRIGERED TO: " + UIActive + "==");
        }

        FpsText.SetActive(fpsOn);
        FpsText.GetComponent<Text>().text = Mathf.Floor(1.0f / Time.smoothDeltaTime * Time.timeScale) + "";

       

        MiniMap.SetActive(miniMapOn);
        MiniMapCamera.SetActive(miniMapOn);

        if (miniMapOn && MiniMapButton.activeInHierarchy)
        {
            MiniMapButton.GetComponent<Image>().color = Color.green;
            MiniMapButton.transform.GetChild(0).GetComponent<Text>().text = "On";
        }
        else
        {
            MiniMapButton.GetComponent<Image>().color = Color.red;
            MiniMapButton.transform.GetChild(0).GetComponent<Text>().text = "Off";
        }

        if (fpsOn && FpsButton.activeInHierarchy)
        {
            FpsButton.GetComponent<Image>().color = Color.green;
            FpsButton.transform.GetChild(0).GetComponent<Text>().text = "On";
        }
        else
        {
            FpsButton.GetComponent<Image>().color = Color.red;
            FpsButton.transform.GetChild(0).GetComponent<Text>().text = "Off";
        }

        if (vSync && VSyncButton.activeInHierarchy)
        {
            VSyncButton.GetComponent<Image>().color = Color.green;
            VSyncButton.transform.GetChild(0).GetComponent<Text>().text = "On";
        }
        else
        {
            VSyncButton.GetComponent<Image>().color = Color.red;
            VSyncButton.transform.GetChild(0).GetComponent<Text>().text = "Off";
        }


        if (GInput.leagerInput.platform == "Mobile")
        {
            MiniMapButton.GetComponent<Image>().color = Color.red;
            MiniMapButton.transform.GetChild(0).GetComponent<Text>().text = "Off";
            MiniMapButton.GetComponent<Button>().interactable = false;
            ResolutionDropdown.interactable = false;
        }
    }

    public void StartServer()
    {
        Server.Main(NetworkController.GetLocalIP(), System.Convert.ToInt32(portHost.text), "host");
        gameManager.isNetworkHost = true;
    }

    private void OnApplicationFocus(bool focus)
    {
        if(focus == false && !Application.isEditor)
        {
            deployed = true;
            gameManager.InGame = false;
        }
    }

    public void UpdateTileSpawnRate()
    {
        int val = (int)ChunkUpdateSlider.value;
        ChunkUpdateNotifier.text = val + "";
        gameManager.tileSpawnRate = val;
    }

    public void MenuDeploy()
    {
        if (!settingsMenu.activeInHierarchy && !planetMenu.activeInHierarchy)
        {
            if (deployed)
            {
                deployed = false;
            }
            else
            {
                deployed = true;
            }
        }
    }


    public void PlanetMenuDeploy(ResourceLauncher resourceLauncher)
    {
        if (gameManager.InGame && !planetMenu.GetComponent<PlanetMenuController>().subPanel.gameObject.activeInHierarchy)
        {
            gameManager.InGame = false;
            planetMenu.GetComponent<PlanetMenuController>().targetResourceLauncher = resourceLauncher;
            planetMenu.SetActive(true);
        }
    }

    public void PlanetaryTravel(PlanetData planet, string resources, ResourceLauncher from, Sprite core)
    {
        travelCoroutine = StartCoroutine(IEPlanetaryTravel(planet, resources, from, core));
    }

    public void CancelTravel()
    {
        if (travelCoroutine != null)
            StopCoroutine(travelCoroutine);
    }

    public IEnumerator IEPlanetaryTravel(PlanetData planet, string resources, ResourceLauncher from, Sprite core)
    {
        yield return new WaitForSeconds(1);
        Debug.Log("==GOING TO LOCATION: " + planet.planetName + " WITH " + resources + " RESOURCES");
        string state = "null";

        if ((DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + gameManager.worldRootName + "/" + planet.planetName) || planet.planetName == "Korenz") && core == gameManager.tiles[0])
        {
            state = "antenna";
        }
        else
        {
            state = "packagelaunch";
            UIActive = false;
        }

        try
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("worldEnterAnimation");
        }
        catch { Debug.Log("==SKIPPED DELETING WORLD ENTER ANIMATION=="); }

        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { state }, "worldEnterAnimation");

        from.TriggerAnimation(state, core);

        while (!from.animationPlayed)
        {
            yield return new WaitForSeconds(0.016f);
        }

        LoadingPlanetScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        GameManager.gameManagerReference.SaveGameData(false);

        if (planet.planetName == "Korenz")
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("worldLoadType");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("worldName");

            try
            {
                GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("newWorldSize");
                GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("seed");
            }
            catch { Debug.Log("==SKIPPED DELETING SEED AND SIZE FROM SAVEOBJECT=="); }

            try
            {
                GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("planetName");
                GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("planetColor");
            }
            catch { Debug.Log("==SKIPPED DELETING PLANET COLOR AND NAME FROM SAVEOBJECT=="); }

            try
            {
                GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("resources");
            }
            catch { Debug.Log("==SKIPPED DELETING RESOURCES FROM SAVEOBJECT=="); }

            if(resources != "null")
            {
                GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { resources }, "resources");
            }


            string[] loadType = new string[] { "existentWorld" };
            string[] worldLoadName = new string[] { gameManager.worldRootName };
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(loadType, "worldLoadType");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(worldLoadName, "worldName");

            SceneManager.LoadScene("Game");
        }
        else
        {
            if (DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + gameManager.worldRootName + "/" + planet.planetName))
            {
                LoadExistentPlanet(planet, resources);
            }
            else
            {
                LoadNewPlanet(planet, resources);
            }
        }
    }

    void LoadNewPlanet(PlanetData planet, string resources)
    {
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("worldName");
        try
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("newWorldSize");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("seed");
        }
        catch { Debug.Log("==SKIPPED DELETING SEED AND SIZE FROM SAVEOBJECT=="); }

        try
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("planetName");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("planetColor");
        }
        catch { Debug.Log("==SKIPPED DELETING PLANET COLOR AND NAME FROM SAVEOBJECT=="); }

        try
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("resources");
        }
        catch { Debug.Log("==SKIPPED DELETING RESOURCES FROM SAVEOBJECT=="); }

        if (resources != "null")
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { resources }, "resources");
        }

        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("worldLoadType");

        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { gameManager.worldRootName + "/" + planet.planetName }, "worldName");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { "newPlanet" }, "worldLoadType");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { planet.chunkSize + "", "160" }, "newWorldSize");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { planet.ColorToHex() }, "planetColor");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { planet.planetName }, "planetName");

        DataSaver.CreateFolder(Application.persistentDataPath + @"/worlds/" + gameManager.worldRootName + "/" + planet.planetName);


        SceneManager.LoadScene("Game");
    }

    void LoadExistentPlanet(PlanetData planet, string resources)
    {
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("worldName");
        try
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("newWorldSize");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("seed");
        }
        catch { Debug.Log("==SKIPPED DELETING SEED AND SIZE FROM SAVEOBJECT=="); }

        try
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("planetName");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("planetColor");
        }
        catch { Debug.Log("==SKIPPED DELETING PLANET COLOR AND NAME FROM SAVEOBJECT=="); }

        try
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("resources");
        }
        catch { Debug.Log("==SKIPPED DELETING RESOURCES FROM SAVEOBJECT=="); }

        if (resources != "null")
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { resources }, "resources");
        }

        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("worldLoadType");

        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { gameManager.worldRootName + "/" + planet.planetName }, "worldName");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { planet.ColorToHex() }, "planetColor");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { planet.planetName }, "planetName");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { "existingPlanet" }, "worldLoadType");


        SceneManager.LoadScene("Game");
    }

    public void Quit()
    {
        GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", false);
        StartCoroutine(ExitGame());
    }

    public void Respawn()
    {
        StartCoroutine(DeathRespawn(1.5f));
    }

    public IEnumerator DeathRespawn(float secs)
    {
        GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", false);
        yield return new WaitForSeconds(1);
        gameManager.player.Respawn((gameManager.WorldWidth * 16) / 2, gameManager.SearchForClearSpawn((gameManager.WorldWidth * 16) / 2) + 2);
        gameManager.UpdateChunksActive();
        yield return new WaitForSeconds(Mathf.Clamp(secs - 1, 0, 100));
        deathScreenController.ResetScreen();
        GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", true);
    }

    public void RefreshTransition()
    {
        StartCoroutine(RefTrans());
    }

    private IEnumerator RefTrans()
    {
        GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", false);
        yield return new WaitForSeconds(1.2f);
        GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", true);
    }

    IEnumerator ExitGame()
    {
        yield return new WaitForSeconds(1);
        GameManager.gameManagerReference.SaveGameData(false);
        if (gameManager.isNetworkHost)
        {
            Server.CloseServer();
            Destroy(NetworkController.networkController.gameObject);
        }
        if (gameManager.isNetworkClient)
        {
            Client.Disconnect();
            Destroy(NetworkController.networkController.gameObject);
        }
        Debug.Log("===EXITED TO MAIN MENU===");
        Destroy(GameObject.Find("SaveObject"));
        SceneManager.LoadScene("MainMenu");
        
    }



    public void Resume()
    {
        deployed = false;
    }

    public void UpdateRenderChunkDistance(int idx)
    {
        int[] list = { 3, 5, 10, 20, 40 };
        chunkLoadDistance = list[idx] * 20;
        chunksOnEachSide = list[idx];
        Debug.Log("(OPTIONS)==CHUNKS ACTIVATED SET TO: " + idx + ". EQUALS: " + list[idx] + ".==");
    }

    public void UpdateResolution(int resIdx)
    {
        Resolution resolution = resolutions[resIdx];
        if (Screen.fullScreen)
        {
            Screen.SetResolution(resolution.width, resolution.height, true);
        }
        else
        {
            Screen.SetResolution(resolution.width, resolution.height, false);
        }
    }

    public void ToggleFullScreen()
    {
        Screen.SetResolution(Screen.width, Screen.height, !Screen.fullScreen);
    }

    public void SetFPS(int idx)
    {
        string value = FPSDropdown.options[idx].text;
        Application.targetFrameRate = System.Convert.ToInt32(value);
    }

    public void SetResolutionDropdown()
    {
        List<string> options = new List<string>();
        int currentRes = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (i == resolutions.Length - 1)
            {
                currentRes = i;
            }
        }

        ResolutionDropdown.AddOptions(options);
        ResolutionDropdown.value = currentRes;
        ResolutionDropdown.RefreshShownValue();

    }

    public void ShowHideMiniMap()
    {
        if (miniMapOn)
        {
            miniMapOn = false;
            DataSaver.SaveStats(new string[] { "off" }, Application.persistentDataPath + @"/settings/minimap.lgrsd");
        }
        else
        {
            miniMapOn = true;
            DataSaver.SaveStats(new string[] { "on" }, Application.persistentDataPath + @"/settings/minimap.lgrsd");
        }
        LightController.lightController.AddRenderQueue(GameManager.gameManagerReference.player.transform.position);
    }

    public void ShowHideFps()
    {
        if (fpsOn)
        {
            fpsOn = false;
            DataSaver.SaveStats(new string[] { "off" }, Application.persistentDataPath + @"/settings/fps.lgrsd");
        }
        else
        {
            fpsOn = true;
            DataSaver.SaveStats(new string[] { "on" }, Application.persistentDataPath + @"/settings/fps.lgrsd");
        }
    }

    public void ActivateVSync()
    {
        vSync = !vSync;
        QualitySettings.vSyncCount = System.Convert.ToInt32(vSync);
        DataSaver.SaveStats(new string[] { fpsOn.ToString().ToLower() }, Application.persistentDataPath + @"/settings/vsync.lgrsd");
    }
}
