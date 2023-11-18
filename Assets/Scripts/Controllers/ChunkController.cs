using System.Collections.Generic;
using UnityEngine;


public class ChunkController : MonoBehaviour, ITimerCall
{
    [SerializeField] public LayerMask tilesMasks;
    public Color chunkColor;
    public bool loaded = false;
    public string chunkBiome = "dumby";
    bool loading = false;
    public float[] ChunkGrid;
    public int[] TileGrid;
    public int[] TileGridRotation;
    public float[] LightMap;
    public GameObject[] TileObject;
    public float ID;
    public int childId;
    public int tilesToChunk;
    GameManager manager;
    PlayerController player;
    public const float entitiesSpawnTimeConstant = 3f;
    public float entitiesSpawnTime = 0f;

    public int[] liquidTileGrid;

    [Header("Prefabs")]

    [SerializeField] GameObject grassObject;
    [SerializeField] GameObject fireObject;
    [SerializeField] GameObject doorObject;
    [SerializeField] GameObject resourceGrabberObject;

    [SerializeField] Sprite[] waterFrames;

    public void CreateChunk(int h, float id, int cId, string chunkBiomeParam)
    {
        loaded = false;
        ID = id;
        ChunkGrid = new float[h * 16];
        TileGrid = new int[h * 16];
        TileGridRotation = new int[h * 16];
        TileObject = new GameObject[h * 16];
        LightMap = new float[h * 16];
        manager = GameManager.gameManagerReference;
        player = GameObject.Find("Lecter").GetComponent<PlayerController>();
        childId = cId;
        tilesToChunk = (manager.WorldHeight * 16) * childId;
        chunkBiome = chunkBiomeParam;
    }


    public void RegisterTile(float i, int t, int r, GameObject o)
    {
        ChunkGrid[(int)Mathf.Floor(i)] = i;
        TileGrid[(int)Mathf.Floor(i)] = t;
        TileGridRotation[(int)Mathf.Floor(i)] = r;
        TileObject[(int)Mathf.Floor(i)] = o;
    }

