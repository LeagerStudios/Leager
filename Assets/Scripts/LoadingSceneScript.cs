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
    [SerializeField] RectTransform load;
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
            craftingT2DefaultConfig = DataSaver.ReadTxt(Application.dataPath + @"/packages/LeagerStudios/craftsAdvTech.txt");

            reloadDefault = false;
        }
    }

    void Start()
    {
        isLoading = true;
        Debug.Log("===STARTED INITIAL LOADING===");

        DontDestroyOnLoad(canvas.gameObject);
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
        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/packages/LeagerStudios/craftsAdvTech.txt"))
        {
            DataSaver.CreateTxt(Application.persistentDataPath + @"/packages/LeagerStudios/craftsAdvTech.txt", new string[] { });
        }

        {
            if (Application.isEditor)
            {
                DataSaver.CopyPasteTxt(Application.dataPath + @"/packages/LeagerStudios/crafts.txt", Application.persistentDataPath + @"/packages/LeagerStudios/crafts.txt");
                DataSaver.CopyPasteTxt(Application.dataPath + @"/packages/LeagerStudios/craftsAdvTech.txt", Application.persistentDataPath + @"/packages/LeagerStudios/craftsAdvTech.txt");
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

                if (DataSaver.CheckIfFileExists(Application.dataPath + @"/packages/LeagerStudios/craftsAdvTech.txt"))
                {
                    DataSaver.CopyPasteTxt(Application.dataPath + @"/packages/LeagerStudios/craftsAdvTech.txt", Application.persistentDataPath + @"/packages/LeagerStudios/craftsAdvTech.txt");
                }
                else
                {
                    loadingTxt.GetComponent<Text>().text = Application.dataPath + @"/packages/LeagerStudios/craftsAdvTech.txt is missing...";
                    StartCoroutine(Freeze());
                    return;
                }

            }
            else
            {

                DataSaver.ModifyTxt(Application.persistentDataPath + @"/packages/LeagerStudios/crafts.txt", craftingT1DefaultConfig);
                DataSaver.ModifyTxt(Application.persistentDataPath + @"/packages/LeagerStudios/craftsAdvTech.txt", craftingT2DefaultConfig);
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
        loadingBar.GetComponent<RectTransform>().localScale = new Vector2(0f, 1f);

        if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/data/version.txt"))
        {
            DataSaver.CreateTxt(Application.persistentDataPath + @"/data/version.txt", new string[] { version });
        }
        else DataSaver.ModifyTxt(Application.persistentDataPath + @"/data/version.txt", new string[] { version });

        yield return new WaitForSeconds(1f);

        string[] randomMessage = {
            "The Loading Time is Long, Isn't It?",
            "Loading, loading, loading...",
            "Waiting for the Loadingbar to Reach the End",
            "Writing stupid Phrases",
            "Undoing Stuff",
            "We are on " + Application.version + ", Right?",
            "You Don't Know How to Play? Read the Instructions.",
            ":)",
            "JifteDev is Life",
            "Go to Eat a Waffle While this Loads",
            "Creating the Creator",
            "Waiting for a Good Idea",
            "Go to The Sur and Craft a Core",
            "Get Ready to Play",
            "Programming Useless Mechanics",
            "Programming Useful Mechanics Nobody will Use",
            "Miler is slow",
            "[something has to be put in here]",
            "12 + 1, Everything matches.",
            "Enemies are not your allies",
            "Doors don't close unless you close them.",
            "The Krotek is Behind you",
            "Exploring the entire world",
            "añañañaño",
            "Changing from LoadScene.unity to MainMenu.unity",
            "Nobody talks about how good waffles are",
            "Razor is taking a Rocket to Space",
            "Nodes... AHHHHH",
            "WOW! LOOK! Is that a fortress? Oh... no, its not.",
            ":(",
            "miler sucks",
            "Just wait...",
            "DROP! WAW WAWAWAWAWAWA",
            "Some load messages are just... Load messages, I guess.",
            "LoadingSceneScript.cs is not scripting",
            "Work? Nah. Jetpack video.",
            "Hey! If the game runs slow, isn't my fault ok?",
            "You should try a DavidDEV game... wait... he doesn't have one...",
            "Testing gamer skillz",
            "These will be the best graphics you'll ever see",
            "Fun Fact: There aren't Fun Facts"
        };


        loadingTxt.GetComponent<Text>().text = randomMessage[Random.Range(0, randomMessage.Length)];
        Debug.Log("Load Text:" + loadingTxt.GetComponent<Text>().text);

        Scene scene = SceneManager.GetActiveScene();
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);

        while(asyncOperation.progress < 1f)
        {
            loadingBar.GetComponent<RectTransform>().localScale = new Vector3(Mathf.MoveTowards(loadingBar.GetComponent<RectTransform>().localScale.z, asyncOperation.progress / 2f, 0.6f * Time.deltaTime), 1f, 1f);
            yield return new WaitForEndOfFrame();
        }

        asyncOperation = SceneManager.UnloadSceneAsync(scene);

        while (asyncOperation.progress < 1f || loadingBar.GetComponent<RectTransform>().localScale.x < 1f)
        {
            loadingBar.GetComponent<RectTransform>().localScale = new Vector3(Mathf.MoveTowards(loadingBar.GetComponent<RectTransform>().localScale.z, asyncOperation.progress, 0.6f * Time.deltaTime), 1f, 1f);
            yield return new WaitForEndOfFrame();
        }



        yield return new WaitForSeconds(.1f);
        Debug.Log("===ENDED INITIAL LOADING===");

        Color color = loadingBarBg.GetComponent<Image>().color;
        color.a = 0f;
        loadingBarBg.GetComponent<Image>().color = color;

        float speed = 0;
        while (load.anchoredPosition.y > -25)
        {
            speed -= 2 * Time.deltaTime;
            load.anchoredPosition = load.anchoredPosition + Vector2.up * speed;
            yield return new WaitForSeconds(0.016f);
        }

        isLoading = false;
        Destroy(canvas.gameObject);
    }
}
