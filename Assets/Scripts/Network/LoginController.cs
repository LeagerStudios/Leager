using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoginController : MonoBehaviour {

    bool gettingData = false;
    public bool loggedIn = false;

    [SerializeField] InputField loginUserField;
    [SerializeField] InputField loginPassField;
    [SerializeField] InputField signinUserField;
    [SerializeField] InputField signinPassField;
    [SerializeField] InputField signinNickField;
    [SerializeField] GameObject loginButton;
    [SerializeField] Button logOffButton;
    [SerializeField] GameObject loggedUser;
    [SerializeField] MainCameraController cameraController;

    void Start()
    {
        RefreshLogin();
    }

    void Update()
    {
        if (loggedIn)
        {
            loggedUser.SetActive(cameraController.Focus == "mainMenu");
        }
    }

    void RefreshLogin()
    {
        loggedIn = DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/logginSettings.lgrsd");

        PushPlay.main.multiplayerButton.interactable = false;

        loggedUser.SetActive(loggedIn);
        loginButton.SetActive(!loggedIn);
        logOffButton.interactable = loggedIn;

        if (loggedIn)
        {
            string[] logginSession = DataSaver.LoadStats(Application.persistentDataPath + @"/logginSettings.lgrsd").SavedData;

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("user", logginSession[0]));
            formData.Add(new MultipartFormDataSection("pass", logginSession[1]));

            StartCoroutine(GetDataFromURL("http://localhost/game/database/login.php", "userLogin", formData));
        }
    }

    public void Login()
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("user", loginUserField.text));
        formData.Add(new MultipartFormDataSection("pass", loginPassField.text));

        StartCoroutine(GetDataFromURL("http://localhost/game/database/login.php", "login", formData));
    }

    public void LogOff()
    {
        DataSaver.DeleteFile(Application.persistentDataPath + @"/logginSettings.lgrsd");
        RefreshLogin();
        cameraController.ChangeFocus("mainMenu");
    }

    public void SignIn()
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("user", signinUserField.text));
        formData.Add(new MultipartFormDataSection("pass", signinPassField.text));

        if(signinNickField.text == "")
            formData.Add(new MultipartFormDataSection("nick", "null"));
        else
            formData.Add(new MultipartFormDataSection("nick", signinNickField.text));

        StartCoroutine(GetDataFromURL("http://localhost/game/database/signin.php", "signin", formData));
    }

    void URLCallback(string data, string requestType)
    {
        Debug.Log(data + ", [" + requestType + "]");

        if(requestType == "debug")
        {
            
        }

        if(requestType == "signin")
        {
            if (data != "2" && data != "-1")
            {
                DataSaver.SaveStats(new string[] { signinUserField.text, signinPassField.text, data }, Application.persistentDataPath + @"/logginSettings.lgrsd");
                RefreshLogin();
                PushPlay.main.multiplayerButton.interactable = true;
                cameraController.ChangeFocus("mainMenu");
            }
        }

        if (requestType == "login")
        {
            if (data != "1" && data != "-1")
            {
                DataSaver.SaveStats(new string[] { loginUserField.text, loginPassField.text, data }, Application.persistentDataPath + @"/logginSettings.lgrsd");
                RefreshLogin();
                PushPlay.main.multiplayerButton.interactable = true;
                cameraController.ChangeFocus("mainMenu");
            }
        }

        if (requestType == "userLogin")
        {
            if (data != "1" && data != "-1")
            {
                string[] logginSession = DataSaver.LoadStats(Application.persistentDataPath + @"/logginSettings.lgrsd").SavedData;

                loggedUser.transform.GetChild(0).GetComponent<Text>().text = logginSession[0];
                loggedUser.GetComponent<RectTransform>().sizeDelta = new Vector2(logginSession[0].Length * 14 + 6, 30);
                loggedUser.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(logginSession[0].Length * 14, 30);
                PushPlay.main.multiplayerButton.interactable = true;
            }
            else
            {
                DataSaver.DeleteFile(Application.persistentDataPath + @"/logginSettings.lgrsd");
                RefreshLogin();
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
}