    public void UpdateChunk()
    {
        if (loaded)
        {
            for (int e = 0; e < TileGrid.Length; e++)
            {
                DivisionResult d = ManagingFunctions.EntireDivision(e, manager.WorldHeight);
                int tileX = (int)d.cocient;
                int tileY = (int)d.rest;

                if (TileGrid[e] < 0) TileGrid[e] = 0;
                if (manager.tileAnimation[TileGrid[e]] != null)
                {
                    if (TileObject[e].GetComponent<BlockAnimationController>() == null)
                    {
                        BlockAnimationController animationController = TileObject[e].AddComponent<BlockAnimationController>();
                        animationController.animationData = manager.tileAnimation[TileGrid[e]];
                        animationController.rootBIdx = e;
                        animationController.rootChunk = this;
                    }
                }
                else
                {
                    if (TileObject[e].GetComponent<BlockAnimationController>() != null)
                    {
                        Destroy(TileObject[e].GetComponent<BlockAnimationController>());
                    }
                }

                TileMod(e);

                /*BEHAVIOURS*/
                int aux1 = -1;
                if ((e + 1) < TileGrid.Length) aux1 = TileGrid[e + 1];

                bool hasSolidOnTop = false;
                if (aux1 != -1)
                {
                    if (manager.TileCollisionType[aux1] != "#")
                        hasSolidOnTop = false;
                    else
                        hasSolidOnTop = true;
                }

                if (!((e + 1) < TileGrid.Length) || !hasSolidOnTop)
                {
                    if (TileGrid[e] == 7 && TileObject[e].GetComponent<Timer>() == null)
                    {
                        if (tileY < manager.WorldHeight * 0.7)
                        {
                            Timer timer = TileObject[e].AddComponent<Timer>();
                            timer.InvokeTimer(Random.Range(70, 120), new string[] { "DirtReplace", e + "", "1", "1" }, this);
                        }
                        else
                        {
                            Timer timer = TileObject[e].AddComponent<Timer>();
                            timer.InvokeTimer(Random.Range(100, 170), new string[] { "DirtReplace", e + "", "2", "1" }, this);
                        }
                    }
                    if (TileGrid[e] == 1 && tileY > manager.WorldHeight * 0.7 && TileObject[e].GetComponent<Timer>() == null)
                    {
                        Timer timer = TileObject[e].AddComponent<Timer>();
                        timer.InvokeTimer(Random.Range(30, 50), new string[] { "DirtReplace", e + "", "2", "1" }, this);
                    }
                }
                if (aux1 != -1)
                    if (manager.TileCollisionType[aux1] == "#")//Si algo SOLIDO por encima
                    {
                        if (TileGrid[e] == 1 && TileObject[e].GetComponent<Timer>() == null)
                        {
                            Timer timer = TileObject[e].AddComponent<Timer>();
                            timer.InvokeTimer(Random.Range(30, 60), new string[] { "DirtReplace", e + "", "7", "1" }, this);
                        }
                        if (TileGrid[e] == 2 && TileObject[e].GetComponent<Timer>() == null)
                        {
                            Timer timer = TileObject[e].AddComponent<Timer>();
                            timer.InvokeTimer(Random.Range(30, 60), new string[] { "DirtReplace", e + "", "7", "1" }, this);
                        }
                    }


                if (TileGrid[e] == 2 && TileObject[e].GetComponent<Timer>() == null && tileY < manager.WorldHeight * 0.7)
                {
                    Timer timer = TileObject[e].AddComponent<Timer>();
                    timer.InvokeTimer(Random.Range(20, 40), new string[] { "Change", e + "", "1", "1" }, this);
                }

                if (TileGrid[e] == 3 && TileObject[e].GetComponent<Timer>() == null && tileY < manager.WorldHeight * 0.7)
                {
                    Timer timer = TileObject[e].AddComponent<Timer>();
                    if (Random.Range(0, 2) == 0)
                        timer.InvokeTimer(Random.Range(10, 20), new string[] { "Change", e + "", "62" }, this);
                    else
                        timer.InvokeTimer(Random.Range(30, 55), new string[] { "Change", e + "", "0" }, this);
                }

                if (TileGrid[e] == 70)
                {
                    if (manager.TileCollisionType[TileGrid[e - 1]] == "#" || manager.TileCollisionType[TileGrid[e - 1]] == "=")
                    {

                    }
                    else
                    {
                        TileGrid[e] = 0;
                        if (StackBar.stackBarController.currentItem == 70)
                            StackBar.AddItem(70);
                        else
                            ManagingFunctions.DropItem(70, TileObject[e].transform.position);

                    }
                }

                if (TileGrid[e] == 84)
                {
                    if (manager.TileCollisionType[TileGrid[e - 1]] == "#" || manager.TileCollisionType[TileGrid[e - 1]] == "=")
                    {
                        if (TileObject[e].GetComponent<Timer>() == null)
                        {
                            Timer timer = TileObject[e].AddComponent<Timer>();
                            timer.InvokeTimer(Random.Range(10, 20), new string[] { "Change", e + "", "0" }, this);
                        }
                    }
                    else
                    {
                        TileGrid[e] = 0;
                    }
                }

                if (TileGrid[e] == 88)
                {
                    if (e + 1 != GameManager.gameManagerReference.NumberOfTilesInChunk)
                        if (manager.TileCollisionType[TileGrid[e + 1]] == "#")
                        {

                        }
                        else
                        {
                            TileGrid[e] = 0;
                            if (StackBar.stackBarController.currentItem == 88)
                                StackBar.AddItem(88);
                            else
                                ManagingFunctions.DropItem(88, TileObject[e].transform.position);
                        }
                    else
                    {
                        TileGrid[e] = 0;
                        StackBar.AddItem(88);
                    }
                }


                if (manager.tileAnimation[TileGrid[e]] == null)
                    TileObject[e].GetComponent<SpriteRenderer>().sprite = manager.tiles[TileGrid[e]];

                string cT = manager.TileCollisionType[TileGrid[e]];

                TileObject[e].GetComponent<BoxCollider2D>().enabled = true;
                TileObject[e].transform.eulerAngles = new Vector3(0, 0, TileGridRotation[e]);
                switch (cT)
                {
                    case "#":
                        TileObject[e].layer = 8;
                        if (TileObject[e].GetComponent<PlatformEffector2D>() != null)
                        {
                            ModifyTile(false, "platform", TileObject[e]);
                        }
                        break;

                    case "":
                        TileObject[e].layer = 9;
                        if (TileObject[e].GetComponent<PlatformEffector2D>() != null)
                        {
                            ModifyTile(false, "platform", TileObject[e]);
                        }
                        break;

                    case "~":
                        TileObject[e].layer = 10;
                        if (TileObject[e].GetComponent<PlatformEffector2D>() != null)
                        {
                            ModifyTile(false, "platform", TileObject[e]);
                        }
                        break;

                    case "=":
                        TileObject[e].layer = 8;
                        if (TileObject[e].GetComponent<PlatformEffector2D>() == null)
                        {
                            ModifyTile(true, "platform", TileObject[e]);
                        }
                        break;
                }
                manager.allMapGrid[tilesToChunk + e] = TileGrid[e];
            }

            for (int x = 0; x < 16; x++)
            {
                bool isStillSunny = true;
                for (int y = manager.WorldHeight - 1; y > -1; y--)
                {
                    if (manager.TileCollisionType[TileGrid[(x * manager.WorldHeight) + y]] == "#") isStillSunny = false;
                    if (isStillSunny)
                    {
                        LightMap[(x * manager.WorldHeight) + y] = 2;
                    }
                    else
                    {
                        if (LightMap[(x * manager.WorldHeight) + y] < 3)
                            LightMap[(x * manager.WorldHeight) + y] = 0;
                        else
                            LightMap[(x * manager.WorldHeight) + y] = LightMap[(x * manager.WorldHeight) + y] - 2f;
                    }
                    if (TileGrid[(x * manager.WorldHeight) + y] == 70)
                    {
                        LightMap[(x * manager.WorldHeight) + y] = 1;
                    }
                    else if (TileGrid[(x * manager.WorldHeight) + y] == 84)
                    {
                        LightMap[(x * manager.WorldHeight) + y] = 0.5f;
                    }
                    else if (TileGrid[(x * manager.WorldHeight) + y] == 88)
                    {
                        LightMap[(x * manager.WorldHeight) + y] = 1;
                        LightMap[(x * manager.WorldHeight) + y - 1] = 3;
                    }
                }
            }
        }
        else if (!loading && !loaded)
        {
            /*StartCoroutine(*/
            SpawnChunk()/*)*/;
        }
    }

