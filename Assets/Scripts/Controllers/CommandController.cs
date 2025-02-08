using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandController : MonoBehaviour {

    Image background;
    Text text;
    bool commandEnabled;
    bool varraSan = false;
    public bool showcaseMode = false;
    string command = "";
    char writebar = ' ';
    int writebarFrame = 0;
    public static CommandController commandController;

    public GameObject chatDialog;
    public Text chatDialogTxt;


	void Start () {
        background = GetComponent<Image>();
        text = transform.GetChild(0).GetComponent<Text>();
        commandController = this;

        commandEnabled = false;
        background.enabled = false;
        text.enabled = false;
	}
	

	void Update () {
        //if (varraSan)
        //{
        //    GameManager.gameManagerReference.player.HP += 1;
        //}

        if (showcaseMode)
        {
            GameManager.gameManagerReference.player.transform.Translate(Vector2.right * Time.deltaTime);
            int idx = ManagingFunctions.CreateIndex(Vector2Int.RoundToInt(GameManager.gameManagerReference.player.transform.position));
            int mesh1 = GameManager.gameManagerReference.TileCollisionType[GameManager.gameManagerReference.GetTileAt(idx - 1)];
            if (mesh1 == 1 || mesh1 == 3) GameManager.gameManagerReference.player.transform.Translate(Vector2.up * Time.deltaTime);
            int mesh2 = GameManager.gameManagerReference.TileCollisionType[GameManager.gameManagerReference.GetTileAt(idx - 2)];
            if (mesh2 != 1 && mesh2 != 3) GameManager.gameManagerReference.player.transform.Translate(Vector2.down * Time.deltaTime);

        }

        if (Input.GetKeyDown(KeyCode.G) && Application.isEditor && GameManager.gameManagerReference.InGame)
        {

        }

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
                chatDialog.SetActive(false);
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
        try
        {
            commandInput = commandInput.ToLower();
            string[] input = commandInput.Split();

            if (input.Length > 0)
            {
                if (input[0] == "kill")
                {
                    if (input.Length == 1)
                    {
                        GameManager.gameManagerReference.InGame = true;
                        GameManager.gameManagerReference.player.LoseHp(99999, GameManager.gameManagerReference.player.GetComponent<EntityCommonScript>(), true);
                    }
                    else if (input.Length == 2)
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
                else if (input[0] == "goto")
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
                else if (input[0] == "step")
                {
                    PlanetMenuController.planetMenu.Simulate(System.Convert.ToSingle(input[1]), false);
                    LightController.lightController.AddRenderQueue(Camera.main.transform.position);
                }
                else if (input[0] == "sun")
                {
                    varraSan = true;
                }
                else if (input[0] == "showcase")
                {
                    GameManager.gameManagerReference.InGame = true;
                    GameManager.gameManagerReference.player.LoseHp(99999, GameManager.gameManagerReference.player.entityScript, true, 0, true);
                    MenuController.menuController.UIActive = false;
                    showcaseMode = true;

                }
                else
                {
                    Debug.Log("unrecognized");
                }
            }
        }
        catch(System.Exception ex)
        {
            chatDialogTxt.text = ex.Message;
            chatDialog.SetActive(true);
        }
    }
}
