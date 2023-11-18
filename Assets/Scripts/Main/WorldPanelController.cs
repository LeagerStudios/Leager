using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldPanelController : MonoBehaviour {

    ComponetSaver saveObject;
    public static WorldPanelController worldPanelController;
    [SerializeField] public GameObject dropdown;
    [SerializeField] public GameObject playButton;
    [SerializeField] public GameObject newWorldName;
    [SerializeField] public GameObject newWorldSeed;
    [SerializeField] public GameObject newWorldButton;
    [SerializeField] public Button newWorldMenuButton;
    [SerializeField] public GameObject verySmallWorldButton;
    [SerializeField] GameObject AdvertText1;
    [SerializeField] GameObject AdvertText2;

    [SerializeField] public InputField multiplayerIp;
    [SerializeField] public InputField multiplayerPort;
    [SerializeField] public Button multiplayerWorldButton;

    public List<string> listOfLoadedWorlds;
    public bool networkWorldSelection;



    public static int worldIndex = 0;

    void Start() {
        worldPanelController = this;
        if (!GameObject.Find("SaveObject"))
        {
            GameObject saveObject = Instantiate(new GameObject("NewObject"));
            saveObject.name = "SaveObject";
            DontDestroyOnLoad(saveObject);
            GameObject.Find("SaveObject").AddComponent<ComponetSaver>();
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>();
        }


        GameObject.Find("MenuManager").GetComponent<PushPlay>().CreateMainSaves();
        saveObject = GameObject.Find("SaveObject").GetComponent<ComponetSaver>();
        Dropdown worldsDropdown = dropdown.GetComponent<Dropdown>();
        worldsDropdown.ClearOptions();

        RefreshWorlds();
    }

    void Update() {

        newWorldMenuButton.interactable = !networkWorldSelection;

        if (dropdown.GetComponent<Dropdown>().options.Count == 0)
        {
            dropdown.GetComponent<Dropdown>().interactable = false;
            playButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            dropdown.GetComponent<Dropdown>().interactable = true;
            playButton.GetComponent<Button>().interactable = true;
        }

        if (newWorldName.GetComponent<InputField>().text == "" ||
            (ManagingFunctions.ConvertStringToIntArray(saveObject.LoadData("newWorldSize"))[0] < 1 && ManagingFunctions.ConvertStringToIntArray(saveObject.LoadData("newWorldSize"))[1] < 1) ||
            GameObject.Find("Transition").GetComponent<Animator>().GetBool("Open") == false ||
            listOfLoadedWorlds.Contains(newWorldName.GetComponent<InputField>().text) || !CheckWorldChars(newWorldName.GetComponent<InputField>().text))
        {
            newWorldButton.GetComponent<Button>().interactable = false;
            if (listOfLoadedWorlds.Contains(newWorldName.GetComponent<InputField>().text))
            {
                AdvertText1.SetActive(true);
            }
            else
            {
                AdvertText1.SetActive(false);
            }

            if (!CheckWorldChars(newWorldName.GetComponent<InputField>().text))
            {
                AdvertText2.SetActive(true);
            }
            else
            {
                AdvertText2.SetActive(false);
            }
        }
        else
        {
            newWorldButton.GetComponent<Button>().interactable = true;
            AdvertText1.SetActive(false);
            AdvertText2.SetActive(false);
        }
        verySmallWorldButton.SetActive(Application.isEditor);

    }

    public void SetNetworkSelection()
    {
        networkWorldSelection = true;
    }

    public void StartGame()
    {
        if (!networkWorldSelection)
        {
            PushPlay.main.StartExistentGame();
        }
    }

    public void RefreshWorlds()
    {
        Dropdown worldsDropdown = dropdown.GetComponent<Dropdown>();
        worldsDropdown.ClearOptions();
        string[] allWorlds = DataSaver.LoadStats(Application.persistentDataPath + @"/worldsData.lgrsd").SavedData;

        List<string> listOfWorlds = new List<string>(allWorlds);

        worldsDropdown.AddOptions(listOfWorlds);
        worldsDropdown.value = 0;
        worldsDropdown.RefreshShownValue();

        listOfLoadedWorlds = listOfWorlds;
    }

    public void MoveToFirst()
    {
        List<string> newWorldsData = new List<string>(DataSaver.LoadStats(Application.persistentDataPath + @"/worldsData.lgrsd").SavedData);
        string worldName = listOfLoadedWorlds[dropdown.GetComponent<Dropdown>().value];
        newWorldsData.Remove(worldName);
        newWorldsData.Insert(0, worldName);

        DataSaver.SaveStats(newWorldsData.ToArray(), Application.persistentDataPath + @"/worldsData.lgrsd");
        RefreshWorlds();
    }

    public void ChangeWorldIndex(int newIdx)
    {
        worldIndex = newIdx;
    }

    public void EraseWorld()
    {
        string worldName = listOfLoadedWorlds[dropdown.GetComponent<Dropdown>().value];

        List<string> newWorldsData = new List<string>(DataSaver.LoadStats(Application.persistentDataPath + @"/worldsData.lgrsd").SavedData);
        newWorldsData.Remove(worldName);
        DataSaver.SaveStats(newWorldsData.ToArray(), Application.persistentDataPath + @"/worldsData.lgrsd");
        RefreshWorlds();
        DataSaver.DeleteFile(Application.persistentDataPath + @"/worlds/" + worldName);
    }

    private bool CheckWorldChars(string name)
    {
        List<char> characters = new List<char>(new char[] { ' ', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'ñ', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'Ñ', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

        foreach(char c in name)
        {
            if (!characters.Contains(c))
            {
                return false;
            }
        }

        return true;
    }

  
}
