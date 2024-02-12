using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandController : MonoBehaviour {

    Image background;
    Text text;
    bool commandEnabled;
    string command = "";
    char writebar = ' ';
    int writebarFrame = 0;
    public string thingForced;

	void Start () {
        background = GetComponent<Image>();
        text = transform.GetChild(0).GetComponent<Text>();

        commandEnabled = false;
        background.enabled = false;
        text.enabled = false;
	}
	

	void Update () {
        if (Input.GetKeyDown(KeyCode.G) && Application.isEditor)
        {
            UNIT_Darkn.StaticSpawn(null, new Vector2(GameManager.gameManagerReference.player.transform.position.x, 80));
        }

        if (!commandEnabled)
        {
            if (GInput.GetKeyDown(KeyCode.F12) && GameManager.gameManagerReference.InGame)
            {
                commandEnabled = true;
                GameManager.gameManagerReference.InGame = false;
                command = "";
            }

            background.enabled = false;
            text.enabled = false;
        }
        else
        {
            if(writebarFrame < 10)
            {
                writebarFrame++;
            }
            else
            {
                writebarFrame = 0;
                if(writebar == '_')
                {
                    writebar = ' ';
                }
                else
                {
                    writebar = '_';
                }
            }

            background.enabled = true;
            text.enabled = true;
            if (GInput.GetKeyDown(KeyCode.Return))
            {
                ProcessCommand(command);
                Debug.Log(command);
                commandEnabled = false;
                GameManager.gameManagerReference.InGame = true;
            }
            if (GInput.GetKeyDown(KeyCode.Backspace) && command.Length - 1 >= 0) command = command.Remove(command.Length - 1);
            if (commandEnabled && !GInput.GetKeyDown(KeyCode.Backspace)) command = command + Input.inputString;
            text.text = "/" + command + writebar;
            
        }
	}

    void ProcessCommand(string commandInput)
    {
        commandInput = commandInput.ToLower();
        string[] input = commandInput.Split();

        if (input.Length > 0)
        {
            if (input[0] == "kill")
            {
                if(input.Length == 1)
                {
                    GameManager.gameManagerReference.InGame = true;
                    GameManager.gameManagerReference.player.LoseHp(10000, true);
                    GameManager.gameManagerReference.player.deathScreenController.InstaKill();
                }
                else if(input.Length == 2)
                {
                    bool isInt = int.TryParse(input[1], out int intValue);
                    if (isInt)
                    {
                        if (GameManager.gameManagerReference.entitiesContainer.transform.childCount > intValue)
                        {
                            Destroy(GameManager.gameManagerReference.entitiesContainer.transform.GetChild(intValue).gameObject);
                            Debug.Log("Killed entity in index " + intValue + ".");
                        }
                        else
                            Debug.Log("No entity founded in index " + intValue + ".");
                    }
                    else
                    {
                        //kill the x player
                    }
                }
            }
            else if (input[0] == "spawn")
            {
                int loopIdx = 0;
                string entity = "";
                Vector2 spawn = new Vector2();
                foreach (string s in input)
                {
                    if (loopIdx == 1)
                    {
                        entity = s;
                    }
                    if (loopIdx == 2)
                    {
                        if (s == "#") spawn.x = GameManager.gameManagerReference.player.transform.position.x;
                        else spawn.x = System.Convert.ToSingle(s);
                    }
                    if (loopIdx == 3)
                    {
                        if (s == "#") spawn.y = GameManager.gameManagerReference.player.transform.position.y;
                        else spawn.y = System.Convert.ToSingle(s);
                    }

                    loopIdx++;
                }

                if (entity != "")
                {
                    if (entity == "krotek")
                    {
                        ENTITY_KrotekController.StaticSpawn(null, spawn);
                    }
                    if (entity == "nanobot1")
                    {
                        ENTITY_NanoBotT1.StaticSpawn(null, spawn);
                    }
                    if (entity == "nanobot2")
                    {
                        ENTITY_NanoBotT2.StaticSpawn(null, spawn);
                    }
                    if (entity == "nanobot3")
                    {
                        ENTITY_NanoBotT3.StaticSpawn(null, spawn);
                    }
                    if (entity == "destroyer")
                    {
                        ENTITY_TheDestroyer.StaticSpawn(null, spawn);
                    }
                    if (entity == "darkn")
                    {
                        UNIT_Darkn.StaticSpawn(null, spawn);
                    }
                }
            }
            else if (input[0] == "give")
            {
                try
                {
                    if (System.Convert.ToInt32(input[1]) >= GameManager.gameManagerReference.tiles.Length)
                    {
                        throw new System.IndexOutOfRangeException("Item index is out of range!");
                    }
                    int item = System.Convert.ToInt32(input[1]);
                    int repeat = System.Convert.ToInt32(input[2]);

                    ManagingFunctions.DropItem(item, GameManager.gameManagerReference.player.transform.position, repeat);
                }
                catch (System.Exception exception)
                {
                    Debug.Log("==INFORMATIVE== " + exception);
                    Debug.Log(exception.Source);
                    Debug.Log(exception.Message);
                }
            }
            else if(input[0] == "goto")
            {
                GameManager.gameManagerReference.player.transform.position = new Vector2(System.Convert.ToSingle(input[1]), System.Convert.ToSingle(input[2]));
                GameManager.gameManagerReference.UpdateChunksActive();
            }
            else if (input[0] == "skin")
            {
                GameManager.gameManagerReference.player.skin = System.Convert.ToInt32(input[1]);
            }
            else if (input[0] == "customres")
            {
                Screen.SetResolution(System.Convert.ToInt32(input[1]), System.Convert.ToInt32(input[2]), Screen.fullScreen);
            }
            else if (input[0] == "setdaytime")
            {
                GameManager.gameManagerReference.dayTime = System.Convert.ToInt32(input[1]);
                LightController.lightController.AddRenderQueue(Camera.main.transform.position);
            }
            else
            {
                Debug.Log("unrecognized");
            }
        }
    }
}
