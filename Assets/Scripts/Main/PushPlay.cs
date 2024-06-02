using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Audio;
using System.Linq;

public class PushPlay : MonoBehaviour
{
    public static PushPlay main;
    public AudioMixer gameAudio;
    [SerializeField] Slider loadingSlider;
    [SerializeField] Slider bgmSlider;
    //[SerializeField] GameObject loadingTexts;
    [SerializeField] public Button multiplayerButton;
    [SerializeField] NetworkController networkController;
    [SerializeField] GameObject transition;
    [SerializeField] AudioSource menuThemeAudio;
    bool gettingData = false;


    void Start()
    {
        GameManager.gameManagerReference = null;
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        transition.SetActive(true);
        main = this;
        if (!GameObject.Find("SaveObject"))
        {
            GameObject saveObject = Instantiate(new GameObject("NewObject"));
            saveObject.name = "SaveObject";
            DontDestroyOnLoad(saveObject);
            GameObject.Find("SaveObject").AddComponent<ComponetSaver>();
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>();
        }

        Debug.Log("Leager version: " + Application.version);

        CreateMainSaves();
        if (DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/settings/bgmvol.lgrsd"))
        {
            bgmSlider.value = System.Convert.ToSingle(DataSaver.ReadTxt(Application.persistentDataPath + @"/settings/bgmvol.lgrsd")[0]);
            BGMVol(bgmSlider.value);
        }
        else
            DataSaver.CreateTxt(Application.persistentDataPath + @"/settings/bgmvol.lgrsd", new string[] { System.Convert.ToString(1f) });

        menuThemeAudio.Play();

        GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", true);
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { "0", "0" }, "newWorldSize");
        if (Application.isBatchMode)
        {
            ServerConsole.serverConsole.StartConsole();
        }
    }

    private void Update()
    {
        if (!ServerConsole.serverConsole.awaiting)
        {
            ServerConsole.serverConsole.SendEmbed(new string[] { "Select world:" }.Concat(WorldPanelController.worldPanelController.listOfLoadedWorlds).ToArray());
        }
        else if(ServerConsole.serverConsole.GetOutput(out string output))
        {

        }
    }

    public void OpenSocialMedia(string link)
    {
        Application.OpenURL(link);
    }


    public void StartNewGame()
    {
        string worldName = WorldPanelController.worldPanelController.newWorldName.GetComponent<InputField>().text;
        if (worldName == "miler sucks")
        {
            if (Application.platform != RuntimePlatform.Android)
                multiplayerButton.interactable = true;
            Camera.main.GetComponent<MainCameraController>().Focus = "mainMenu";
        }
        else
            StartCoroutine(LoadNewWorld(1f));
    }


    public void StartExistentGame()
    {
        StartCoroutine(LoadExistentWorld(1f, WorldPanelController.worldIndex));
    }

    public void BGMVol(float vol)
    {
        float db = 20 * Mathf.Log10(vol);
        gameAudio.SetFloat("BGM", db);

        DataSaver.ModifyTxt(Application.persistentDataPath + @"/settings/bgmvol.lgrsd", new string[] { System.Convert.ToString(vol) });
    }

    public void ConnectToExternalGame()
    {
        MultiplayerPanelController.nameInUse = false;

        string ip = WorldPanelController.worldPanelController.multiplayerIp.GetComponent<InputField>().text;
        int port = System.Convert.ToInt32(WorldPanelController.worldPanelController.multiplayerPort.GetComponent<InputField>().text);
        string user = WorldPanelController.worldPanelController.multiplayerUsername.GetComponent<InputField>().text;
        Debug.Log("Trying to connect to server: " + port);

        if (networkController.ConnectTo(ip, port, user))
            StartCoroutine(LoadNetworkWorld(1f, port));
    }

