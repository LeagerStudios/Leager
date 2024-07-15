using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using PlayerIOClient;
using System.Text;
using System.Security.Cryptography;

public class LoginController : MonoBehaviour {

    bool gettingData = false;
    public bool loggedIn = false;

    [SerializeField] InputField loginEmailField;
    [SerializeField] InputField loginPassField;
    [SerializeField] InputField signinUserField;
    [SerializeField] InputField signinPassField;
    [SerializeField] InputField signinEmailField;
    [SerializeField] GameObject loginButton;
    [SerializeField] Button logOffButton;
    [SerializeField] GameObject loggedUser;
    [SerializeField] MainCameraController cameraController;

    [SerializeField] string serverID;
    [SerializeField] string message;

    void Start()
    {
        //RefreshLogin();
    }

    public bool CreateUser()
    {
        string username = signinUserField.text;
        string password = signinPassField.text;
        string email = signinEmailField.text;

        string data = CreateHead("default", message);
        PlayerIOClient.Client client = PlayerIO.Authenticate(serverID, "userreq", ManagingFunctions.CreateArgs(new string[] { "userId", "auth" }, new string[] { "default", data }), null);

        if(client.BigDB.Load("Users", email) == null)
        {
            DatabaseObject newUser = new DatabaseObject();
            newUser.Set("Username", username);
            newUser.Set("Email", email);
            newUser.Set("Password", password);

            client.BigDB.CreateObject("Users", email, newUser);
            return true;
        }
        else
        {
            return false;
        }
       
    }

    void Update()
    {
        if (loggedIn)
        {
            loggedUser.SetActive(cameraController.Focus == "mainMenu");
        }
        else
        {
            loginButton.SetActive(cameraController.Focus == "mainMenu");
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

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("user", logginSession[0]),
                new MultipartFormDataSection("pass", logginSession[1])
            };

           
        }
    }

    public void LogOff()
    {
        DataSaver.DeleteFile(Application.persistentDataPath + @"/logginSettings.lgrsd");
        RefreshLogin();
        cameraController.ChangeFocus("mainMenu");
    }

    public static string CreateHead(string userId, string sharedSecret)
    {
        int unixTime = (int)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedSecret)).ComputeHash(Encoding.UTF8.GetBytes(unixTime + ":" + userId));
        return unixTime + ":" + ToHexString(hmac);
    }

    public static string ToHexString(byte[] bytes)
    {
        return System.BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}
