using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandController : MonoBehaviour {

    Image background;
    Text text;
    bool commandEnabled;
    bool varraSan = false;
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
        //if (varraSan)
        //{
        //    Vector2 pos = GameManager.gameManagerReference.player.transform.position;
        //    GameManager.gameManagerReference.SetTileAt((int)pos.x * GameManager.gameManagerReference.WorldHeight + ((int)pos.y + 10), 62, false);
        //}

        //if (Input.GetKeyDown(KeyCode.G) && Application.isEditor && GameManager.gameManagerReference.InGame)
        //{
        //    ENTITY_Raideon.StaticSpawn(null, GameManager.gameManagerReference.player.transform.position + Vector3.up * 5f);
        //}

        if (!commandEnabled)
        {
            if (GInput.GetKeyDown(KeyCode.F12) && GameManager.gameManagerReference.InGame && GameManager.gameManagerReference.player.alive && Debug.isDebugBuild)
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
                    GameManager.gameManagerReference.player.LoseHp(99999,GameManager.gameManagerReference.player.GetComponent<EntityCommonScript>(), true);
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
                    GameManager.gameManagerReference.SpawnEntity(GameManager.gameManagerReference.StringToEntity(entity), null, spawn);
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

                    ManagingFunctions.DropItem(item, GameManager.gameManagerReference.player.transform.position, amount: repeat);
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
            else if (input[0] == "sun")
            {
                varraSan = true;
            }
            else
            {
                Debug.Log("unrecognized");
            }
        }
    }
}