    public void TileMod(int e)
    {
        if (TileObject[e].transform.childCount > 0)
        {
            for (int i = 0; i < TileObject[e].transform.childCount; i++)
            {
                if (TileObject[e].transform.GetChild(i).gameObject.name != TileGrid[e] + "")
                {
                    GameObject block = TileObject[e].transform.GetChild(i).gameObject;
                    block.transform.parent = null;
                    Destroy(block);
                }
            }
        }

        if (TileGrid[e] == 1)
        {
            GameObject grassBlock = Instantiate(grassObject, TileObject[e].transform);
            grassBlock.transform.localPosition = Vector2.zero;
        }

        if (TileGrid[e] == 84 && TileObject[e].transform.childCount < 1)
        {
            GameObject fireBlock = Instantiate(fireObject, TileObject[e].transform);
            fireBlock.name = "84";
            fireBlock.transform.localPosition = Vector2.zero;
        }

        if (TileGrid[e] == 85)
        {
            try
            {
                if (TileGrid[e + 1] == 0 && TileGrid[e - 1] != 0)
                {
                    TileGrid[e] = 86;
                    TileGrid[e + 1] = 87;
                }
                else
                {
                    throw new System.Exception("UH NO");
                }
            }
            catch
            {
                TileGrid[e] = 0;
                StackBar.AddItem(85);
            }
        }

        if (TileGrid[e] == 86)
        {
            if (TileGrid[e + 1] != 87)
            {
                ManagingFunctions.DropItem(85, TileObject[e].transform.position);
                TileGrid[e] = 0;
            }
            else
            {
                if (TileObject[e].GetComponent<BlockAnimationController>() == null)
                {
                    BlockAnimationController animationController = TileObject[e].AddComponent<BlockAnimationController>();
                    animationController.animationData = manager.tileAnimation[TileGrid[e]];
                    animationController.rootBIdx = e;
                    animationController.rootChunk = this;
                }

                GameObject doorBlock = Instantiate(doorObject, TileObject[e].transform);
                doorBlock.transform.localPosition = Vector2.zero;
            }
        }

        if (TileGrid[e] == 87)
        {
            if (TileGrid[e - 1] != 86)
            {
                ManagingFunctions.DropItem(85, TileObject[e].transform.position - Vector3.up);
                TileGrid[e] = 0;
            }
        }

        if (TileGrid[e] == 89 && TileObject[e].transform.childCount < 1)
        {
            GameObject grabberObject = Instantiate(resourceGrabberObject, TileObject[e].transform);
            grabberObject.name = "89";
            grabberObject.transform.localPosition = Vector2.zero;
        }
    }


