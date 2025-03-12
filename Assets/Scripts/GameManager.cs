﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public enum Difficulty : int
{
    normal = 1, hard = 2, extreme = 3,
}

public enum Biomes : int
{
    meadow = 0, forest = 1, mountain = 2, ocean = 3,
}

public enum Entities : int
{
    NanoBotT1 = 0, Krotek = 1, TheDestroyer = 2, NanoBotT2 = 3, Raideon = 4, NanoBotT3 = 5, Darkn = 6, NanoBotT5 = 7, Sheep = 8, Cow = 9, NanoBotT4 = 10, x
}
public enum UnitWeapons : int
{
    Shooter = 0, LaserDrill = 1,
}

public enum Projectiles : int
{
    Arrow = 0, SwordParticle = 1, Laser = 2, PlasmaBomb = 3,
}

public class GameManager : MonoBehaviour
{
    public static GameManager gameManagerReference;
    public static event System.Action<int> OnWorldRounding;
    [SerializeField] private bool inGame = false;
    [SerializeField] private bool playerFocused = false;
    public int seed = 0;
    public float frameTimerAdder = 0f;
    public int frameTimer = 0;
    public bool addedFrameThisFrame = false;
    public Difficulty gameDifficulty = Difficulty.normal;

    public GameObject[] EntitiesGameObject;
    public GameObject[] ProjectilesGameObject;
    public GameObject[] GameChunks;
    public List<GameObject> LoadedChunks;
    public Transform chunksLimits;
    public Transform dummyObjects;

    [Header("Tiles Config")]
    [SerializeField] public Sprite[] tiles;
    [SerializeField] public Sprite[] tileBuildPrev;
    [SerializeField] public string[] tileType;
    [SerializeField] public int[] TileCollisionType;
    [SerializeField] public string[] tileMainProperty;
    [SerializeField] public string[] tileDefaultBrokeTool;
    [SerializeField] public int[] stackLimit;
    [SerializeField] public int[] ToolEfficency;
    [SerializeField] public string[] tileName;
    [SerializeField] public Color[] rawColor;
    [SerializeField] public BlockAnimationData[] tileAnimation;
    [SerializeField] public Vector4[] tileSize;
    [SerializeField] public AudioClip[] breakSound;
    [SerializeField] public PlayerController player;
    [SerializeField] public MainSoundController soundController;
    [SerializeField] public int WorldHeight;
    [SerializeField] public int WorldWidth;
    [SerializeField] public GameObject emptyTile;
    [SerializeField] public GameObject lecterSprite;
    [SerializeField] public GameObject emptySprite;
    [SerializeField] GameObject emptyChunk;
    [SerializeField] public GameObject chunkContainer;
    [SerializeField] public GameObject entitiesContainer;
    [SerializeField] GameObject Transition;
    [SerializeField] GameObject savingText;
    [SerializeField] AudioClip[] surfaceOST;
    [SerializeField] AudioClip[] caveOST;

    [Header("Other")]
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

    public Vector3 mouseDelta;
    public int NumberOfTilesInChunk;

    public int[] equipedArmor = new int[6];

    //Auxiliar for build
    GameObject lastTileBrush;
    public int breakingTime = -1;
    public int tileBreaking = -1;

    //The map container
    public int[] allMapGrid;
    public int[] allBackgroundGrid;
    public string[] allMapProp;
    public string currentPlanetName = "Korenz";
    public string currentHexPlanetColor = "25FF00FF";
    public bool currentPlanetIsRoot = true;

    //Events and game data
    public Color skyboxColor;
    public Color daytimeUpdatedSkyboxColor;
    public AnimationCurve dayNightCycle;
    public float dayLuminosity = 0;
    public string worldName = "null";
    public string worldRootName = "null";
    public bool isLorePlanet = true;
    public bool isSavingData = false;
    public string persistentDataPath = "null";
    public int tileSpawnRate = 0;


    //Lists
    List<GameObject> poolTiles = new List<GameObject>();
    public List<Vector2> platformDrops = new List<Vector2>();
    public List<Vector2> buoyancyFloats = new List<Vector2>();

    public SkinContainer[] playerSkins;

    [Header("Events")]
    public bool raining = false;
    public AudioSource rainSource;
    public float ostCooldown = 124f;
    public AudioSource ostSource;
    public AudioSource breakSoundSource;
    public float breakSoundCooldown = 0f;
    public AudioLowPassFilter audioPassFilter;

    public ArmorBarController armorBarController;
    public GameObject damageTextPrefab;

    public Transform celestialBodies;

    public bool InGame
    {
        get { return inGame; }
        set
        {
            inGame = value;
            if (isNetworkClient || isNetworkHost) inGame = true;

            UpdateEntitiesRB2D(inGame);
        }
    }

    public void UpdateEntitiesRB2D(bool value)
    {
        Rigidbody2D[] rigidbodys = entitiesContainer.GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rigidbody2D in rigidbodys)
        {
            if (rigidbody2D.simulated != value)
                rigidbody2D.simulated = value;
        }

