using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public enum Biomes : int
{
    meadow = 0, forest = 1, mountain = 2, ocean = 3,
}

public enum Entities : int
{
    NanoBotT1 = 0, KrotekBoss = 1, TheDestroyer = 2, NanoBotT2 = 3, AntiLaser = 4, NanoBotT3 = 5,
}

public enum Projectiles : int
{
    Arrow = 0, SwordParticle = 1, Laser = 2,
}

public class GameManager : MonoBehaviour
{
    public static GameManager gameManagerReference;
    [SerializeField] private bool inGame = false;
    [SerializeField] private bool playerFocused = false;
    public int seed = 0;
    public int frameTimer = 0;

    public GameObject[] EntitiesGameObject;
    public GameObject[] ProjectilesGameObject;
    public GameObject[] GameChunks;
    public GameObject[] LoadedChunks;
    public Transform chunksLimits;
    public Transform dummyObjects;

    [Header("Tiles Config")]
    [SerializeField] public Sprite[] tiles;
    [SerializeField] public Sprite[] tileBuildPrev;
    [SerializeField] public string[] tileType;
    [SerializeField] public string[] TileCollisionType;
    [SerializeField] public string[] tileMainProperty;
    [SerializeField] public string[] tileDefaultBrokeTool;
    [SerializeField] public bool[] canRotate;
    [SerializeField] public int[] ToolEfficency;
    [SerializeField] public string[] tileName;
    [SerializeField] public Color[] rawColor;
    [SerializeField] public BlockAnimationData[] tileAnimation;
    [SerializeField] public Vector4[] tileSize;
    [SerializeField] public PlayerController player;
    [SerializeField] public MainSoundController soundController;
    [SerializeField] public int WorldHeight;
    [SerializeField] public int WorldWidth;
    [SerializeField] public GameObject emptyTile;
    [SerializeField] public GameObject lecterSprite;
    [SerializeField] public GameObject emptySprite;
    [SerializeField] GameObject emptyChunk;
    [SerializeField] GameObject falseChunk;
    [SerializeField] public GameObject chunkContainer;
    [SerializeField] public GameObject entitiesContainer;
    [SerializeField] GameObject Transition;
    [SerializeField] GameObject savingText;

    [Header("Other")]
    public bool Generated = false;
    public bool isNetworkClient = false;
    public bool isNetworkHost = false;
    public int brush;
    public int mouseUp = 0;
    public int chosenBrush = 1;
    public bool doingAnAction = false;
    public bool canBuild = false;
    public bool canUseTool = false;
    public bool canAtack = false;
    public bool canEquip = false;
    public bool canConsume = false;
    public bool building = false;
    public bool usingTool = false;
    public bool usingArm = false;
    public int buildRotation = 0;
    public string armUsing = "";
    public string toolUsing = "";
    public int toolUsingEfficency = 0;
    public int armUsingDamageDeal = 0;
    public string equipType = "";
    public Vector3 mouseCurrentPosition;
    public int NumberOfTilesInChunk;

    public int[] equipedArmor = new int[3];

    //Auxiliar for build
    GameObject lastTileBrush;
    [HideInInspector] public int breakingTime = -1;
    public int tileBreaking = -1;
    public bool cancelPlacing = false; //Mobile

    //The map container
    public int[] allMapGrid;
    public string currentPlanetName = "Korenz";
    public string currentHexPlanetColor = "25FF00FF";
    public bool currentPlanetIsRoot = true;

    //Events and game data
    public Color skyboxColor;
    public Color daytimeUpdatedSkyboxColor;
    public float dayFloat = 0;
    public int dayTime = fullDay / 5;
    public const int fullDay = 54000;
    public float dayLuminosity = 1;
    public string worldName = "null";
    public string worldRootName = "null";
    public bool isLorePlanet = true;
    public bool isSavingData = false;
    public string persistentDataPath = "null";

    //Lists
    List<GameObject> poolTiles = new List<GameObject>();
    public List<Vector2> platformDrops = new List<Vector2>();
    public List<Vector2> buoyancyFloats = new List<Vector2>();

    public SkinContainer[] playerSkins;

    public bool InGame
    {
        get { return inGame; }
        set
        {
            inGame = value;
            if (isNetworkClient || isNetworkHost) inGame = true;

            Rigidbody2D[] rigidbodys = FindObjectsOfType<Rigidbody2D>();
            foreach (Rigidbody2D rigidbody2D in rigidbodys)
            {
                rigidbody2D.simulated = inGame;
            }
        }
    }

    public bool PlayerFocused
    {
        get { return playerFocused; }
        set { playerFocused = value; }
    }

    void Start()
    {  
        //Setup
        skyboxColor = Camera.main.backgroundColor;
        Time.timeScale = 1.0f;
        soundController = GameObject.Find("Audio").GetComponent<MainSoundController>();

        worldName = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0];
        persistentDataPath = Application.persistentDataPath;

