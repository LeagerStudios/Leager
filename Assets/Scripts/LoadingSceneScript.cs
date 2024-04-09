using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneScript : MonoBehaviour 
{
    [SerializeField] GameObject loadingBar;
    [SerializeField] GameObject loadingBarBg;
    [SerializeField] GameObject loadingTxt;
    [SerializeField] RectTransform canvas;
    [SerializeField] string[] craftingT1DefaultConfig;
    [SerializeField] string[] craftingT2DefaultConfig;
    [SerializeField] string[] craftingT3DefaultConfig;
    bool isLoading = true;
    public bool reloadDefault = false;

    private void OnValidate()
    {
        if (reloadDefault == true)
        {
            craftingT1DefaultConfig = DataSaver.ReadTxt(Application.dataPath + @"/packages/LeagerStudios/crafts.txt");

            reloadDefault = false;
        }
    }

    void Start()
    {
        isLoading = true;
        Debug.Log("===STARTED INITIAL LOADING===");

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/data"))
        {
            DataSaver.CreateFolder(Application.persistentDataPath + @"/data");
        }
        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/packages"))
        {
            DataSaver.CreateFolder(Application.persistentDataPath + @"/packages");
            DataSaver.CreateFolder(Application.persistentDataPath + @"/packages/LeagerStudios");
        }
        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/packages/LeagerStudios/crafts.txt"))
        {
            DataSaver.CreateTxt(Application.persistentDataPath + @"/packages/LeagerStudios/crafts.txt", new string[] { });
        }

        {
            if (Application.isEditor)
            {
                DataSaver.CopyPasteTxt(Application.dataPath + @"/packages/LeagerStudios/crafts.txt", Application.persistentDataPath + @"/packages/LeagerStudios/crafts.txt");
            }
            else if ((Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer) && !Debug.isDebugBuild)
            {
                if (!DataSaver.CheckIfFileExists(Application.dataPath + @"/packages"))
                {
                    loadingTxt.GetComponent<Text>().text = Application.dataPath + @"/packages is missing...";
                    StartCoroutine(Freeze());
                    return;
                }
                if (!DataSaver.CheckIfFileExists(Application.dataPath + @"/packages/LeagerStudios"))
                {
                    loadingTxt.GetComponent<Text>().text = Application.dataPath + @"/packages/LeagerStudios is missing...";
                    StartCoroutine(Freeze());
                    return;
                }
                if (DataSaver.CheckIfFileExists(Application.dataPath + @"/packages/LeagerStudios/crafts.txt"))
                {
                    DataSaver.CopyPasteTxt(Application.dataPath + @"/packages/LeagerStudios/crafts.txt", Application.persistentDataPath + @"/packages/LeagerStudios/crafts.txt");
                }
                else
                {
                    loadingTxt.GetComponent<Text>().text = Application.dataPath + @"/packages/LeagerStudios/crafts.txt is missing...";
                    StartCoroutine(Freeze());
                    return;
                }


            }
            else
            {

                DataSaver.ModifyTxt(Application.persistentDataPath + @"/packages/LeagerStudios/crafts.txt", craftingT1DefaultConfig);
            }
            StartCoroutine(TasksBeforeStart(DataSaver.version));
        }


        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/data/loginlog.txt"))
        {
            DataSaver.CreateTxt(Application.persistentDataPath + @"/data/loginlog.txt", new string[] { "Log:", "", "" });
            DataSaver.AddLineToTxt(Application.persistentDataPath + @"/data/loginlog.txt", "!FIRST LOGIN!: ");
            DataSaver.AddLineToTxt(Application.persistentDataPath + @"/data/loginlog.txt", System.Convert.ToString(System.DateTime.Now) + " (" + DataSaver.version + ")" + " (editor:" + Application.isEditor + ")");
            DataSaver.AddLineToTxt(Application.persistentDataPath + @"/data/loginlog.txt", "Total Logins: 1");
        }
        else
        {
            DataSaver.DeleteLastLineInTxt(Application.persistentDataPath + @"/data/loginlog.txt");
            DataSaver.AddLineToTxt(Application.persistentDataPath + @"/data/loginlog.txt", System.Convert.ToString(System.DateTime.Now) + " (" + DataSaver.version + ")" + " (editor:" + Application.isEditor + ")");
            DataSaver.AddLineToTxt(Application.persistentDataPath + @"/data/loginlog.txt", "Total Logins: " + (DataSaver.LinesInTxt(Application.persistentDataPath + @"/data/loginlog.txt") - 4));
        }
    } 

    IEnumerator Freeze()
    {
        yield return new WaitForSecondsRealtime(7.5f);
        Application.Quit();
    }

    IEnumerator TasksBeforeStart(string version)
    {

        for (int i = 0; i < 10; i++)
        {
            loadingBar.GetComponent<RectTransform>().localScale = new Vector2(loadingBar.GetComponent<RectTransform>().localScale.x - 0.1f, 1f);
            yield return new WaitForSeconds(0.016f);
        }

        loadingBar.GetComponent<RectTransform>().localScale = new Vector2(0f, 1f);

        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/data/version.txt"))
        {
            DataSaver.CreateTxt(Application.persistentDataPath + @"/data/version.txt", new string[] { version });
        }
        else DataSaver.ModifyTxt(Application.persistentDataPath + @"/data/version.txt", new string[] { version });

        yield return new WaitForSeconds(1f);

        string[] randomMessage = { "The Loading Time is Long, Isn't It?", "Loading, loading, loading...", "Waiting for the Loadingbar to Reach the End", "Writing stupid Phrases", "Undoing Stuff", Application.version + ", Cool version, Right?", "You Don't Know How to Play? Read the Instructions.", ":)", "JifteDev is Life", "Go to Eat a Waffle While this Loads", "Creating the Creator", "Waiting a Good Idea", "Go to The Sur and Craft a Core", "Get Ready to Play", "Programming Useless Mechanics", "Miler is slow", "[something has to be put in here]", "12 + 1, Everything matches.", "Enemies are not your allies", "Doors don't close unless you close them.", "The Krotek is Behind you", "Exploring the entire world", "añañañaño" };


        loadingTxt.GetComponent<Text>().text = randomMessage[Random.Range(0, randomMessage.Length)];
        Debug.Log("Load Text:" + loadingTxt.GetComponent<Text>().text);

        for (int i = 0; i < 10; i++)
        {
            for (int i2 = 0; i2 < 10; i2++)
            {
                loadingBar.GetComponent<RectTransform>().localScale = new Vector2(loadingBar.GetComponent<RectTransform>().localScale.x + 0.01f, 1f);
                yield return new WaitForSeconds(0.016f);
            }
        }

        yield return new WaitForSeconds(.5f);

        StartCoroutine(ExitLoadMenu());
    }

    IEnumerator ExitLoadMenu()
    {
        Color color = loadingBarBg.GetComponent<Image>().color;
        color.a = 0f;
        loadingBarBg.GetComponent<Image>().color = color;

        for (int i = 0; i < 50; i++)
        {
            color = loadingBar.GetComponent<Image>().color;
            color.a -= 0.02f;
            loadingBar.GetComponent<Image>().color = color;

            color = loadingTxt.GetComponent<Text>().color;
            color.a -= 0.02f;
            loadingTxt.GetComponent<Text>().color = color;
            yield return new WaitForSeconds(0.016f);
        }

        Debug.Log("===ENDED INITIAL LOADING===");

        isLoading = false;

        SceneManager.LoadScene("MainMenu");
    }
}