        rigidbodys = dummyObjects.GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rigidbody2D in rigidbodys)
        {
            if (rigidbody2D.simulated != value)
                rigidbody2D.simulated = value;
        }
    }

    private void Awake()
    {
        gameManagerReference = this;
        OnWorldRounding = delegate { };

        worldName = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0];
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

        persistentDataPath = Application.persistentDataPath;
    }

    void Start()
    {
        skyboxColor = Camera.main.backgroundColor;
        Time.timeScale = 1.0f;
        soundController = GameObject.Find("Audio").GetComponent<MainSoundController>();

        if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("isNetworkLoad")[0] == "true") isNetworkClient = true;
        if (Server.isOpen) isNetworkHost = true;



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
                    int random = Random.Range(-99999999, 999999999);
                    Random.InitState(random);
                    seed = random;
                }
            }
            else if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldLoadType")[0] == "newPlanet")
            {
                currentHexPlanetColor = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("planetColor")[0];
                currentPlanetName = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("planetName")[0];
                isLorePlanet = false;

                seed = Random.Range(-99999999, 999999999);
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

            if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldLoadType")[0] != "newWorld")
                if (DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + worldRootName + @"/difficulty.lgrsd"))
                {
                    gameDifficulty = (Difficulty)ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldRootName + @"/difficulty.lgrsd").SavedData)[0];
                }
                else
                {
                    DataSaver.SaveStats(new string[] { "1" }, Application.persistentDataPath + @"/worlds/" + worldRootName + @"/difficulty.lgrsd");
                }
            else
            {
                gameDifficulty = (Difficulty)ManagingFunctions.ConvertStringToIntArray(GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("newWorldDifficulty"))[0];
            }
        }
        else
        {
            gameDifficulty = Client.difficulty;
        }

        PlanetMenuController.planetMenu = MenuController.menuController.canvas.transform.Find("PlanetsPanel").GetComponent<PlanetMenuController>();

        isLorePlanet = PlanetMenuController.planetMenu.lorePlanets.Contains(currentPlanetName);

        InventoryBar.Reset();
        StackBar.Reset();


        //h += 0.19166666666f;
        if (!isLorePlanet)
        {
            skyboxColor = ManagingFunctions.HexToColor(currentHexPlanetColor);
            Color.RGBToHSV(skyboxColor, out float h, out float s, out float v);
            v -= 0.2f;
            skyboxColor = Color.HSVToRGB(h, s, v);
            skyboxColor.a = 0f;

        }
        else
        {
            skyboxColor = PlanetMenuController.planetMenu.lorePlanetsColor[PlanetMenuController.planetMenu.lorePlanets.IndexOf(currentPlanetName)];
        }


        armorBarController.Initialize(equipedArmor);

        frameTimer = 0;
        Transition.SetActive(true);

        //Generate all
        GenerateAllChunks(GenerateMap());
        PlanetMenuController.planetMenu.Startt();

        Vector2 respawnPosition = Vector2.zero;


        if (DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + worldName + @"/spawnpoint.lgrsd"))
        {
            string[] pos = DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/spawnpoint.lgrsd").SavedData[0].Split(';');
            respawnPosition = new Vector2(System.Convert.ToSingle(pos[0]), System.Convert.ToSingle(pos[1]));
        }
        else
        {
            respawnPosition = RespawnPosition();
        }

        if (GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldLoadType")[0] == "newWorld")
        {
            player.Respawn(respawnPosition.x, respawnPosition.y);
            SetSkybox();

            while (dayLuminosity > 1f)
            {
                Debug.Log("a" + dayLuminosity);
                PlanetMenuController.planetMenu.Simulate(0.25f, false);
                SetSkybox();
            }

            while (dayLuminosity < 1f)
            {
                Debug.Log("b" + dayLuminosity);
                PlanetMenuController.planetMenu.Simulate(0.25f, false);
                SetSkybox();
            }


            player.transform.position = new Vector2(respawnPosition.x, WorldHeight);
            player.GetComponent<Rigidbody2D>().velocity = Vector2.right * -5;
            player.PutMyselfInATragicTerminalColisionCourse();
        }
        else
            player.Respawn(respawnPosition.x, respawnPosition.y + 1);

        inGame = true;
        playerFocused = true;
        StartCoroutine(CheckChunks());
        Transition.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Done!";
        StartCoroutine(ManageTransition("CanStart", true, 1f));
        Invoke("UpdateChunksActive", 0.1f);
        GameObject.Find("IlluminationCape").GetComponent<LightControllerCurrent>().AddRenderQueue(player.transform.position);

        if (!isNetworkClient)
            LoadEntities();

        try
        {
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("connectionViaAntenna");
            StaticController.self.Trigger(1.5f, false);
        }
        catch
        {

        }
    }


    void Update()
    {
        mouseCurrentPosition = Camera.main.ScreenToWorldPoint(GInput.MousePosition);
        addedFrameThisFrame = false;

        if (inGame || PlanetMenuController.planetMenu.gameObject.activeInHierarchy)
        {
            if (!isNetworkClient)
            {
                if (PlanetMenuController.planetMenu.gameObject.activeInHierarchy)
                {
                    if (GInput.GetKey(KeyCode.Comma))
                        if (PlanetMenuController.planetMenu.timewarp < 576869)
                            PlanetMenuController.planetMenu.timewarp += PlanetMenuController.planetMenu.timewarp * Time.deltaTime * 2;
                        else
                            PlanetMenuController.planetMenu.timewarp = 576870;
                    else if (PlanetMenuController.planetMenu.timewarp > 1)
                        PlanetMenuController.planetMenu.timewarp -= PlanetMenuController.planetMenu.timewarp * Time.deltaTime * 5;

                    if (PlanetMenuController.planetMenu.timewarp < 1)
                        PlanetMenuController.planetMenu.timewarp = 1;
                }
                else
                {
                    PlanetMenuController.planetMenu.timewarp = 1;
                }

                PlanetMenuController.planetMenu.Simulate(Time.deltaTime / 255f * PlanetMenuController.planetMenu.timewarp, PlanetMenuController.planetMenu.gameObject.activeInHierarchy);
            }

            if (frameTimer > 2147483640)
            {
                frameTimer = 0;
            }
            SetSkybox();
        }

        if (inGame)
        {
            audioPassFilter.enabled = false;

            if (ostCooldown > 0f)
            {
                ostCooldown -= Time.deltaTime;
            }

            if (breakSoundCooldown > 0f)
            {
                breakSoundCooldown -= Time.deltaTime;
            }

            frameTimerAdder += Time.deltaTime;

            if (frameTimerAdder >= 0.016665f)//aprox 1 frame duration on 60FPS
            {
                while (frameTimerAdder >= 0.016665f)
                {
                    frameTimerAdder -= 0.016665f;
                }
                addedFrameThisFrame = true;
                frameTimer++;
            }



            if (Input.GetMouseButtonUp(0))
            {
                breakingTime = -1;
                tileBreaking = -1;
            }

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

                    int equipPiece = StackBar.stackBarController.CurrentItem;

                    int equipReturned = EquipItem(equipPiece, equipType);

                    if (equipReturned > 0)
                    {
                        StackBar.stackBarController.StackBarGrid[StackBar.stackBarController.idx] = equipReturned;
                        StackBar.stackBarController.UpdateStacks();
                    }
                    else
                    {
                        StackBar.LoseItem();
                    }
                }
                if (player.alive && canConsume)
                {
                    Consume(StackBar.stackBarController.CurrentItem);
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

            if (GInput.GetKeyDown(KeyCode.F2))
            {
                int freeFile = 1;

                for (int newFreeFile = 1; DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/screenshots/screenshot" + newFreeFile + ".png"); newFreeFile++)
                {
                    freeFile++;
                }
                ScreenCapture.CaptureScreenshot(Application.persistentDataPath + @"/screenshots/screenshot" + freeFile + ".png");
            }

            //EVENTS

            if (raining)
            {
                if (!rainSource.isPlaying)
                {
                    rainSource.Play();
                }
            }
            else
            {
                if (rainSource.isPlaying)
                {
                    rainSource.Stop();
                }
            }
        }
        else
        {
            audioPassFilter.enabled = true;
        }

        savingText.SetActive(isSavingData);


        ostSource.volume += Time.deltaTime;

        UpdateEntitiesRB2D(InGame);


        int playerChunk = (int)Mathf.Floor(player.transform.position.x / 16f);
        chunksLimits.GetChild(0).position = new Vector2((playerChunk - MenuController.menuController.chunksOnEachSide) * 16, WorldHeight / 2);
        chunksLimits.GetChild(1).position = new Vector2((playerChunk + MenuController.menuController.chunksOnEachSide) * 16, WorldHeight / 2);
    }

    public void PlayOST(string type)
    {
        if (ostCooldown <= 0f)
        {
            if(type == "surface")
            {
                AudioClip clip = surfaceOST[Random.Range(0, surfaceOST.Length)];

                ostSource.Stop();
                ostSource.clip = clip;
                ostSource.Play();

                ostCooldown = clip.length * Random.Range(5, 15);
            }
            else if (type == "cave")
            {
                AudioClip clip = caveOST[Random.Range(0, caveOST.Length)];

                ostSource.Stop();
                ostSource.clip = clip;
                ostSource.Play();

                ostCooldown = clip.length * Random.Range(5, 15);
            }

        }
    }

    public void PlayBreakSound(int block, bool ignoreCooldown = false)
    {
        if (block != -1)
            if (breakSoundCooldown <= 0f || ignoreCooldown)
            {
                AudioClip clip = breakSound[block];

                if (clip != null)
                {
                    breakSoundSource.Stop();
                    breakSoundSource.clip = clip;
                    breakSoundSource.pitch = Random.Range(0.85f, 1.15f);
                    breakSoundSource.Play();

                    breakSoundCooldown = clip.length * breakSoundSource.pitch + 0.01f;
                }
            }
    }

    public int EquipItem(int equipPiece, string type)
    {
        int idx = 0;

        if (type == "helmet")
        {
            idx = 0;
        }
        else if (type == "chestplate")
        {
            idx = 1;
        }
        else if (type == "boots")
        {
            idx = 2;
        }
        else if (type == "helmetAcc")
        {
            idx = 3;
        }
        else if (type == "chestplateAcc")
        {
            idx = 4;
        }
        else if (type == "bootsAcc")
        {
            idx = 5;
        }


        int returnVal = equipedArmor[idx];

        equipedArmor[idx] = equipPiece;

        return returnVal;
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
            string[] data = GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("resources");
            GameObject.Find("SaveObject").GetComponent<ComponetSaver>().DeleteData("resources");

            string resources = data[0];
            Debug.Log(resources);

            if(resources != "null")
            {
                foreach (string resource in resources.Split(';'))
                {
                    if (resource != "")
                    {
                        int tile = System.Convert.ToInt32(resource.Split(':')[0]);
                        int tileAmount = System.Convert.ToInt32(resource.Split(':')[1]);

                        for (int e = 0; e < tileAmount; e++)
                        {
                            if (!StackBar.AddItemInv(tile))
                            {
                                ManagingFunctions.DropItem(tile, player.transform.position);
                            }
                        }
                    }
                }
            }

            CoreReentryController.self.StartAnimation(System.Convert.ToInt32(data[1]));
        }
        catch
        {
            Debug.Log("==DIDNT IMPORT RESOURCES==");
            Destroy(CoreReentryController.self.gameObject);
        }
    }

    public void LoadEntities()
    {
        if (DataSaver.CheckIfFileExists(persistentDataPath + @"/worlds/" + worldName + @"/ent.lgrsd"))
        {
            string[] entities = DataSaver.LoadStats(persistentDataPath + @"/worlds/" + worldName + @"/ent.lgrsd").SavedData;

            foreach (string entity in entities)
            {
                string[] datas = entity.Split(';');

                Vector2 spawnPos = new Vector2(System.Convert.ToSingle(datas[2]), System.Convert.ToSingle(datas[3]));
                int entityHp = System.Convert.ToInt32(datas[1]);
                string entityType = datas[0];
                string[] args = null;
                if (datas.Length > 4)
                    args = datas[4].Split('#');

                EntityBase entityBase = null;

                entityBase = SpawnEntity(StringToEntity(entityType), args, spawnPos);

                if (entityBase != null)
                {
                    entityBase.Hp = entityHp;
                    if(entityBase.EntityCommonScript != null)
                    {
                        entityBase.EntityCommonScript.saveToFile = true;
                    }
                }
            }
        }

        if (DataSaver.CheckIfFileExists(persistentDataPath + @"/worlds/" + worldName + @"/dro.lgrsd"))
        {
            string[] drops = DataSaver.LoadStats(persistentDataPath + @"/worlds/" + worldName + @"/dro.lgrsd").SavedData;
            foreach (string drop in drops)
            {
                string[] datas = drop.Split(';');

                Vector2 spawnPos = new Vector2(System.Convert.ToSingle(datas[2]), System.Convert.ToSingle(datas[3]));
                Vector2 spawnVelocity = new Vector2(System.Convert.ToSingle(datas[4]), System.Convert.ToSingle(datas[5]));
                int item = System.Convert.ToInt32(datas[0]);
                int amount = System.Convert.ToInt32(datas[1]);
                float imunityGrab = System.Convert.ToSingle(datas[6]);

                ManagingFunctions.DropItem(item, spawnPos, spawnVelocity, amount, imunityGrab);
            }
        }
    }

    public string EntityToString(Entities entity)
    {
        switch (entity)
        {
            case Entities.NanoBotT1:
                return "nanobot1";
            case Entities.NanoBotT2:
                return "nanobot2";
            case Entities.NanoBotT3:
                return "nanobot3";
            case Entities.NanoBotT5:
                return "nanobot5";
            case Entities.TheDestroyer:
                return "destroyer";
            case Entities.Krotek:
                return "krotek";
            case Entities.Darkn:
                return "darkn";
            case Entities.Raideon:
                return "raideon";
            case Entities.Sheep:
                return "sheep";
            default:
                return "null";
        }
    }

    public Entities StringToEntity(string entity)
    {
        switch (entity)
        {
            case "nanobot1":
                return Entities.NanoBotT1;
            case "nanobot2":
                return Entities.NanoBotT2;
            case "nanobot3":
                return Entities.NanoBotT3;
            case "nanobot5":
                return Entities.NanoBotT5;
            case "destroyer":
                return Entities.TheDestroyer;
            case "krotek":
                return Entities.Krotek;
            case "darkn":
                return Entities.Darkn;
            case "raideon":
                return Entities.Raideon;
            case "sheep":
                return Entities.Sheep;
            default:
                return Entities.x;
        }
    }

    public EntityBase SpawnEntity(Entities entityType, string[] args, Vector2 spawnPos)
    {
        EntityBase entityBase = null;

        if (entityType == Entities.NanoBotT1)
        {
            entityBase = ENTITY_NanoBotT1.StaticSpawn(args, spawnPos);
        }
        else if (entityType == Entities.NanoBotT2)
        {
            entityBase = ENTITY_NanoBotT2.StaticSpawn(args, spawnPos);
        }
        else if (entityType == Entities.NanoBotT3)
        {
            entityBase = ENTITY_NanoBotT3.StaticSpawn(args, spawnPos);
        }
        else if (entityType == Entities.NanoBotT5)
        {
            entityBase = ENTITY_NanobotT5.StaticSpawn(args, spawnPos);
        }
        else if (entityType == Entities.Darkn)
        {
            entityBase = UNIT_Darkn.StaticSpawn(args, spawnPos);
        }
        else if (entityType == Entities.TheDestroyer)
        {
            entityBase = ENTITY_TheDestroyer.StaticSpawn(args, spawnPos);
        }
        else if (entityType == Entities.Krotek)
        {
            entityBase = ENTITY_KrotekController.StaticSpawn(args, spawnPos);
        }
        else if (entityType == Entities.Raideon)
        {
            entityBase = ENTITY_Raideon.StaticSpawn(args, spawnPos);
        }
        else if (entityType == Entities.Sheep)
        {
            entityBase = ENTITY_Sheep.StaticSpawn(args, spawnPos);
        }

        //entityBase.IsLocal = !isNetworkClient;
        return entityBase;
    }

    private void SetSkybox()
    {
        Vector2 currentPlanetPos = PlanetMenuController.currentPlanet.physicalPlanet.anchoredPosition / PlanetMenuController.planetMenu.planetPanelRectTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;

        float angle = ManagingFunctions.PointToPivotUp(currentPlanetPos, PlanetMenuController.planetMenu.planets[0].physicalPlanet.anchoredPosition / PlanetMenuController.planetMenu.planetPanelRectTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x);
        angle -= PlanetMenuController.currentPlanet.physicalPlanet.GetChild(1).GetChild(1).eulerAngles.z;
        angle -= player.mainCamera.transform.position.x / (WorldWidth * 16) * 360;
        angle += 90;

        angle %= 360;
        if (angle < 0) angle += 360;
        angle = angle / 360;

        float value = dayNightCycle.Evaluate(angle);


        dayLuminosity = Mathf.Clamp(value, 0.1f, 1);

        if (raining)
            dayLuminosity = Mathf.Clamp(value, 0.1f, 0.25f);


        daytimeUpdatedSkyboxColor = skyboxColor * dayLuminosity;

        for (int i = 0; i < PlanetMenuController.planetMenu.planets.Count; i++)
        {
            if (PlanetMenuController.planetMenu.planets[i].planetName != currentPlanetName)
            {
                GameObject skyPlanet;

                if (i >= celestialBodies.childCount)
                {
                    skyPlanet = Instantiate(celestialBodies.GetChild(0).gameObject, celestialBodies);
                    skyPlanet.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = PlanetMenuController.planetMenu.planetSprite;
                    skyPlanet.transform.GetChild(0).GetComponent<SpriteRenderer>().color = PlanetMenuController.planetMenu.planets[i].planetColor.GetColor();
                }
                else
                {
                    skyPlanet = celestialBodies.GetChild(i).gameObject;
                }


                Vector2 skyPlanetPos = PlanetMenuController.planetMenu.planets[i].physicalPlanet.anchoredPosition / PlanetMenuController.planetMenu.planetPanelRectTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
                float planetAngle = ManagingFunctions.PointToPivotUp(currentPlanetPos, skyPlanetPos);

                planetAngle -= PlanetMenuController.currentPlanet.physicalPlanet.GetChild(1).GetChild(1).eulerAngles.z;
                planetAngle -= player.mainCamera.transform.position.x / (WorldWidth * 16) * 360;

                float planetDistance = Vector2.Distance(skyPlanetPos, currentPlanetPos);

                skyPlanet.transform.eulerAngles = Vector3.forward * planetAngle;
                skyPlanet.transform.GetChild(0).localPosition = new Vector3(0, 14, planetDistance);

                if (PlanetMenuController.planetMenu.planets[i].revolutionTime > 0)
                    skyPlanet.transform.GetChild(0).localEulerAngles = Vector3.forward * (float)(PlanetMenuController.planetMenu.planets[i].rTime / PlanetMenuController.planetMenu.planets[i].revolutionTime * 360);
                skyPlanet.transform.GetChild(0).localScale = Vector3.one * (PlanetMenuController.planetMenu.planets[i].chunkSize / (planetDistance * 25));
            }
            else
            {
                if (i >= celestialBodies.childCount)
                {
                    Instantiate(celestialBodies.GetChild(0).gameObject, celestialBodies);
                }
                celestialBodies.GetChild(i).gameObject.SetActive(false);
            }
        }
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
        return DataSaver.ReadTxt(persistentDataPath + @"/worlds/" + worldName + @"/mapBiomes.lgrsd");
    }

    public void SaveGameData(bool alternateThread)
    {
        if (!isSavingData)
        {
            EntityCommonScript[] entityCommonScripts = entitiesContainer.GetComponentsInChildren<EntityCommonScript>(true);

            List<string> entities = new List<string>();

            foreach (EntityCommonScript entity in entityCommonScripts)
            {
                if (entity.entityBase != null && entity.saveToFile)
                {
                    List<string> toEncrypt = new List<string>
                    {
                        entity.EntityType + ";",
                        entity.entityBase.Hp + ";",
                        entity.transform.position.x + ";",
                        entity.transform.position.y+ ";",
                    };
                    if (entity.entityBase != null)
                    {
                        string[] args = entity.entityBase.GenerateArgs();

                        if (args != null)
                            toEncrypt.Add(string.Join("#", entity.entityBase.GenerateArgs()));
                    }

                    entities.Add(string.Join("", toEncrypt.ToArray()));
                }
            }

            DroppedItemController[] drops = ManagingFunctions.dropContainer.GetComponentsInChildren<DroppedItemController>(true);

            List<string> dropsSaves = new List<string>();

            foreach (DroppedItemController drop in drops)
            {
                List<string> toEncrypt = new List<string>
                {
                     drop.item + "",
                     drop.amount + "",
                     drop.transform.position.x + "",
                     drop.transform.position.y + "",
                     drop.GetComponent<Rigidbody2D>().velocity.x + "",
                     drop.GetComponent<Rigidbody2D>().velocity.y + "",
                     drop.imunityGrab + ""
                };

                dropsSaves.Add(string.Join(";", toEncrypt.ToArray()));
            }

            string[] playerPosition;
            if (player.alive)
                playerPosition = new string[] { player.transform.position.x + ";" + player.transform.position.y };
            else
            {
                Vector2 respawnPos = RespawnPosition();
                playerPosition = new string[] { respawnPos.x + ";" + respawnPos.y };
            }
                

            if (alternateThread)
            {
                Thread mapSave = new Thread(new ThreadStart(() => SaveGameDataThread(entities.ToArray(), playerPosition, dropsSaves.ToArray())));
                mapSave.Start();
            }
            else
            {
                SaveGameDataThread(entities.ToArray(), playerPosition, dropsSaves.ToArray());
            }
        }
    }

    private void SaveGameDataThread(string[] entities, string[] playerPosition, string[] drops)
    {
        isSavingData = true;

        if (!isNetworkClient)
        {
            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(allMapGrid), persistentDataPath + @"/worlds/" + worldName + @"/map.lgrsd");
            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(allBackgroundGrid), persistentDataPath + @"/worlds/" + worldName + @"/bgmap.lgrsd");
            DataSaver.SaveStats(allMapProp, persistentDataPath + @"/worlds/" + worldName + @"/mapprop.lgrsd");
            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(equipedArmor), persistentDataPath + @"/worlds/" + worldName + @"/equips.lgrsd");
            DataSaver.SaveStats(new string[] { currentHexPlanetColor }, persistentDataPath + @"/worlds/" + worldName + @"/planetColor.lgrsd");
            DataSaver.SaveStats(new string[] { currentPlanetName }, persistentDataPath + @"/worlds/" + worldRootName + @"/lastLocation.lgrsd");
            DataSaver.SerializeAt(PlanetMenuController.planetMenu.planets, persistentDataPath + @"/worlds/" + worldRootName + @"/planets.lgrsd");

            DataSaver.SaveStats(drops, persistentDataPath + @"/worlds/" + worldName + @"/dro.lgrsd");
            DataSaver.SaveStats(entities, persistentDataPath + @"/worlds/" + worldName + @"/ent.lgrsd");
            DataSaver.SaveStats(playerPosition, persistentDataPath + @"/worlds/" + worldName + @"/spawnpoint.lgrsd");
            DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(TechManager.techTree.fullyUnlockedItems.ToArray()), persistentDataPath + @"/worlds/" + worldRootName + @"/tech.lgrsd");
            ManagingFunctions.SaveStackBarAndInventory();
            NodeManager.self.Save();
        }

        isSavingData = false;
    }


    int[] GenerateMap()
    {
        int[] mapGrid;

        if (isNetworkClient)
        {
            mapGrid = Client.worldMapLoad;
            allBackgroundGrid = Client.backgroundMapLoad;
            WorldHeight = Client.worldProportionsLoad[0];
            WorldWidth = Client.worldProportionsLoad[1];
            allMapProp = Client.worldMapPropLoad;
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

        DataSaver.SaveStats(new string[] { (int)gameDifficulty + "" }, Application.persistentDataPath + @"/worlds/" + worldName + @"/difficulty.lgrsd");

        int[] buildedMapGrid = new int[(WorldWidth * 16) * WorldHeight];
        allBackgroundGrid = new int[(WorldWidth * 16) * WorldHeight];
        string[] mapBiomes = new string[WorldWidth];
        string[] biomes = { "meadow", "forest", "mountain", "ocean" };
        float[] biomeProbability = { 10, 15, 5, 0 };
        int[] worldTileHeight = new int[WorldWidth * 16];

        float totalProbability = 0f;

        allMapProp = new string[buildedMapGrid.Length];
        for (int i = 0; i < allMapProp.Length; i++)
        {
            allMapProp[i] = "null";
        }

        foreach (float f in biomeProbability)
        {
            totalProbability += f;
        }

        float aaa = Random.Range(-9999f, 9999f);

        for (int i = 0; i < worldTileHeight.Length; i++)
        {
            worldTileHeight[i] = (int)((Mathf.PerlinNoise(i * 0.04f, aaa) * 0.4f + Mathf.PerlinNoise(i * 0.02f, aaa) * 0.6f) * (WorldHeight / 3)) + (WorldHeight / 2 /*0.5*/);
            if (i > worldTileHeight.Length - 16)
            {
                worldTileHeight[i] = (int)Mathf.Lerp(worldTileHeight[i], worldTileHeight[0], (i - (worldTileHeight.Length - 16f)) / 16f);
            }
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


        float perlinFreq = 0.08f;
        float perlinOffsetX = Random.Range(-9999f, 9999f);
        float perlinOffsetY = Random.Range(-9999f, 9999f);

        for (int i = 0; i < mapBiomes.Length; i++)
        {
            string currentBiome = mapBiomes[i];
            floorUndergroundEnd = worldTileHeight[i * 16] - 5;

            if (currentBiome == biomes[(int)Biomes.forest])
            {
                for (int i2 = 0; i2 < 16; i2++)
                {


                    if (worldTileHeight[i * 16 + i2] < WorldHeight * 0.66f)
                    {
                        floorSurfTile = 113;
                        floorUndergroundTile = 113;
                        floorUndergroundEnd = worldTileHeight[i * 16 + i2] - Random.Range(4, 8);
                    }
                    else
                    {
                        floorSurfTile = 1;
                        floorUndergroundTile = 7;
                        floorUndergroundEnd = worldTileHeight[i * 16 + i2] - Random.Range(3, 6);
                    }

                    for (int e = 0; e < WorldHeight; e++)
                    {
                        int idx = (((i * 16) + i2) * WorldHeight) + e;

                        if (e < 5)
                        {
                            if (Random.Range(0, e + 1) == 0)
                            {
                                buildedMapGrid[idx] = 13;
                                
                                if(e == 4 && Random.Range(0,2) == 0)
                                {
                                    if(buildedMapGrid[idx + 1] == 6)
                                    {
                                        buildedMapGrid[idx + 1] = 117;
                                    }
                                }
                            }
                            else if(Random.Range(0, 2) == 0)
                            {
                                buildedMapGrid[idx] = 118;
                            }
                        }
                        if (e <= floorUndergroundEnd && buildedMapGrid[idx] == 6)
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
                            else if (e < floorUndergroundEnd * 0.85f && e > floorUndergroundEnd * 0.5f && Random.Range(0, 245) == 0)
                            {
                                buildedMapGrid[idx] = 5;
                                if (buildedMapGrid[idx + 1] == 6) buildedMapGrid[idx + 1] = 5;
                                if (buildedMapGrid[idx - 1] == 6) buildedMapGrid[idx - 1] = 5;
                                if (i != WorldWidth - 1 && i2 != 15)
                                    if (buildedMapGrid[idx + WorldHeight] == 6) buildedMapGrid[idx + WorldHeight] = 5;
                                if (i != 0 && i2 != 0)
                                    if (buildedMapGrid[idx - WorldHeight] == 6) buildedMapGrid[idx - WorldHeight] = 5;
                                if (i != WorldWidth - 1 && i2 != 15)
                                    if (buildedMapGrid[idx + 1 + WorldHeight] == 6 && Random.Range(0, 3) == 0) buildedMapGrid[idx + 1 + WorldHeight] = 5;
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
                            else if (e < floorUndergroundEnd * 0.25f && e > floorUndergroundEnd * 0.1f && Random.Range(0, 100) == 0)
                            {
                                buildedMapGrid[idx] = 12;
                            }
                        }
                        if (e > floorUndergroundEnd && e < worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = floorUndergroundTile;
                        }
                        if (e == floorUndergroundEnd && Random.Range(0, 24) == 0)
                        {
                            buildedMapGrid[idx] = 4;
                            int max = Random.Range(0, 7);
                            for (int it = 0; it < max; it++)
                            {
                                if (buildedMapGrid[idx - it] == 6)
                                {
                                    buildedMapGrid[idx - it] = 4;
                                }
                            }
                        }
                        if (e == worldTileHeight[i * 16 + i2])
                        {
                            buildedMapGrid[idx] = floorSurfTile;
                        }
                        if (e > worldTileHeight[i * 16 + i2] && e < WorldHeight * 0.64f)
                        {
                            buildedMapGrid[idx] = 62;

                            if (buildedMapGrid[idx - 1] == 1)
                            {
                                if (Random.Range(0, 4) == 0)
                                {
                                    buildedMapGrid[idx - 1] = 4;
                                }
                                else
                                {
                                    buildedMapGrid[idx - 1] = 7;
                                }
                            }
                        }
                        else if (e > worldTileHeight[i * 16 + i2] && buildedMapGrid[idx] != 53 && buildedMapGrid[idx] != 55)
                        {
                            buildedMapGrid[idx] = 0;

                            if (buildedMapGrid[idx - 1] == 1)
                                if (Random.Range(0, 6) == 5)
                                {
                                    buildedMapGrid[idx] = 106;
                                }
                        }
                        if (Random.Range(0, 3) == 0 && i2 < 14 && i2 > 3 && e > worldTileHeight[i * 16 + i2])
                        {
                            if (buildedMapGrid[idx] == 0)
                            {
                                if (buildedMapGrid[idx - 1] == 1 &&
                                    buildedMapGrid[idx + WorldHeight] != 53 && buildedMapGrid[idx - WorldHeight] != 53
                                    && buildedMapGrid[idx + WorldHeight * 2] != 53 && buildedMapGrid[idx - WorldHeight * 2] != 53)
                                {
                                    buildedMapGrid[idx] = 53;
                                    buildedMapGrid[idx + 1] = 53;
                                    buildedMapGrid[idx + 2 + WorldHeight] = 55;
                                    buildedMapGrid[idx + 2] = 55;
                                    buildedMapGrid[idx + 2 - WorldHeight] = 55;
                                    buildedMapGrid[idx + 3 + WorldHeight] = 55;
                                    buildedMapGrid[idx + 3] = 55;
                                    buildedMapGrid[idx + 3 - WorldHeight] = 55;
                                }
                                else if (buildedMapGrid[idx - 1] == 113 &&
                                   buildedMapGrid[idx + WorldHeight] != 53 && buildedMapGrid[idx - WorldHeight] != 53
                                   && buildedMapGrid[idx + WorldHeight * 2] != 53 && buildedMapGrid[idx - WorldHeight * 2] != 53)
                                {
                                    buildedMapGrid[idx] = 53;
                                    buildedMapGrid[idx + 1] = 53;
                                    buildedMapGrid[idx + 2] = 53;
                                    buildedMapGrid[idx + 3] = 55;
                                    buildedMapGrid[idx + 4 + WorldHeight] = 55;
                                    buildedMapGrid[idx + 4 + WorldHeight * 2] = 55;
                                    buildedMapGrid[idx + 3 + WorldHeight * 2] = 55;
                                    buildedMapGrid[idx + 3 + WorldHeight] = 55;
                                    buildedMapGrid[idx + 4] = 55;
                                    buildedMapGrid[idx + 4 - WorldHeight] = 55;
                                    buildedMapGrid[idx + 4 - WorldHeight * 2] = 55;
                                    buildedMapGrid[idx + 3 - WorldHeight * 2] = 55;
                                    buildedMapGrid[idx + 3 - WorldHeight] = 55;
                                    buildedMapGrid[idx + 5 + WorldHeight] = 55;
                                    buildedMapGrid[idx + 5] = 55;
                                    buildedMapGrid[idx + 5 - WorldHeight] = 55;
                                }
                            }
                        }

                    }
                }
            }

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

                    if (cavesGeneration.GetPixel(x, y).a < 0.3f && y < floorUndergroundEnd * Random.Range(0.9f, 0.8f) && y > 2)
                    {
                        buildedMapGrid[cavesIdx] = 0;
                        if (y > WorldHeight * 0.2f)
                            if (Random.Range(0, (WorldHeight - y) / 2) == 0)
                            {
                                buildedMapGrid[cavesIdx] = 62;
                            }
                            else if (y < WorldHeight * 0.3)
                                if (Random.Range(0, y * 2) == 0 && buildedMapGrid[cavesIdx - 1] == 6)
                                {
                                    buildedMapGrid[cavesIdx] = 21;
                                }
                        if (y < WorldHeight * 0.06f)
                        {
                            buildedMapGrid[cavesIdx] = 21;
                            if(buildedMapGrid[cavesIdx - 1] == 6)
                            {
                                buildedMapGrid[cavesIdx - 1] = 117;

                                if (buildedMapGrid[cavesIdx - 2] == 6)
                                {
                                    buildedMapGrid[cavesIdx - 2] = 118;
                                }
                            }
                            else if (buildedMapGrid[cavesIdx - 1] == 118)
                            {
                                buildedMapGrid[cavesIdx - 1] = 117;
                            }
                        }
                    }
                    else if (cavesGeneration.GetPixel(x, y).a > 0.96f && y < floorUndergroundEnd * Random.Range(0.5f, 0.4f) && y > 3)
                    {
                        buildedMapGrid[cavesIdx] = 17;
                        if (y > WorldHeight * 0.2f)
                            if (Random.Range(0, (WorldHeight - y) / 2) == 0)
                            {
                                buildedMapGrid[cavesIdx] = 62;
                            }
                            else if (y < WorldHeight * 0.3)
                                if (Random.Range(0, y * 2) == 0 && buildedMapGrid[cavesIdx - 1] == 6)
                                {
                                    buildedMapGrid[cavesIdx] = 21;
                                }
                        if (y < WorldHeight * 0.06f)
                        {
                            buildedMapGrid[cavesIdx] = 21;
                        }
                    }
                }
            }

            perlinOffsetX += 16;
        }


        //STRUCTURES
        {
            for (int i = 0; i < WorldWidth / 5; i++)//chests
            {
                int yPosition = Random.Range((int)(WorldHeight * 0.1f), (int)(WorldHeight * 0.3f));
                int xPosition = Random.Range(0, WorldWidth * 16);
                int idx = yPosition + (xPosition * WorldHeight);

                if (TileCollisionType[buildedMapGrid[idx - 1]] == 1 && buildedMapGrid[idx] == 0)
                {
                    buildedMapGrid[idx] = 102;
                    string[] loot = { "102@true@30:5#32:3@@0", "102@true@39:1#64:5@@0", "102@true@75:1#51:13@@0", "102@true@70:3#93:6@@0", "102@true@30:7#31:4@@0", "102@true@96:1#66:4@@0" };
                    allMapProp[idx] = loot[Random.Range(0, loot.Length)];
                }
                else i--;
            }


            for (int i = 0; i < WorldWidth / 10; i++)//broken drones
            {
                int yPosition = Random.Range((int)(WorldHeight * 0.1f), (int)(WorldHeight * 0.3f));
                int xPosition = Random.Range(0, WorldWidth * 16);
                int idx = yPosition + (xPosition * WorldHeight);

                if (TileCollisionType[buildedMapGrid[idx - 1]] == 1 && buildedMapGrid[idx] == 0)
                {
                    buildedMapGrid[idx] = 103;
                }
                else i--;
            }

            if (isLorePlanet)
            {//lost storage center
                int xPosition = Random.Range(0, WorldWidth * 15);
                int yPosition = Random.Range(20, 30);

                for (int dx = xPosition; dx < xPosition + 4; dx++)
                {
                    for (int dy = yPosition; dy < yPosition + 4; dy++)
                    {
                        int idx = dy + (dx * WorldHeight);

                        allBackgroundGrid[idx] = 105;

                        if (dy == yPosition)
                            buildedMapGrid[idx] = 6;
                        else if (dy == yPosition + 1)
                        {
                            if (dx == xPosition)
                            {
                                buildedMapGrid[idx] = 104;
                                allMapProp[idx] = "104@true@78:2#103:" + Random.Range(1, 3) + "@@0";
                            }
                            else if (dx == 1 + xPosition)
                            {
                                buildedMapGrid[idx] = 104;
                                allMapProp[idx] = "104@true@88:2#92:1@@0";
                            }
                            else if (dx == 2 + xPosition)
                            {
                                if (Random.Range(0, 2) == 0)
                                {
                                    buildedMapGrid[idx] = 102;
                                    allMapProp[idx] = "102@true@66:2#67:1@@0";
                                }
                                else
                                {
                                    buildedMapGrid[idx] = 92;
                                }
                            }
                        }
                        else if (dy == 2 + yPosition)
                        {
                            if (dx == 1 + xPosition)
                            {
                                if (Random.Range(0, 2) == 0)
                                {
                                    buildedMapGrid[idx] = 104;
                                    allMapProp[idx] = "104@true@80:1#90:3#91:1@@0";
                                }
                                else
                                {
                                    buildedMapGrid[idx] = 89;
                                }
                            }
                        }
                    }
                }
            }

            if (currentPlanetName == "Korenz" || currentPlanetName == "Intersection")//enemy center
            {
                int xPosition = Random.Range(0, (int)(WorldWidth * 15 * 0.2f));
                int yPosition = WorldHeight / 10 * 9;

                while (yPosition > 0)
                {
                    int idx = yPosition + (xPosition * WorldHeight);

                    if (TileCollisionType[buildedMapGrid[idx]] != 0)
                        break;
                    else
                        yPosition--;
                }

                for (int dx = xPosition; dx < xPosition + 5; dx++)
                {
                    for (int dy = yPosition; dy < yPosition + 14; dy++)
                    {
                        int idx = dy + (dx * WorldHeight);

                        buildedMapGrid[idx] = 18;

                        if (dy == yPosition)
                        {
                            int e = idx - 1;
                            int pallete = 7;
                            while(TileCollisionType[buildedMapGrid[e]] != 1)
                            {
                                e--;
                            }
                            pallete = buildedMapGrid[e];

                            e = idx - 1;
                            while(TileCollisionType[buildedMapGrid[e]] != 1)
                            {
                                buildedMapGrid[e] = pallete;
                                e--;
                            }
                        }


                        if (dx == xPosition)
                        {
                            if (dy == yPosition + 1 || dy == yPosition + 2)
                            {
                                buildedMapGrid[idx] = 107;
                            }

                            if (dy == yPosition + 13)
                            {
                                buildedMapGrid[idx] = 108;
                            }
                        }

                        if (dx == xPosition + 1)
                        {
                            if (dy == yPosition + 1)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 2)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 3)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 4)
                            {
                                buildedMapGrid[idx] = 88;
                                allBackgroundGrid[idx] = 107;
                            }
                            if (dy == yPosition + 6)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 7)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 8)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 9)
                            {
                                buildedMapGrid[idx] = 88;
                                allBackgroundGrid[idx] = 107;
                            }
                            if (dy == yPosition + 11)
                            {
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 12)
                            {
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 13)
                            {
                                buildedMapGrid[idx] = 0;
                            }
                        }

                        if (dx == xPosition + 2)
                        {
                            if (dy == yPosition + 13)
                            {
                                buildedMapGrid[idx] = 18;
                            }
                            else
                            {
                                buildedMapGrid[idx] = 110;
                                allBackgroundGrid[idx] = 107;
                            }
                        }

                        if (dx == xPosition + 3)
                        {
                            if (dy == yPosition + 1)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 2)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 3)
                            {
                                buildedMapGrid[idx] = 0;
                                allBackgroundGrid[idx] = 107;
                            }
                            if (dy == yPosition + 4)
                            {
                                buildedMapGrid[idx] = 88;
                                allBackgroundGrid[idx] = 107;
                            }
                            if (dy == yPosition + 6)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 7)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 8)
                            {
                                allBackgroundGrid[idx] = 107;
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 9)
                            {
                                buildedMapGrid[idx] = 88;
                                allBackgroundGrid[idx] = 107;
                            }
                            if (dy == yPosition + 11)
                            {
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 12)
                            {
                                buildedMapGrid[idx] = 0;
                            }
                            if (dy == yPosition + 13)
                            {
                                buildedMapGrid[idx] = 0;
                            }
                        }
                        if (dx == xPosition + 4)
                        {
                            if (dy == yPosition + 1 || dy == yPosition + 2)
                            {
                                buildedMapGrid[idx] = 107;
                            }

                            if (dy == yPosition + 13)
                            {
                                buildedMapGrid[idx] = 108;
                            }
                        }
                    }
                }

                xPosition += 2;

                for (int y = yPosition; y > 20; y--)
                {
                    int idx = y + (xPosition * WorldHeight);
                    if (y > 35)
                        buildedMapGrid[idx] = 110;
                    else if (y == 35)
                        buildedMapGrid[idx] = 109;
                    else
                        buildedMapGrid[idx] = 0;

                    allBackgroundGrid[idx] = 107;

                    if (y == 21)
                    {
                        buildedMapGrid[idx - WorldHeight] = 88;
                        buildedMapGrid[idx + WorldHeight] = 88;
                        buildedMapGrid[idx - 1] = 21;
                        buildedMapGrid[idx - 1 - WorldHeight] = 6;
                        buildedMapGrid[idx - 1 + WorldHeight] = 6;
                        buildedMapGrid[idx - 2] = 6;
                    }
                }
            }
        }

        DataSaver.CreateTxt(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/mapBiomes.lgrsd", mapBiomes);
        DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(buildedMapGrid), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/map.lgrsd");
        DataSaver.SaveStats(ManagingFunctions.ConvertIntToStringArray(allBackgroundGrid), Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/bgmap.lgrsd");


        int[] sg = { 24, 16, 0, 0, 0, 0, 0, 0, 0 };
        int[] sa = { 1, 1, 0, 0, 0, 0, 0, 0, 0 };
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
        DataSaver.SaveStats(allMapProp, Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/mapprop.lgrsd");

        return buildedMapGrid;
    }

    int[] LoadMapGrid(string worldName)
    {
        WorldWidth = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/wp.lgrsd").SavedData)[0];
        WorldHeight = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/wp.lgrsd").SavedData)[1];

        int[] buildedMapGrid = new int[(WorldWidth * 16) * WorldHeight];

        buildedMapGrid = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/map.lgrsd").SavedData);
        if (DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + worldName + @"/mapprop.lgrsd") && !isNetworkClient)
            allMapProp = DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/mapprop.lgrsd").SavedData;
        else if (!isNetworkClient)
        {
            allMapProp = new string[buildedMapGrid.Length];

            for (int i = 0; i < allMapProp.Length; i++)
            {
                allMapProp[i] = "null";
            }
        }

        if (DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + worldName + @"/bgmap.lgrsd") && !isNetworkClient)
        {
            allBackgroundGrid = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + worldName + @"/bgmap.lgrsd").SavedData);
        }
        else if (!isNetworkClient)
        {
            allBackgroundGrid = new int[buildedMapGrid.Length];

            for (int i = 0; i < allBackgroundGrid.Length; i++)
            {
                allBackgroundGrid[i] = 0;
            }
        }


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
                int tileSet = allMapGrid[spawnedIdxs + tileIdx];
                int bgTileSet = allBackgroundGrid[spawnedIdxs + tileIdx];
                string tileProp = allMapProp[spawnedIdxs + tileIdx];
                newChunk.GetComponent<ChunkController>().TileGrid[tileIdx] = tileSet;
                newChunk.GetComponent<ChunkController>().BackgroundTileGrid[tileIdx] = bgTileSet;
                newChunk.GetComponent<ChunkController>().TilePropertiesArr[tileIdx] = tileProp;
                tileIdx++;
            }
        }
    }

    public IEnumerator ManageTransition(string variable, bool managingAction, float wait)
    {
        yield return new WaitForSeconds(wait);
        Transition.GetComponent<Animator>().SetBool(variable, managingAction);
        Transition.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }

    IEnumerator CheckChunks()
    {
        UpdateChunksActive();

        while (true)
        {
            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForSeconds(1);
                UpdateChunksActive();
                entitiesContainer.GetComponent<EntitiesManager>().UpdateEntities(LoadedChunks.ToArray());
                LightController.lightController.AddRenderQueue(Camera.main.transform.position);
            }
            if (!isNetworkClient)
                SaveGameData(true);
        }
    }

    public void UpdateChunksActive()
    {
        List<GameObject> gameChunksLoaded = new List<GameObject>();
        List<GameObject> gameChunksToLoad = new List<GameObject>();

        MenuController menuController = MenuController.menuController;

        int playerChunk = (int)Mathf.Floor(player.transform.position.x / 16f);

        for (int i = playerChunk - menuController.chunksOnEachSide; i < playerChunk + menuController.chunksOnEachSide; i++)
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

        foreach (GameObject chunk in gameChunksToLoad)
        {
            if (!chunk.activeInHierarchy)
            {
                chunk.SetActive(true);
            }

            //chunk.GetComponent<ChunkController>().UpdateWalls();
            gameChunksLoaded.Add(chunk);

        }

        chunksLimits.GetChild(0).GetComponent<BoxCollider2D>().size = new Vector2(1, WorldHeight + 4);
        chunksLimits.GetChild(1).GetComponent<BoxCollider2D>().size = new Vector2(1, WorldHeight + 4);

        LoadedChunks = gameChunksLoaded;
        UpdateChunksRelPos(0);
    }

    public void UpdateChunksRelPos(int movement)
    {
        foreach (GameObject chunk in LoadedChunks)
        {
            chunk.GetComponent<ChunkController>().UpdateChunkPos();
        }
        OnWorldRounding?.Invoke(movement);
    }

    public void ChangeBrush(int entryTile, GameObject tile)
    {
        if (building && !GInput.CancelPlacing)
        {
            int idx = System.Array.IndexOf(tile.transform.parent.GetComponent<ChunkController>().TileObject, tile);

            if (tileType[chosenBrush] != "background")
            {
                if (tileDefaultBrokeTool[entryTile] == "replace" && chosenBrush != tile.transform.parent.GetComponent<ChunkController>().TileGrid[idx])
                {
                    brush = chosenBrush;
                    StackBar.LoseItem();
                    if (tile.GetComponent<Timer>() != null) Destroy(tile.GetComponent<Timer>());


                    if (tile.GetComponent<BlockAnimationController>() != null) Destroy(tile.GetComponent<BlockAnimationController>());
                    tile.transform.parent.GetComponent<ChunkController>().TileGrid[idx] = brush;

                    tile.transform.parent.GetComponent<ChunkController>().UpdateChunk();
                    LightController.lightController.AddRenderQueue(player.transform.position);

                    if (isNetworkClient || isNetworkHost)
                    {
                        NetworkController.networkController.UpdateBlock(tile.transform.parent.GetSiblingIndex(), idx, brush);
                    }
                }
            }
            else
            {
                if (tileDefaultBrokeTool[tile.transform.parent.GetComponent<ChunkController>().BackgroundTileGrid[idx]] == "replace" && chosenBrush != tile.transform.parent.GetComponent<ChunkController>().BackgroundTileGrid[idx])
                {
                    brush = chosenBrush;
                    StackBar.LoseItem();

                    tile.transform.parent.GetComponent<ChunkController>().BackgroundTileGrid[idx] = brush;

                    tile.transform.parent.GetComponent<ChunkController>().UpdateChunk();

                    if (isNetworkClient || isNetworkHost)
                    {
                        NetworkController.networkController.UpdateBackgroundBlock(tile.transform.parent.GetSiblingIndex(), idx, brush);
                    }
                }
            }
        }
        else
        {
            brush = entryTile;
        }

        if (usingTool && !GInput.CancelPlacing)
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

                    PlayBreakSound(entryTile);
                }
                else if (toolUsing == tileDefaultBrokeTool[entryTile] && lastTileBrush == tile)
                {
                    if (addedFrameThisFrame)
                        breakingTime -= (toolUsingEfficency - ToolEfficency[entryTile]);

                    PlayBreakSound(entryTile);
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

                PlayBreakSound(entryTile);

                tile.transform.parent.GetComponent<ChunkController>().TileGrid[System.Array.IndexOf(tile.transform.parent.GetComponent<ChunkController>().TileObject, tile)] = brush;
                tile.transform.parent.GetComponent<ChunkController>().UpdateChunk();
                ManagingFunctions.DropItem(SwitchTroughBlockBroke(entryTile, Vector2Int.RoundToInt(tile.transform.position)), tile.transform.position);
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

    public int SwitchTroughBlockBroke(int entryTile, Vector2Int tileEntryPos, bool extraDrop = true)
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
                if (Random.Range(0, 5) == 0 && extraDrop)
                    ManagingFunctions.DropItem(3, tilePos);
                break;
            case 8:
                returnItem = 31;
                if(extraDrop)
                ManagingFunctions.DropItem(6, tilePos);
                break;
            case 9:
                returnItem = 33;
                if (extraDrop)
                    ManagingFunctions.DropItem(6, tilePos);
                break;
            case 10:
                returnItem = 32;
                if (extraDrop)
                    ManagingFunctions.DropItem(6, tilePos);
                break;
            case 11:
                returnItem = 30;
                if (extraDrop)
                    ManagingFunctions.DropItem(6, tilePos);
                break;
            case 12:
                returnItem = 34;
                if (extraDrop)
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
            case 98:
                if (ManagingFunctions.EntireDivision(tileEntryPos.x, 16).rest == 15 && extraDrop)
                {
                    SetTileAt((tileEntryPos.x + 1) * WorldHeight + tileEntryPos.y, 0);
                    returnItem = 100;
                }
                else
                {
                    returnItem = 0;
                }
                break;
            case 99:
                if (ManagingFunctions.EntireDivision(tileEntryPos.x, 16).rest == 0 && extraDrop)
                {
                    SetTileAt((tileEntryPos.x - 1) * WorldHeight + tileEntryPos.y, 0);
                    returnItem = 100;
                }
                else
                {
                    returnItem = 0;
                }
                break;
            case 104:
                returnItem = 0;
                break;
            case 106:
                returnItem = 0;
                break;
            case 114:
                if (ManagingFunctions.EntireDivision(tileEntryPos.x, 16).rest == 15 && extraDrop)
                {
                    SetTileAt((tileEntryPos.x + 1) * WorldHeight + tileEntryPos.y, 0);
                    returnItem = 116;
                }
                else
                {
                    returnItem = 0;
                }
                break;
            case 115:
                if (ManagingFunctions.EntireDivision(tileEntryPos.x, 16).rest == 0 && extraDrop)
                {
                    SetTileAt((tileEntryPos.x - 1) * WorldHeight + tileEntryPos.y, 0);
                    returnItem = 116;
                }
                else
                {
                    returnItem = 0;
                }
                break;
        }


        return returnItem;
    }

    public int SearchForClearSpawn(ref int x)
    {
        while (x < WorldWidth * 16) // Asegúrate de que no salga del límite del mundo
        {
            for (int i = WorldHeight - 2; i > 0; i--)
            {
                int currentIndex = (x * WorldHeight) + i;
                if (allMapGrid[currentIndex] == 62)
                {
                    x += 1; // Incrementa x y continúa con la siguiente columna
                    break;
                }
                else if (allMapGrid[currentIndex] != 0 && allMapGrid[currentIndex + 1] == 0)
                {
                    return i;
                }
            }
        }

        return WorldHeight + 1; // Si no se encontró una posición válida, retornamos un valor por defecto
    }

    public Vector2 RespawnPosition()
    {
        int x = Random.Range((int)(WorldWidth * 16 * 0.45f), (int)(WorldWidth * 16 * 0.55f));
        int y = SearchForClearSpawn(ref x);
        return new Vector2(x, y + 2);
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
            ChunkController chunk = chunkContainer.transform.GetChild(ManagingFunctions.EntireDivision(idx, WorldHeight * 16).cocient).GetComponent<ChunkController>();

            return chunk.TileObject[idx - chunk.tilesToChunk];
        }
        catch
        {
            return null;
        }
    }

    public bool ChunkActive(int x)
    {
        ChunkController c = chunkContainer.transform.GetChild((int)ManagingFunctions.ClampX(x) / 16).GetComponent<ChunkController>();
        return c.loaded && !c.loading;
    }

    public void Consume(int tile)
    {
        switch (tile)
        {
            case 68:
                player.FlyTime -= 200;
                break;
            case 78:
                if (dayLuminosity < 0.5f && entitiesContainer.GetComponentInChildren<ENTITY_TheDestroyer>() == null)
                {
                    ENTITY_TheDestroyer.StaticSpawn(null, new Vector2(player.transform.position.x + Random.Range(-10f, 10f), 0));
                    soundController.PlaySfxSound(SoundName.select);
                }
                else StackBar.AddItemInv(78);
                break;
            case 122:
                player.entityScript.AddState(EntityState.FireResistance, 90f);
                soundController.PlaySfxSound(SoundName.select);
                break;
        }
    }

    public ChunkController SetTileAt(int idx, int tile, bool updateChunk = true, bool markUpdateChunk = false)
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
            chunk.TileGrid[idx - chunk.tilesToChunk] = tile;
            if (updateChunk) chunk.UpdateChunk();
            if (markUpdateChunk) chunk.updateChunk = true;
            if (isNetworkClient || isNetworkHost) NetworkController.networkController.UpdateBlock(chunk.transform.GetSiblingIndex(), idx - chunk.tilesToChunk, tile);
            return chunk;
        }
        catch
        {
            return null;
        }
    }

    public void TileExplosionAt(int x, int y, int radius, int strength, bool dropBlocks = true)
    {
        for (int dx = x - radius; dx < x + radius + 1; dx++)
        {
            for (int dy = y - radius; dy < y + radius + 1; dy++)
            {
                int getTile = GetTileAt(dx * WorldHeight + dy);

                if (Vector2.Distance(new Vector2(dx, dy), new Vector2(x, y)) < radius && ToolEfficency[getTile] < strength)
                {
                    ChunkController c = SetTileAt(dx * WorldHeight + dy, 0, false, true);

                    if (Random.Range(0, 2) == 0 && dropBlocks)
                    {
                        ManagingFunctions.DropItem(SwitchTroughBlockBroke(getTile, new Vector2Int(dx, dy)), new Vector2(dx, dy));
                    }
                }
            }
        }
    }
}