        if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("isNetworkLoad")[0] == "true") isNetworkClient = true;
        if (Server.isOpen) isNetworkHost = true;

        int rootPlanetIdx = worldName.IndexOf("/");
        if (rootPlanetIdx >= 0)
        {
            worldRootName = worldName.Substring(0, rootPlanetIdx);
            currentPlanetIsRoot = false;
        }
        else
        {
            worldRootName = worldName;
        }

        if (!isNetworkClient)
        {
            if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldLoadType")[0] == "newWorld")
            {
                try
                {
                    Random.InitState(System.Convert.ToInt32(GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("seed")[0]));
                    seed = System.Convert.ToInt32(GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("seed")[0]);
                }
                catch
                {
                    int random = Random.Range(0, 999999999);
                    Random.InitState(random);
                    seed = random;
                }
            }
            else if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldLoadType")[0] == "newPlanet")
            {
                currentHexPlanetColor = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("planetColor")[0];
                currentPlanetName = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("planetName")[0];
                isLorePlanet = false;

                int random = Random.Range(0, 999999999);
                Random.InitState(random);
                seed = random;
            }
            else if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldLoadType")[0] == "existingPlanet")
            {
                currentPlanetName = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("planetName")[0];
                if (DataSaver.CheckIfFileExists(persistentDataPath + @"/worlds/" + worldName + @"/planetColor.lgrsd"))
                    currentHexPlanetColor = DataSaver.LoadStats(persistentDataPath + @"/worlds/" + worldName + @"/planetColor.lgrsd").SavedData[0];
                else
                    currentHexPlanetColor = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("planetColor")[0];
                isLorePlanet = false;

                try
                {
                    seed = System.Convert.ToInt32(DataSaver.ReadTxt(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/seed.lgrsd")[0]);
                }
                catch
                {
                    seed = 000000000;
                    DataSaver.CreateTxt(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/seed.lgrsd", new string[] { seed + "" });
                }
            }
            else
            {
                try
                {
                    seed = System.Convert.ToInt32(DataSaver.ReadTxt(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/seed.lgrsd")[0]);
                }
                catch
                {
                    seed = 000000000;
                    DataSaver.CreateTxt(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/seed.lgrsd", new string[] { seed + "" });
                }
            }

            if (DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/equips.lgrsd"))
            {
                equipedArmor = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/equips.lgrsd").SavedData);
            }
            else
            {
                DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(equipedArmor), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/equips.lgrsd");
            }

            if (DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/daytime.lgrsd"))
            {
                dayTime = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/daytime.lgrsd").SavedData)[0];
            }
            else
            {
                DataSaver.SaveStats(new string[] { (fullDay / 5) + "" }, Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/daytime.lgrsd");
            }
        }

        InventoryBar.Reset();
        StackBar.Reset();

        frameTimer = 0;
        Transition.SetActive(true);
        gameManagerReference = this;

        //Generate all
        Generated = false;
        GenerateAllChunks(GenerateMap());
        Generated = true;

        player.Respawn((WorldWidth * 16) / 2, SearchForClearSpawn((WorldWidth * 16) / 2) + 2);
        GameObject.Find("IlluminationCape").GetComponent<LightControllerCurrent>().AddRenderQueue(player.transform.position);
        inGame = true;
        playerFocused = true;
        StartCoroutine(CheckChunks());
        StartCoroutine(ManageTransition("CanStart", true, 1f));
        Invoke("UpdateChunksActive", 0.1f);

        LoadEntities();
    }


    void Update()
    {
        mouseCurrentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (inGame)
        {
            frameTimer++;

            dayFloat += Time.deltaTime * 60;

            while (dayFloat >= 1)
            {
                dayTime++;
                dayFloat--;
            }
            
            if (frameTimer > 2000000000)
            {
                frameTimer = 0;
            }
            if (dayTime >= fullDay)
            {
                dayTime = dayTime - fullDay;
            }
            SetSkybox();

            

            if (Input.GetMouseButtonUp(0))
            {
                breakingTime = -1;
            }

            if (Generated)
            {
                if (GInput.GetMouseButton(0))
                {
                    mouseUp = 0;
                }
                else if (mouseUp >= 3)
                {
                    brush = -1;
                }
                mouseUp++;

                chunksLimits.GetChild(2).transform.position = new Vector2(player.transform.position.x, WorldHeight + 2);

                if (GInput.GetKeyDown(KeyCode.V))
                {
                    if (player.alive && canBuild)
                    {
                        if (building)
                        {
                            building = false;
                        }
                        else
                        {
                            building = true;
                        }
                    }
                    if (player.alive && canUseTool)
                    {
                        if (usingTool)
                        {
                            usingTool = false;
                        }
                        else
                        {
                            usingTool = true;
                        }
                    }
                    if (player.alive && canAtack)
                    {
                        if (usingArm)
                        {
                            usingArm = false;
                        }
                        else
                        {
                            usingArm = true;
                        }
                    }
                    if (player.alive && canEquip)
                    {
                        soundController.PlaySfxSound(SoundName.select);
                        int equipPiece = StackBar.stackBarController.currentItem;
                        int idx = 0;
                        if (equipType == "helmet")
                        {
                            idx = 0;
                        }
                        else if (equipType == "chestplate")
                        {
                            idx = 1;
                        }
                        else if (equipType == "boots")
                        {
                            idx = 2;
                        }
                        
                        if (equipedArmor[idx] > 0)
                        {
                            StackBar.stackBarController.StackBarGrid[StackBar.stackBarController.idx] = equipedArmor[idx];
                        }
                        else
                        {
                            StackBar.LoseItem();
                        }

                        equipedArmor[idx] = equipPiece;
                    }
                    if (player.alive && canConsume)
                    {
                        Consume(StackBar.stackBarController.currentItem);
                        StackBar.LoseItem();
                    }
                }
                {//Manage Action
                    if (!canBuild)
                    {
                        building = false;
                        canBuild = false;
                    }

                    if (!canUseTool)
                    {
                        usingTool = false;
                        canUseTool = false;
                    }

                    if (!canAtack)
                    {
                        usingArm = false;
                        canAtack = false;
                    }

                    if (building && !player.alive)
                    {
                        building = false;
                    }

                    if (usingTool && !player.alive)
                    {
                        usingTool = false;
                    }

                    if (usingArm && !player.alive)
                    {
                        usingArm = false;
                    }

                    if (building || usingTool || usingArm)
                    {
                        doingAnAction = true;
                    }
                    else
                    {
                        doingAnAction = false;
                    }

                }


                //SendUpdateToAllChunks();
            }

            if (GInput.GetKeyDown(KeyCode.F2))
            {
                int freeFile = 1;

                for (int newFreeFile = 1; DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/screenshots/screenshot" + newFreeFile + ".png"); newFreeFile++)
                {
                    freeFile++;
                }
                ScreenCapture.CaptureScreenshot(Application.persistentDataPath + @"/screenshots/screenshot" + freeFile + ".png");
            }

            if (!building || (building && !canRotate[chosenBrush]))
            {
                buildRotation = 0;
            }
            else
            {
                if (GInput.GetKeyDown(KeyCode.R))
                {
                    buildRotation = (int)Mathf.Repeat(buildRotation - 90, 360);
                }
            }
        }

        savingText.SetActive(isSavingData);
    }

    public int GetCapacityOfCore(int core)
    {
        int result = 0;
        switch (core)
        {
            case 96:
                result = 200;
                break;
            case 97:
                result = 450;
                break;
            case 100:
                result = 755;
                break;
        }
        return result;
    }

    public GameObject ExtractPooledTile(Vector2 position)
    {
        if (poolTiles.Count > 0)
        {
            return poolTiles[0];
        }
        else
        {
            GameObject newTile = Instantiate(emptyTile, new Vector3(position.x, position.y, 0), Quaternion.identity);
            return newTile;
        }
    }

    public void AddTileToPool(GameObject tile)
    {
        tile.transform.SetParent(transform);
        tile.GetComponent<SpriteRenderer>().sprite = tiles[0];
        tile.transform.position = Vector2.one * -1;
        if (tile.GetComponent<PlatformEffector2D>() != null)
        {
            Destroy(tile.GetComponent<PlatformEffector2D>());
        }
        poolTiles.Add(tile);
    }

    public void ImportResources()
    {
        try
        {
            string resources = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("resources")[0];

            foreach (string resource in resources.Split(';'))
            {
                if (resource != "")
                {
                    int tile = System.Convert.ToInt32(resource.Split(':')[0]);
                    int tileAmount = System.Convert.ToInt32(resource.Split(':')[1]);

                    for (int e = 0; e < tileAmount; e++)
                    {
                        if (!StackBar.AddItem(tile))
                        {
                            ManagingFunctions.DropItem(tile, player.transform.position);
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("==NOT IMPORTED RESOURCES== " + ex.Message + " " + ex.ToString());
        }
    }

    public void LoadEntities()
    {
        if(DataSaver.CheckIfFileExists(persistentDataPath + @"/worlds/" + worldName + @"/ent.lgrsd"))
        {
            string[] entities = DataSaver.LoadStats(persistentDataPath + @"/worlds/" + worldName + @"/ent.lgrsd").SavedData;

            foreach(string entity in entities)
            {
                string[] datas = entity.Split(';');

                Vector2 spawnPos = new Vector2(System.Convert.ToSingle(datas[2]), System.Convert.ToSingle(datas[3]));
                int entityHp = System.Convert.ToInt32(datas[1]);
                string entityType = datas[0];
                EntityBase entityBase = null;

                if (entityType == "nanobot1")
                {
                    entityBase = ENTITY_NanoBotT1.StaticSpawn(null, spawnPos);
                }
                if (entityType == "nanobot2")
                {
                    entityBase = ENTITY_NanoBotT2.StaticSpawn(null, spawnPos);
                }
                if (entityType == "nanobot3")
                {
                    entityBase = ENTITY_NanoBotT3.StaticSpawn(null, spawnPos);
                }
                if (entityType == "antilaser")
                {
                    entityBase = ENTITY_AntiLaser.StaticSpawn(null, spawnPos);
                }

                if (entityBase != null)
                {
                    entityBase.Hp = entityHp;
                }
            }
        }
    }

    private void SetSkybox()
    {
        float value = 1;
        bool afterMiddleDay = dayTime > (fullDay / 2);

        if (!afterMiddleDay)
        {
            value = (float)dayTime / (fullDay / 2);
        }
        else
        {
            value = 1f - ((float)dayTime / (fullDay / 2)) + 1f;
        }


        if (!afterMiddleDay)
        {
            if (value > 0.05f)
            {
                value = value * 2;
            }
            else if (value > 0.1f)
            {
                value = value * 3;
            }
            else if (value > 0.2f)
            {
                value = value * 4;
            }
            else if (value > 0.3f)
            {
                value = value * 5;
            }
        }
        else
        {
            if (value < 0.3f)
            {
                value = value / 10f;
            }
            else if (value < 0.6f)
            {
                value = value / 5f;
            }
            else if (value < 0.65f)
            {
                value = value / 2f;
            }
            else if (value < 0.7f)
            {
                value = value / 1.6f;
            }
            else if (value < 0.75f)
            {
                value = value / 1.2f;
            }
        }
        value += 0.05f;

        dayLuminosity = Mathf.Clamp01(value);
        daytimeUpdatedSkyboxColor = skyboxColor * value;
    }

    public void DropOn(Vector2 position, float length)
    {
        platformDrops.Add(position);
        StartCoroutine(DropOff(position, length));
    }

    private IEnumerator DropOff(Vector2 position, float time)
    {
        yield return new WaitForSeconds(time);
        platformDrops.Remove(position);
    }

    public void FloatOn(Vector2 position, float length)
    {
        buoyancyFloats.Add(position);
        StartCoroutine(FloatOff(position, length));
    }

    private IEnumerator FloatOff(Vector2 position, float time)
    {
        yield return new WaitForSeconds(time);
        buoyancyFloats.Remove(position);
    }

    public string[] GetBiomes()
    {
        return DataSaver.ReadTxt(Application.persistentDataPath + @"/worlds/" + worldName + @"/mapBiomes.lgrsd");
    }

    public void SaveGameData(bool alternateThread)
    {
        if (!isSavingData)
        {
            if (alternateThread)
            {
                Thread mapSave = new Thread(new ThreadStart(SaveGameDataThread));
                mapSave.Start();
            }
            else
            {
                SaveGameDataThread();
            }
            ManagingFunctions.SaveStackBarAndInventory();

            EntityCommonScript[] entityCommonScripts = entitiesContainer.GetComponentsInChildren<EntityCommonScript>(true);

            List<string> entities = new List<string>();

            foreach (EntityCommonScript entity in entityCommonScripts)
            {
                if (entity.entityBase != null && entity.saveToFile)
                {
                    List<string> toEncrypt = new List<string>();
                    toEncrypt.Add(entity.EntityType + ";");
                    toEncrypt.Add(entity.entityBase.Hp + ";");
                    toEncrypt.Add(entity.transform.position.x + ";" + entity.transform.position.y);
                    entities.Add(string.Join("", toEncrypt.ToArray()));
                }
            }

            DataSaver.SaveStats(entities.ToArray(), persistentDataPath + @"/worlds/" + worldName + @"/ent.lgrsd");
        }
    }

    private void SaveGameDataThread()
    {
        isSavingData = true;

        if (!isNetworkClient)
        {
            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(allMapGrid), persistentDataPath + @"/worlds/" + worldName + @"/map.lgrsd");
            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(equipedArmor), persistentDataPath + @"/worlds/" + worldName + @"/equips.lgrsd");
            DataSaver.SaveStats(new string[] { currentHexPlanetColor }, persistentDataPath + @"/worlds/" + worldName + @"/planetColor.lgrsd");
            DataSaver.SaveStats(new string[] { dayTime + "" }, persistentDataPath + @"/worlds/" + worldName + @"/daytime.lgrsd");
            DataSaver.SaveStats(new string[] { currentPlanetName }, persistentDataPath + @"/worlds/" + worldRootName + @"/lastLocation.lgrsd");
        }

        isSavingData = false;
    }


    int[] GenerateMap()
    {
        int[] mapGrid;

        if (isNetworkClient)
        {
            mapGrid = Client.worldMapLoad;
            WorldHeight = Client.worldProportionsLoad[0];
            WorldWidth = Client.worldProportionsLoad[1];
        }
        else
        {
            if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldLoadType")[0] == "newWorld") mapGrid = GenerateNewMapGrid();
            else if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldLoadType")[0] == "newPlanet") mapGrid = GenerateNewMapGrid();
            else
            {
                if (!DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/mapBiomes.lgrsd"))
                {
                    string[] wB = new string[System.Convert.ToInt32(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/wp.lgrsd").SavedData[0])];
                    for (int i = 0; i < wB.Length; i++)
                    {
                        wB[i] = "unknownBiome";
                    }
                    DataSaver.CreateTxt(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/mapBiomes.lgrsd", wB);
                }
                mapGrid = LoadMapGrid(GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0]);
            }
        }

        NumberOfTilesInChunk = WorldHeight * 16;

        return mapGrid;
    }

    int[] GenerateNewMapGrid()
    {
        WorldWidth = ManagingFunctions.ConvertStringToIntArray(GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("newWorldSize"))[0];
        WorldHeight = ManagingFunctions.ConvertStringToIntArray(GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("newWorldSize"))[1];

        int[] buildedMapGrid = new int[(WorldWidth * 16) * WorldHeight];
        string[] mapBiomes = new string[WorldWidth];
        string[] biomes = { "meadow", "forest", "mountain", "ocean" };
        float[] biomeProbability = { 10, 15, 5, 0 };
        int[] worldTileHeight = new int[WorldWidth * 16];

        float totalProbability = 0f;

        foreach (float f in biomeProbability)
        {
            totalProbability += f;
        }

        float aaa = Random.Range(-9999f, 9999f);

        for (int i = 0; i < worldTileHeight.Length; i++)
        {
            worldTileHeight[i] = (int)(Mathf.PerlinNoise(i * 0.05f, aaa) * 30) + (WorldHeight / 2 /*0.5*/);
        }

        for (int i = 0; i < WorldWidth; i++)
        {
            string finalBiome = "forest";//delete meadow (TEMP!!!)
            //float randomNumber = Random.Range(0, totalProbability);

            //float n = 0f;
            //float nn = 0f;

            //for (int e = 0; e < biomes.Length; e++)
            //{
            //    if (e > 0) n += biomeProbability[e - 1];
            //    nn += biomeProbability[e];

            //    if (randomNumber > n && randomNumber <= nn)
            //    {
            //        finalBiome = biomes[e];
            //        break;
            //    }
            //}


            mapBiomes[i] = finalBiome;
        }

        for (int i = 0; i < buildedMapGrid.Length; i++)
        {
            buildedMapGrid[i] = 6;
        }


        int floorUndergroundEnd = worldTileHeight[0] - 5;
        int floorSurfTile = 1;
        int floorUndergroundTile = 7;


        for (int i = 0; i < mapBiomes.Length; i++)
        {
            string currentBiome = mapBiomes[i];
            floorUndergroundEnd = worldTileHeight[i * 16] - 5;

            if (currentBiome == biomes[(int)Biomes.meadow])
            {
                floorSurfTile = 1;
                floorUndergroundTile = 7;

                for (int i2 = 0; i2 < 16; i2++)
                {
                    floorUndergroundEnd = worldTileHeight[i * 16 + i2] - Random.Range(2, 6);

                    for (int e = 0; e < WorldHeight; e++)
                    {
                        int idx = (((i * 16) + i2) * WorldHeight) + e;

                        if (e < 3)
                        {
                            if (Random.Range(0, e + 1) == 0)
                            {
                                buildedMapGrid[idx] = 13;
                            }
                        }
                        if (!(e > floorUndergroundEnd) && buildedMapGrid[idx] == 6)
                        {
                            if (e < floorUndergroundEnd * 0.9f && e > floorUndergroundEnd * 0.65f && Random.Range(0, 15) == 0)
                            {
                                buildedMapGrid[idx] = 11;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 3) == 0) buildedMapGrid[idx + ore2] = 11;
                            }
                            else if (e < floorUndergroundEnd * 0.85f && e > floorUndergroundEnd * 0.2f && Random.Range(0, 25) == 0)
                            {
                                buildedMapGrid[idx] = 8;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 11) == 0) buildedMapGrid[idx + ore2] = 8;
                            }
                            else if (e < floorUndergroundEnd * 0.75f && e > floorUndergroundEnd * 0.5f && Random.Range(0, 25) == 0)
                            {
                                buildedMapGrid[idx] = 10;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 8) == 0) buildedMapGrid[idx + ore2] = 10;
                            }
                            else if (e < floorUndergroundEnd * 0.7f && e > floorUndergroundEnd * 0.4f && Random.Range(0, 30) == 0)
                            {
                                buildedMapGrid[idx] = 9;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 20) == 0) buildedMapGrid[idx + ore2] = 9;
                            }
                            else if (e < floorUndergroundEnd * 0.25f && e > floorUndergroundEnd * 0.1f && Random.Range(0, 35) == 0)
                            {
                                buildedMapGrid[idx] = 12;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 14) == 0) buildedMapGrid[idx + ore2] = 12;
                            }
                        }
                        if (e > floorUndergroundEnd && e < worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = floorUndergroundTile;
                        }
                        if (e == worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = floorSurfTile;
                        }
                        if (e > worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = 0;
                        }

                    }
                }
            }



            if (currentBiome == biomes[(int)Biomes.forest])
            {
                for (int i2 = 0; i2 < 16; i2++)
                {
                    floorUndergroundEnd = worldTileHeight[i * 16 + i2] - Random.Range(2, 6);

                    for (int e = 0; e < WorldHeight; e++)
                    {
                        int idx = (((i * 16) + i2) * WorldHeight) + e;

                        if (e < 3)
                        {
                            if (Random.Range(0, e + 1) == 0)
                            {
                                buildedMapGrid[idx] = 13;
                            }
                        }
                        if (!(e > floorUndergroundEnd) && buildedMapGrid[idx] == 6)
                        {
                            if (e < floorUndergroundEnd * 0.9f && e > floorUndergroundEnd * 0.65f && Random.Range(0, 15) == 0)
                            {
                                buildedMapGrid[idx] = 11;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 3) == 0) buildedMapGrid[idx + ore2] = 11;
                            }
                            else if (e < floorUndergroundEnd * 0.85f && e > floorUndergroundEnd * 0.2f && Random.Range(0, 25) == 0)
                            {
                                buildedMapGrid[idx] = 8;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 11) == 0) buildedMapGrid[idx + ore2] = 8;
                            }
                            else if (e < floorUndergroundEnd * 0.75f && e > floorUndergroundEnd * 0.5f && Random.Range(0, 25) == 0)
                            {
                                buildedMapGrid[idx] = 10;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 8) == 0) buildedMapGrid[idx + ore2] = 10;
                            }
                            else if (e < floorUndergroundEnd * 0.7f && e > floorUndergroundEnd * 0.4f && Random.Range(0, 30) == 0)
                            {
                                buildedMapGrid[idx] = 9;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 20) == 0) buildedMapGrid[idx + ore2] = 9;
                            }
                            else if (e < floorUndergroundEnd * 0.25f && e > floorUndergroundEnd * 0.1f && Random.Range(0, 20) == 0)
                            {
                                buildedMapGrid[idx] = 12;
                                int ore2 = Random.Range(-1, 2);
                                if (buildedMapGrid[idx + ore2] == 6 && Random.Range(0, 5) == 0) buildedMapGrid[idx + ore2] = 12;
                            }
                        }
                        if (e > floorUndergroundEnd && e < worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = floorUndergroundTile;
                        }
                        if (e == worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = floorSurfTile;
                        }
                        if (e > worldTileHeight[i * 16 + i2] && buildedMapGrid[idx] != 53 && buildedMapGrid[idx] != 55)
                        {
                            buildedMapGrid[idx] = 0;
                        }
                        if (Random.Range(0, 3) == 0 && i2 < 14 && i2 > 3 && e > worldTileHeight[i * 16 + i2])
                        {
                            if (buildedMapGrid[idx - 1] == 1 && buildedMapGrid[idx] == 0 &&
                                buildedMapGrid[idx + WorldHeight] != 53 && buildedMapGrid[idx - WorldHeight] != 53
                                && buildedMapGrid[idx + WorldHeight * 2] != 53 && buildedMapGrid[idx - WorldHeight * 2] != 53)
                            {
                                buildedMapGrid[idx - 1] = 7;
                                buildedMapGrid[idx] = 53;
                                buildedMapGrid[idx + 1] = 53;
                                buildedMapGrid[idx + 2 + WorldHeight] = 55;
                                buildedMapGrid[idx + 2] = 55;
                                buildedMapGrid[idx + 2 - WorldHeight] = 55;
                                buildedMapGrid[idx + 3 + WorldHeight] = 55;
                                buildedMapGrid[idx + 3] = 55;
                                buildedMapGrid[idx + 3 - WorldHeight] = 55;
                            }
                        }

                    }
                }
            }

            if (currentBiome == biomes[(int)Biomes.mountain])
            {
                for (int i2 = 0; i2 < 16; i2++)
                {
                    floorUndergroundEnd = worldTileHeight[i * 16 + i2] - Random.Range(3, 9);

                    for (int e = 0; e < WorldHeight; e++)
                    {
                        int idx = (((i * 16) + i2) * WorldHeight) + e;

                        if (Random.Range(0, 9) == 0 && buildedMapGrid[idx] == 6 && e < worldTileHeight[i * 16 + i2] * 0.7f)
                        {
                            buildedMapGrid[idx] = 8 + Random.Range(0, 4);
                        }
                        if (e < 3)
                        {
                            if (Random.Range(0, e + 1) == 0)
                            {
                                buildedMapGrid[idx] = 13;
                            }
                        }
                        else if (e < floorUndergroundEnd * 0.10f && buildedMapGrid[idx] != 13 && Random.Range(0, 3) == 0)
                        {
                            buildedMapGrid[idx] = 17;
                        }

                        if (e > floorUndergroundEnd && e < worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = floorUndergroundTile;
                        }
                        if (e == worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = floorSurfTile;
                        }
                        if (e > worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = 0;
                        }
                    }
                }
            }

            float perlinFreq = 0.08f;
            float perlinOffsetX = Random.Range(-9999f, 9999f);
            float perlinOffsetY = Random.Range(-9999f, 9999f);

            Texture2D cavesGeneration = new Texture2D(16, WorldHeight);
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < WorldHeight; y++)
                {
                    float xPerlin = (x + perlinOffsetX) * perlinFreq;
                    float yPerlin = (y + perlinOffsetY) * perlinFreq;
                    Color px = new Color(0f, 0f, 0f, Mathf.PerlinNoise(xPerlin, yPerlin));
                    cavesGeneration.SetPixel(x, y, px);
                }
            }
            cavesGeneration.Apply();

            int cavesIdx = 0;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < WorldHeight; y++)
                {
                    cavesIdx = (((i * 16) + x) * WorldHeight) + y;

                    if (cavesGeneration.GetPixel(x, y).a < 0.25f && y < floorUndergroundEnd * Random.Range(0.9f, 0.8f) && y > 2)
                    {
                        buildedMapGrid[cavesIdx] = 0;
                        if (Random.Range(0, (WorldHeight - y) / 2) == 0)
                        {
                            buildedMapGrid[cavesIdx] = 62;
                        }
                    }

                }
            }

            perlinOffsetX += 16;
        }


        //STRUCTURES
        {
            
        }

        DataSaver.CreateTxt(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/mapBiomes.lgrsd", mapBiomes);
        DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(buildedMapGrid), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/map.lgrsd");

        int[] sg = { 22, 23, 24, 0, 0, 0, 0, 0, 16 };
        int[] sa = { 1, 1, 1, 0, 0, 0, 0, 0, 1 };
        int[] ig = new int[36];
        int[] ia = new int[36];
        int[] wp = { WorldWidth, WorldHeight };

        DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(sg), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/sbg.lgrsd");
        DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(sa), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/sba.lgrsd");
        DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(ig), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/ig.lgrsd");
        DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(ia), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/ia.lgrsd");
        DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(wp), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/wp.lgrsd");
        DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(equipedArmor), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/equips.lgrsd");
        DataSaver.CreateTxt(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/seed.lgrsd", new string[] { seed + "" });

        allMapGrid = buildedMapGrid;
        return buildedMapGrid;
    }

    int[] LoadMapGrid(string worldName)
    {
        WorldWidth = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/wp.lgrsd").SavedData)[0];
        WorldHeight = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/wp.lgrsd").SavedData)[1];

        int[] buildedMapGrid = new int[(WorldWidth * 16) * WorldHeight];

        buildedMapGrid = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/map.lgrsd").SavedData);

        return buildedMapGrid;
    }

    void GenerateAllChunks(int[] mapToGenerate)
    {
        Debug.Log("===STARTED MAP GENERATION===");
        string[] worldBiomes;

        if (!isNetworkClient)
            worldBiomes = DataSaver.ReadTxt(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/mapBiomes.lgrsd");
        else
            worldBiomes = Client.worldBiomesLoad;

        allMapGrid = mapToGenerate;
        int lastSpawnedIdx = 0;

        for (int e = 0; e < WorldWidth; e++)
        {
            SpawnChunk(e, System.Convert.ToSingle("0." + e), lastSpawnedIdx, e, worldBiomes[e], e * 16);
            lastSpawnedIdx += 16 * WorldHeight;
        }

        GameChunks = new GameObject[chunkContainer.transform.childCount];
        for (int i = 0; i < chunkContainer.transform.childCount; i++)
        {
            GameChunks[i] = chunkContainer.transform.GetChild(i).gameObject;
            chunkContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        Debug.Log("===ENDED MAP GENERATION===");
        Debug.Log("");
    }

    void SpawnChunk(int chunkX, float chunkIdx, int spawnedIdxs, int childId, string chunkBiome, int orgXpos)
    {
        float tileName = 0 + chunkIdx;
        int tileIdx = 0;
        GameObject newChunk = Instantiate(emptyChunk, new Vector3(chunkX * 16, 0, 0), Quaternion.identity);
        newChunk.name = "Chunk" + chunkIdx;
        newChunk.GetComponent<ChunkController>().CreateChunk(WorldHeight, chunkIdx, childId, chunkBiome, orgXpos);
        newChunk.GetComponent<ChunkController>().chunkColor = ManagingFunctions.HexToColor(currentHexPlanetColor);
        newChunk.transform.parent = chunkContainer.transform;

        for (int eX = 0; eX < 16; eX++)
        {
            for (int e = 0; e < WorldHeight; e++)
            {
                int tileSet = 0;
                tileSet = allMapGrid[spawnedIdxs + tileIdx];
                newChunk.GetComponent<ChunkController>().TileGrid[tileIdx] = tileSet;
                tileIdx++;
            }
        }
    }

    public IEnumerator ManageTransition(string variable, bool managingAction, float wait)
    {
        yield return new WaitForSeconds(wait);
        Transition.GetComponent<Animator>().SetBool(variable, managingAction);
        Transition.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
    }

    IEnumerator CheckChunks()
    {
        UpdateChunksActive();

        while (true)
        {
            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForSeconds(3);
                UpdateChunksActive();
                entitiesContainer.GetComponent<EntitiesManager>().UpdateEntities(LoadedChunks);
                LightController.lightController.AddRenderQueue(Camera.main.transform.position);
            }
            SaveGameData(true);
        }
    }

    public void UpdateChunksActive()
    {
        List<GameObject> gameChunksLoaded = new List<GameObject>();
        List<GameObject> gameChunksToLoad = new List<GameObject>();
        List<GameObject> previousChunks = new List<GameObject>(LoadedChunks);
        MenuController menuController = GameObject.Find("MenuFunctions").GetComponent<MenuController>();

        int chunksActived = 0;
        int chunksDeactived = 0;
        bool firstLimitPlaced = false;

        int playerChunk = (int)Mathf.Floor(player.transform.position.x / 16f);

        for(int i = playerChunk - menuController.chunksOnEachSide; i < playerChunk + menuController.chunksOnEachSide; i++)
        {
            int chunkToLoad = i;
            if (chunkToLoad < 0)
            {
                chunkToLoad += WorldWidth;
            }
            if (chunkToLoad >= WorldWidth)
            {
                chunkToLoad -= WorldWidth;
            }
            
            gameChunksToLoad.Add(chunkContainer.transform.GetChild(chunkToLoad).gameObject);
        }

        foreach (GameObject chunk in previousChunks)
        {
            if (!gameChunksToLoad.Contains(chunk) && chunk.activeInHierarchy)
            {
                chunk.SetActive(false);
                chunksDeactived++;
            }
        }

        foreach (GameObject chunk in gameChunksToLoad)
        {
            if (!firstLimitPlaced)
            {
                chunksLimits.GetChild(0).transform.position = new Vector2(chunk.transform.position.x, WorldHeight / 2);
                chunksLimits.GetChild(0).GetComponent<BoxCollider2D>().size = new Vector2(1, WorldHeight + 4);
                firstLimitPlaced = true;
            }

            if (!previousChunks.Contains(chunk) && !chunk.activeInHierarchy)
            {
                chunk.SetActive(true);
                chunk.GetComponent<ChunkController>().UpdateChunk();
                chunksActived++;
            }

            chunksLimits.GetChild(1).transform.position = new Vector2(chunk.transform.position.x + 16, WorldHeight / 2);
            chunksLimits.GetChild(1).GetComponent<BoxCollider2D>().size = new Vector2(1, WorldHeight + 4);

            gameChunksLoaded.Add(chunk);

        }

        LoadedChunks = gameChunksLoaded.ToArray();
        UpdateChunksRelPos();
    }

    public void UpdateChunksRelPos()
    {
        foreach(GameObject chunk in LoadedChunks)
        {
            chunk.GetComponent<ChunkController>().UpdateChunkPos();
        }
    }

    public void ChangeBrush(int entryTile, GameObject tile)
    {
        if (building && !cancelPlacing)
        {
            if (tileDefaultBrokeTool[entryTile] == "replace" && chosenBrush != tile.transform.parent.GetComponent<ChunkController>().TileGrid[System.Array.IndexOf(tile.transform.parent.GetComponent<ChunkController>().TileObject, tile)])
            {
                brush = chosenBrush;
                StackBar.LoseItem();
                if (tile.GetComponent<Timer>() != null) Destroy(tile.GetComponent<Timer>());
                if (tile.GetComponent<BlockAnimationController>() != null) Destroy(tile.GetComponent<BlockAnimationController>());
                tile.transform.parent.GetComponent<ChunkController>().TileGrid[System.Array.IndexOf(tile.transform.parent.GetComponent<ChunkController>().TileObject, tile)] = brush;
                tile.transform.parent.GetComponent<ChunkController>().TileGridRotation[System.Array.IndexOf(tile.transform.parent.GetComponent<ChunkController>().TileObject, tile)] = buildRotation;
                tile.transform.parent.GetComponent<ChunkController>().UpdateChunk();
                LightController.lightController.AddRenderQueue(player.transform.position);

                if(isNetworkClient || isNetworkHost)
                {
                    NetworkController.networkController.UpdateBlock(tile.transform.parent.GetSiblingIndex(), tile.transform.GetSiblingIndex(), brush);
                }
            }
            else
            {
                brush = entryTile;
            }
        }

        if (usingTool && !cancelPlacing)
        {
            tileBreaking = entryTile;

            if (entryTile >= 0)
            {
                if (lastTileBrush == null)
                {
                    lastTileBrush = tile;
                }
                if (breakingTime == -1 && toolUsing == tileDefaultBrokeTool[entryTile])
                {
                    breakingTime = 100;
                }
                else if (toolUsing == tileDefaultBrokeTool[entryTile] && lastTileBrush == tile)
                {
                    breakingTime -= (toolUsingEfficency - ToolEfficency[entryTile]);
                }
                else
                {
                    breakingTime = -1;
                    tileBreaking = -1;
                }
            }


            if (breakingTime <= 0 && breakingTime != -1)
            {
                breakingTime = 100;

                if (toolUsing == "pickaxe" && tileDefaultBrokeTool[entryTile] == "pickaxe")
                {
                    brush = 0;
                    if (tile.GetComponent<Timer>() != null) Destroy(tile.GetComponent<Timer>());
                    if (tile.GetComponent<BlockAnimationController>() != null) Destroy(tile.GetComponent<BlockAnimationController>());
                }

                else if (toolUsing == "axe" && tileDefaultBrokeTool[entryTile] == "axe")
                {
                    brush = 0;
                    if (tile.GetComponent<Timer>() != null) Destroy(tile.GetComponent<Timer>());
                    if (tile.GetComponent<BlockAnimationController>() != null) Destroy(tile.GetComponent<BlockAnimationController>());
                }

                else if (toolUsing == "shovel" && tileDefaultBrokeTool[entryTile] == "shovel")
                {
                    brush = 0;
                    if (tile.GetComponent<Timer>() != null) Destroy(tile.GetComponent<Timer>());
                    if (tile.GetComponent<BlockAnimationController>() != null) Destroy(tile.GetComponent<BlockAnimationController>());
                }

                else if (toolUsing == "hoe" && tileDefaultBrokeTool[entryTile] == "hoe")
                {
                    brush = 0;
                    if (tile.GetComponent<Timer>() != null) Destroy(tile.GetComponent<Timer>());
                    if (tile.GetComponent<BlockAnimationController>() != null) Destroy(tile.GetComponent<BlockAnimationController>());
                }

                else
                {
                    brush = entryTile;
                }

                ManagingFunctions.DropItem(SwitchTroughBlockBroke(entryTile, tile.transform.position), tile.transform.position);
                tile.transform.parent.GetComponent<ChunkController>().TileGrid[System.Array.IndexOf(tile.transform.parent.GetComponent<ChunkController>().TileObject, tile)] = brush;
                tile.transform.parent.GetComponent<ChunkController>().UpdateChunk();
                LightController.lightController.AddRenderQueue(player.transform.position);

                if (isNetworkClient || isNetworkHost)
                {
                    NetworkController.networkController.UpdateBlock(tile.transform.parent.GetSiblingIndex(), tile.transform.GetSiblingIndex(), brush);
                }
            }
            else
            {
                brush = entryTile;
            }
        }

        if (usingArm)
        {
            brush = entryTile;
        }

        lastTileBrush = tile;
    }

    public int SwitchTroughBlockBroke(int entryTile, Vector2 tileEntryPos)
    {
        int returnItem = entryTile;
        Vector2 tilePos = tileEntryPos;
        tilePos.x += 0.15f;

        switch (entryTile)
        {
            case 1:
                returnItem = 7;
                break;
            case 2:
                returnItem = 7;
                if (Random.Range(0, 5) == 0)
                    ManagingFunctions.DropItem(3, tilePos);
                break;
            case 4:
                returnItem = 7;
                break;
            case 5:
                returnItem = 7;
                break;
            case 8:
                returnItem = 31;
                ManagingFunctions.DropItem(6, tilePos);
                break;
            case 9:
                returnItem = 33;
                ManagingFunctions.DropItem(6, tilePos);
                break;
            case 10:
                returnItem = 32;
                ManagingFunctions.DropItem(6, tilePos);
                break;
            case 11:
                returnItem = 30;
                ManagingFunctions.DropItem(6, tilePos);
                break;
            case 12:
                returnItem = 34;
                ManagingFunctions.DropItem(6, tilePos);
                break;
            case 55:
                if (Random.Range(0, 99) == 0) returnItem = 51;
                else returnItem = 0;
                break;
            case 83:
                returnItem = 82;
                break;
            case 86:
                returnItem = 0;
                break;
            case 87:
                returnItem = 0;
                break;
        }


        return returnItem;
    }

    public int SearchForClearSpawn(int x)
    {
        for (int i = WorldHeight - 2; i > 0; i--)
        {
            if (allMapGrid[(x * WorldHeight) + i] != 0 && allMapGrid[(x * WorldHeight) + i + 1] == 0)
            {
                return i;
            }
        }

        return WorldHeight + 1;
    }

    public int GetTileAt(int idx)
    {
        try
        {
            if (idx < 0)
            {
                idx += allMapGrid.Length;
            }
            if (idx > allMapGrid.Length - 1)
            {
                idx -= allMapGrid.Length;
            }
            ChunkController chunk = chunkContainer.transform.GetChild((int)ManagingFunctions.EntireDivision(idx, WorldHeight * 16).cocient).GetComponent<ChunkController>();

            return chunk.TileGrid[idx - chunk.tilesToChunk];
        }
        catch
        {
            return -1;
        }
    }


    public GameObject GetTileObjectAt(int idx)
    {
        try
        {
            if (idx < 0)
            {
                idx += allMapGrid.Length;
            }
            if (idx > allMapGrid.Length - 1)
            {
                idx -= allMapGrid.Length;
            }
            ChunkController chunk = chunkContainer.transform.GetChild((int)ManagingFunctions.EntireDivision(idx, WorldHeight * 16).cocient).GetComponent<ChunkController>();

            return chunk.TileObject[idx - chunk.tilesToChunk];
        }
        catch
        {
            return null;
        }
    }

    public void Consume(int tile)
    {
        switch (tile)
        {
            case 78:
                if (dayLuminosity < 0.5f && entitiesContainer.GetComponentInChildren<ENTITY_TheDestroyer>() == null)
                {
                    ENTITY_TheDestroyer.StaticSpawn(null, new Vector2(player.transform.position.x + Random.Range(-10f, 10f), 0));
                    soundController.PlaySfxSound(SoundName.select);
                }
                else StackBar.AddItem(78);
                break;
        }
    }
}