    public void ModifyTile(bool isAdd, string effect, GameObject tile)
    {
        if (isAdd)
        {
            if (effect == "platform")
            {
                tile.AddComponent<PlatformDrop>();
                PlatformEffector2D effector2D = tile.AddComponent<PlatformEffector2D>();
                effector2D.surfaceArc = 90;
                effector2D.useColliderMask = false;
                tile.GetComponent<BoxCollider2D>().usedByEffector = true;
            }
        }
        else
        {
            if (effect == "platform")
            {
                Destroy(tile.GetComponent<PlatformEffector2D>());
                Destroy(tile.GetComponent<PlatformDrop>());
                tile.GetComponent<BoxCollider2D>().usedByEffector = false;
            }
        }
    }

    public void BlocksPhysics()
    {
        liquidTileGrid = (int[])TileGrid.Clone();

        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < manager.WorldHeight; y++)
            {
                int idx = (x * manager.WorldHeight) + y;
                int tile = liquidTileGrid[idx];

                if (manager.TileCollisionType[tile] == "~")
                {
                    if (Mathf.Repeat(manager.frameTimer, manager.ToolEfficency[tile]) == 0)
                    {
                        if (liquidTileGrid[idx - 1] == 0)//GRAVITY
                        {
                            PhysicsForFluidBlocks(x, y, tile, new List<int>(new int[] { 0 }), 0, new Vector2(0, -1));
                        }
                        else/* if (liquidTileGrid[idx - 1] != tile)*/
                        {
                            int dir = Random.Range(0, 2);
                            if (dir == 0) dir = -1;
                            if (!PhysicsForFluidBlocks(x, y, tile, new List<int>(new int[] { 0 }), 0, new Vector2(dir, 0)))
                                PhysicsForFluidBlocks(x, y, tile, new List<int>(new int[] { 0 }), 0, new Vector2(-dir, 0));
                        }
                    }
                }
            }
        }
    }


    bool PhysicsForFluidBlocks(int x, int y, int tile, List<int> tileCondition, int residualTile, Vector2 dir)
    {
        int idx = (x * manager.WorldHeight) + y;
        bool placed = false;

        if (dir.x == -1 && !placed)
        {
            if (x == 0)
            {
                int extTile = manager.GetTileAt(tilesToChunk - (manager.WorldHeight - y));
                if (extTile != -1)
                {
                    if (tileCondition.Contains(extTile))
                    {
                        if (manager.GetTileObjectAt(tilesToChunk - (manager.WorldHeight - y)) != null)
                        {
                            Transform extTransform = manager.GetTileObjectAt(tilesToChunk - (manager.WorldHeight - y)).transform;
                            if (extTransform.parent.GetComponent<ChunkController>().loaded)
                            {
                                extTransform.GetComponent<SpriteRenderer>().sprite = manager.tiles[tile];
                                extTransform.parent.GetComponent<ChunkController>().TileGrid[(manager.WorldHeight * 15) + y] = tile;

                                TileGrid[idx] = residualTile;
                                TileObject[idx].GetComponent<SpriteRenderer>().sprite = manager.tiles[residualTile];

                                placed = true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (tileCondition.Contains(liquidTileGrid[idx - manager.WorldHeight]))
                {
                    TileGrid[idx - manager.WorldHeight] = tile;
                    liquidTileGrid[idx - manager.WorldHeight] = 1;
                    TileObject[idx - manager.WorldHeight].GetComponent<SpriteRenderer>().sprite = manager.tiles[tile];

                    TileGrid[idx] = residualTile;
                    TileObject[idx].GetComponent<SpriteRenderer>().sprite = manager.tiles[residualTile];

                    placed = true;
                }
            }
        }
        if (dir.x == 1 && !placed)
        {
            if (x == 15)
            {
                int extTile = manager.GetTileAt(tilesToChunk + manager.NumberOfTilesInChunk + y);
                if (extTile != -1)
                {
                    if (tileCondition.Contains(extTile))
                    {
                        if (manager.GetTileObjectAt(tilesToChunk + manager.NumberOfTilesInChunk + y) != null)
                        {
                            Transform extTransform = manager.GetTileObjectAt(tilesToChunk + manager.NumberOfTilesInChunk + y).transform;
                            if (extTransform.parent.GetComponent<ChunkController>().loaded)
                            {
                                extTransform.GetComponent<SpriteRenderer>().sprite = manager.tiles[tile];
                                extTransform.parent.GetComponent<ChunkController>().TileGrid[y] = tile;

                                TileGrid[idx] = residualTile;
                                TileObject[idx].GetComponent<SpriteRenderer>().sprite = manager.tiles[residualTile];

                                placed = true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (tileCondition.Contains(liquidTileGrid[idx + manager.WorldHeight]))
                {
                    TileGrid[idx + manager.WorldHeight] = tile;
                    liquidTileGrid[idx + manager.WorldHeight] = 1;
                    TileObject[idx + manager.WorldHeight].GetComponent<SpriteRenderer>().sprite = manager.tiles[tile];

                    TileGrid[idx] = residualTile;
                    TileObject[idx].GetComponent<SpriteRenderer>().sprite = manager.tiles[residualTile];

                    placed = true;
                }
            }
        }
        if (dir.y == 1 && !placed)
        {
            if (y < manager.WorldHeight - 1)
            {
                if (tileCondition.Contains(liquidTileGrid[idx + 1]))
                {
                    TileGrid[idx + 1] = tile;
                    liquidTileGrid[idx + 1] = 1;
                    TileObject[idx + 1].GetComponent<SpriteRenderer>().sprite = manager.tiles[tile];

                    TileGrid[idx] = residualTile;
                    TileObject[idx].GetComponent<SpriteRenderer>().sprite = manager.tiles[residualTile];

                    placed = true;
                }
            }
        }
        if (dir.y == -1 && !placed)
        {
            if (y > 0)
            {
                if (tileCondition.Contains(liquidTileGrid[idx - 1]))
                {
                    TileGrid[idx - 1] = tile;
                    liquidTileGrid[idx - 1] = 1;
                    TileObject[idx - 1].GetComponent<SpriteRenderer>().sprite = manager.tiles[tile];

                    TileGrid[idx] = residualTile;
                    TileObject[idx].GetComponent<SpriteRenderer>().sprite = manager.tiles[residualTile];

                    placed = true;
                }
            }
        }

        return placed;
    }

    public void CheckEntitiesSpawn()
    {
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < manager.WorldHeight; y++)
            {
                if (TileGrid[x * manager.WorldHeight + y] == 0 && y > 0 && y < manager.WorldHeight - 1 && manager.TileCollisionType[TileGrid[x * manager.WorldHeight + y - 1]] == "#")//Nano1
                {
                    if (Random.Range(0, (int)(250 * manager.dayLuminosity)) == 0 && manager.dayLuminosity > 0.5f)
                    {
                        if (manager.TileCollisionType[TileGrid[x * manager.WorldHeight + y + 1]] == "" && Vector2.Distance(new Vector2(x + transform.position.x, y + 0.3f), manager.player.transform.position) > 20)
                        {
                            ENTITY_NanoBotT1.StaticSpawn(null, new Vector2(x + transform.position.x, y + 0.3f));
                        }
                    }
                }
                if (TileGrid[x * manager.WorldHeight + y] == 0 && y > 0 && y < manager.WorldHeight - 1 && TileGrid[x * manager.WorldHeight + y - 1] == 6 && y < manager.WorldHeight * 0.5f)//Nano2
                {
                    if (Random.Range(0, 300) == 0)
                    {
                        if (manager.TileCollisionType[TileGrid[x * manager.WorldHeight + y + 1]] == "" && Vector2.Distance(new Vector2(x + transform.position.x, y + 0.3f), manager.player.transform.position) > 20)
                        {
                            ENTITY_NanoBotT2.StaticSpawn(null, new Vector2(x + transform.position.x, y + 0.3f));
                        }
                    }
                }
                if (TileGrid[x * manager.WorldHeight + y] == 0 && y > 0 && y < manager.WorldHeight - 1 && manager.TileCollisionType[TileGrid[x * manager.WorldHeight + y - 1]] == "#")//Nano1
                {
                    if (Random.Range(0, 450) == 0)
                    {
                        if (manager.TileCollisionType[TileGrid[x * manager.WorldHeight + y + 1]] == "" && Vector2.Distance(new Vector2(x + transform.position.x, y + 0.3f), manager.player.transform.position) > 20)
                        {
                            ENTITY_NanoBotT3.StaticSpawn(null, new Vector2(x + transform.position.x, y + 0.3f));
                        }
                    }
                }
            }
        }
    }

    public /*IEnumerator*/void SpawnChunk()
    {
        loading = true;
        int tileX = 0;
        int tileY = 0;
        float tileName = 0 + ID;
        int tileIdx = 0;

        for (int eX = 0; eX < 16; eX++)
        {
            for (int e = 0; e < manager.WorldHeight; e++)
            {
                GameObject newTile = manager.ExtractPooledTile(new Vector2(tileX, tileY));
                newTile.transform.SetParent(transform, false);
                newTile.name = "Tile" + tileName;
                int tileSet = 0;

                tileSet = TileGrid[tileIdx];

                RegisterTile(tileName, tileSet, 0, newTile);
                tileY++;
                tileName++;
                tileIdx++;
            }

            tileY = 0;
            tileX++;
            //if (Mathf.Abs(transform.position.x - player.transform.position.x) > 30)
            //    yield return new WaitForEndOfFrame();
        }

        loaded = true;
        loading = false;
        UpdateChunk();
    }

    public void DestroyChunk()
    {
        //for(int e = 0; e < TileObject.Length; e++)
        //{
        //    manager.AddTileToPool(TileObject[e]);
        //    TileObject[e] = null;
        //}

        //ChunkGrid = new float[manager.WorldHeight * 16];
        //TileObject = new GameObject[manager.WorldHeight * 16];
        //LightMap = new float[manager.WorldHeight * 16];
        //loaded = false;
    }

    void Update () {
        if (manager.InGame && loaded)
        {
            if (manager.doingAnAction)
            {
                if (manager.doingAnAction && Mathf.Abs(player.transform.position.x - transform.position.x) < System.Convert.ToInt32(GameObject.Find("MenuFunctions").GetComponent<MenuController>().ChunkUpdateNotifier.text) || !manager.Generated)
                {
                    if (GInput.GetMouseButton(0))
                    {
                        RaycastHit2D rayHit = new RaycastHit2D();
                        Vector3 mousePos = manager.mouseCurrentPosition;
                        mousePos.y++;
                        rayHit = Physics2D.Raycast(mousePos, Vector2.down, 1f, tilesMasks);
                        if (rayHit.transform != null)
                        {
                            ClickedObject(rayHit.transform.gameObject);
                        }
                        Debug.DrawRay(player.transform.position, manager.mouseCurrentPosition - player.transform.position, Color.blue);
                    }
                    else
                    {
                        Debug.DrawRay(player.transform.position, manager.mouseCurrentPosition - player.transform.position, Color.green);
                    }
                }
            }
            else
            {
                Debug.DrawRay(player.transform.position, manager.mouseCurrentPosition - player.transform.position, Color.red);
            }
            BlocksPhysics();

            if (entitiesSpawnTime > entitiesSpawnTimeConstant)
            {
                CheckEntitiesSpawn();
                entitiesSpawnTime = 0;
            }
            else
            {
                entitiesSpawnTime += Time.deltaTime;
            }

            for (int e = 0; e < TileGrid.Length; e++)
            {
                manager.allMapGrid[tilesToChunk + e] = TileGrid[e];
            }
        }
    }

    void ClickedObject(GameObject obj)
    {
        if (obj.transform.parent == gameObject.transform)
        {
            if (Mathf.Abs(player.transform.position.x - obj.transform.position.x) > 0.6f || Mathf.Abs(player.transform.position.y - obj.transform.position.y) > 0.9f || (!manager.building || manager.TileCollisionType[StackBar.stackBarController.StackBarGrid[StackBar.stackBarController.idx]] != "#"))
            {
                int previousBrush = manager.brush;
                manager.ChangeBrush(TileGrid[System.Array.IndexOf(TileObject, obj)], obj);
            }
        }
    }

    void ITimerCall.TimerCall(string[] msg)
    {
        if (msg[0] == "DirtReplace")
        {
            if (msg[3] == "1" && TileGrid[System.Convert.ToInt32(msg[1])] != 0)
            {
                TileGrid[System.Convert.ToInt32(msg[1])] = System.Convert.ToInt32(msg[2]);

                if ((msg[2] == "1" || msg[2] == "2") && manager.TileCollisionType[TileGrid[System.Convert.ToInt32(msg[1]) + 1]] == "#")
                {
                    TileGrid[System.Convert.ToInt32(msg[1])] = 7;
                }
                if (msg[2] == "7" && manager.TileCollisionType[TileGrid[System.Convert.ToInt32(msg[1]) + 1]] != "#")
                {
                    TileGrid[System.Convert.ToInt32(msg[1])] = 1;
                }
            }
        }

        if(msg[0] == "Change")
        {
            TileGrid[System.Convert.ToInt32(msg[1])] = System.Convert.ToInt32(msg[2]);
            LightController.lightController.AddRenderQueue(player.transform.position);
        }

        UpdateChunk();
    }
}