    public void CreateMainSaves()
    {
        if (! DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds"))
        {
            DataSaver.CreateFolder(Application.persistentDataPath + @"/worlds");
        }
        if(!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/settings"))
        {
            DataSaver.CreateFolder(Application.persistentDataPath + @"/settings");
        }
        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worldsData.lgrsd"))
        {
            DataSaver.SaveStats(new string[0], Application.persistentDataPath + @"/worldsData.lgrsd");
        }
        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/screenshots"))
        {
            DataSaver.CreateFolder(Application.persistentDataPath + @"/screenshots");
        }
        

        //SETTINGS FILES
        {
            if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/settings/minimap.lgrsd"))
            {
                DataSaver.SaveStats(new string[] { "on" }, Application.persistentDataPath + @"/settings/minimap.lgrsd");
            }

            if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/settings/fps.lgrsd"))
            {
                DataSaver.SaveStats(new string[] { "off" }, Application.persistentDataPath + @"/settings/fps.lgrsd");
            }

            if(!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/settings/lightstyle.lgrsd"))
            {
                DataSaver.SaveStats(new string[] { "1" }, Application.persistentDataPath + @"/settings/lightstyle.lgrsd");
            }
            if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/settings/vsync.lgrsd"))
            {
                DataSaver.SaveStats(new string[] { "true" }, Application.persistentDataPath + @"/settings/vsync.lgrsd");
            }
        }

    }

    IEnumerator LoadNewWorld(float secs)
    {
        WorldPanelController worldPanel = WorldPanelController.worldPanelController;

        string worldName = worldPanel.newWorldName.GetComponent<InputField>().text;

        worldPanel.newWorldName.GetComponent<InputField>().interactable = false;
        worldPanel.newWorldSeed.GetComponent<InputField>().interactable = false;
        worldPanel.newWorldButton.GetComponent<Button>().interactable = false;

        GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", false);
        yield return new WaitForSeconds(secs);


        List<string> newWorldsData = new List<string>(DataSaver.LoadStats(Application.persistentDataPath + @"/worldsData.lgrsd").SavedData) { worldName };

        DataSaver.SaveStats(newWorldsData.ToArray(), Application.persistentDataPath + @"/worldsData.lgrsd");


        string[] loadType = new string[] { "newWorld" };
        string[] worldLoadName = new string[] { worldName };
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { "false" }, "isNetworkLoad");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(loadType, "worldLoadType");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(worldLoadName, "worldName");
        if (worldPanel.newWorldSeed.GetComponent<InputField>().text != null)
        {
            string[] seed = { worldPanel.newWorldSeed.GetComponent<InputField>().text };
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(seed, "seed");
        }

        DataSaver.CreateFolder(Application.persistentDataPath + @"/worlds/" + worldName);

        StartCoroutine(LoadGame());
    }

    IEnumerator LoadExistentWorld(float secs, int worldIdx)
    {
        WorldPanelController worldPanel = WorldPanelController.worldPanelController.GetComponent<WorldPanelController>();
        worldPanel.newWorldName.GetComponent<InputField>().interactable = false;
        string worldName = worldPanel.listOfLoadedWorlds[worldPanel.dropdown.GetComponent<Dropdown>().value];

        string planetName = "null";
        if(DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + worldName + @"/lastLocation.lgrsd"))
        {
            planetName = DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/lastLocation.lgrsd").SavedData[0];
        }

        if (planetName == "null" || planetName == "Korenz")
        {
            string[] loadType = new string[] { "existentWorld" };
            string[] worldLoadName = new string[] { worldName };
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { "false" }, "isNetworkLoad");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(loadType, "worldLoadType");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(worldLoadName, "worldName");

            GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", false);
            yield return new WaitForSeconds(secs);
        }
        else
        {
            string[] loadType = new string[] { "existingPlanet" };
            string[] worldLoadName = new string[] { worldName + "/" + planetName };
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { "false" }, "isNetworkLoad");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(loadType, "worldLoadType");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(worldLoadName, "worldName");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { planetName }, "planetName");

            GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", false);
            yield return new WaitForSeconds(secs);
        }

        StartCoroutine(LoadGame());
    }

    IEnumerator LoadNetworkWorld(float secs, int port)
    {
        WorldPanelController worldPanel = WorldPanelController.worldPanelController.GetComponent<WorldPanelController>();
        worldPanel.newWorldName.GetComponent<InputField>().interactable = false;

        string[] loadType = new string[] { "existentWorld" };
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { "true" }, "isNetworkLoad");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(loadType, "worldLoadType");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(new string[] { port + "" }, "worldName");

        GameObject.Find("Transition").GetComponent<Animator>().SetBool("Open", false);
        yield return new WaitForSeconds(secs);

        StartCoroutine(LoadGame());
    }

    public void SetWorldSize(int width)
    {
        int[] wp = { width, 160 };
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("newWorldSize");
        GameObject.Find("SaveObject").GetComponent<ComponetSaver>().SaveData(ManagingFunctions.ConvertIntToStringArray(wp), "newWorldSize");
    }

    public void Tutorial(string language)
    {
        
    }

    public void OpenSaveDataFolder()
    {
        System.Diagnostics.Process.Start(Application.persistentDataPath);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public IEnumerator LoadGame()
    {
        loadingSlider.gameObject.SetActive(true);
        //loadingTexts.gameObject.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync("Game");
        

        while (!operation.isDone)
        {
            loadingSlider.value = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.transform.GetChild(2).GetComponent<Text>().text = (int)(operation.progress * 100f) + "%";
            yield return  new WaitForEndOfFrame();
        }
    }

    void URLCallback(string data, string requestType)
    {
        Debug.Log(data + ", [" + requestType + "]");

        if (requestType == "joinServer")
        {
            if(data != "-1")
            {
                WorldPanelController.worldPanelController.multiplayerWorldButton.interactable = false;
                int port = System.Convert.ToInt32(WorldPanelController.worldPanelController.multiplayerPort.GetComponent<InputField>().text);
                //StartCoroutine(LoadNetworkWorld(1, port));
            }
        }
    }

    IEnumerator GetDataFromURL(string url, string requestType, List<IMultipartFormSection> post)
    {
        gettingData = true;
        string returnData = "";

        UnityWebRequest www = UnityWebRequest.Post(url, post);
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.LogWarning(www.error);
        }
        else
        {
            returnData = www.downloadHandler.text;
            URLCallback(returnData, requestType);
        }
        gettingData = false;
    }

    //OPTIONS METHODS
    public void EraseScreenshots()
    {
        DataSaver.DeleteFile(Application.persistentDataPath + @"/screenshots");
        DataSaver.CreateFolder(Application.persistentDataPath + @"/screenshots");
    }

}
