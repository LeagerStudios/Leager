using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using PlayerIOClient;

public class LoginController : MonoBehaviour {

    bool gettingData = false;
    public bool loggedIn = false;

    [SerializeField] InputField loginUserField;
    [SerializeField] InputField loginPassField;
    [SerializeField] InputField signinUserField;
    [SerializeField] InputField signinPassField;
    [SerializeField] InputField signinEmailField;
    [SerializeField] GameObject loginButton;
    [SerializeField] Button logOffButton;
    [SerializeField] GameObject loggedUser;
    [SerializeField] MainCameraController cameraController;

    void Start()
    {
        //RefreshLogin();
    }

    public void CreateUser()
    {
        PlayerIOClient.Client client = PlayerIO.Authenticate("leager-h3f0o1td70uyrsf0nnddw", "public", ManagingFunctions.CreateArgsSingle("userId", "default"), null);

        string username = signinUserField.text;
        string password = signinPassField.text;
        string email = signinEmailField.text;

        if(client.BigDB.Load("Users", username) == null)
        {
            DatabaseObject newUser = new DatabaseObject();
            newUser.Set("Username", username);
            newUser.Set("Email", email);
            newUser.Set("Password", password);

            client.BigDB.CreateObject("Users", username, newUser);
            Debug.Log("User " + username + " Created");
        }
        else
        {
            Debug.Log("User already exists!!!");
        }
       
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
        //loggedIn = DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/logginSettings.lgrsd");

        //PushPlay.main.multiplayerButton.interactable = false;

        //loggedUser.SetActive(loggedIn);
        //loginButton.SetActive(!loggedIn);
        //logOffButton.interactable = loggedIn;

        //if (loggedIn)
        //{
        //    string[] logginSession = DataSaver.LoadStats(Application.persistentDataPath + @"/logginSettings.lgrsd").SavedData;

        //    List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        //    {
        //        new MultipartFormDataSection("user", logginSession[0]),
        //        new MultipartFormDataSection("pass", logginSession[1])
        //    };

        //    StartCoroutine(GetDataFromURL("http://localhost:5559/game/database/login.php", "userLogin", formData));
        //}
    }

    public void LogOff()
    {
        DataSaver.DeleteFile(Application.persistentDataPath + @"/logginSettings.lgrsd");
        RefreshLogin();
        cameraController.ChangeFocus("mainMenu");
    }
}
