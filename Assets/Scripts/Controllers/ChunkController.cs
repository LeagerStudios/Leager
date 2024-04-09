using System.Collections.Generic;
using System.Collections;
using UnityEngine;


public class ChunkController : MonoBehaviour, ITimerCall
{
    [SerializeField] public LayerMask tilesMasks;
    public Color chunkColor;
    public bool loaded = false;
    public string chunkBiome = "dumby";
    public bool loading = false;
    public int[] TileGrid;
    public string[] TilePropertiesArr;
    public float[] LightMap;
    public GameObject[] TileObject;
    public float ID;
    public int childId;
    public int tilesToChunk;
    public int orgX;
    GameManager manager;
    PlayerController player;
    public const float entitiesSpawnTimeConstant = 3f;
    public float entitiesSpawnTime = 0f;

    public int[] liquidTileGrid;

    [Header("Prefabs")]

    [SerializeField] GameObject grassObject;
    [SerializeField] GameObject fireObject;
    [SerializeField] GameObject doorObject;
    [SerializeField] GameObject boxObject;
    [SerializeField] GameObject resourceGrabberObject;
    [SerializeField] GameObject unitCenterObject;
    [SerializeField] GameObject leavesObject;
    [SerializeField] GameObject ladderObject;

    [SerializeField] Sprite[] waterFrames;

    public void CreateChunk(int h, float id, int cId, string chunkBiomeParam, int orgXpos)
    {
        loaded = false;
        ID = id;
        TileGrid = new int[h * 16];
        TilePropertiesArr = new string[h * 16];
        TileObject = new GameObject[h * 16];
        LightMap = new float[h * 16];
        manager = GameManager.gameManagerReference;
        orgX = orgXpos;
        player = GameObject.Find("Lecter").GetComponent<PlayerController>();
        childId = cId;
        tilesToChunk = (manager.WorldHeight * 16) * childId;
        chunkBiome = chunkBiomeParam;
    }


    public void RegisterTile(int idx, int t, string prop, string mesh, GameObject o)
    {
        TileGrid[idx] = t;
        TileObject[idx] = o;
        TilePropertiesArr[idx] = prop;
        o.GetComponent<SpriteRenderer>().sprite = GameManager.gameManagerReference.tiles[t];
       if(prop != "null") o.AddComponent<TileProperties>().Load(prop);
    }

    public void UpdateChunkPos()
    {
        float playerx = manager.player.transform.position.x;
        float nearest = 999999f;
        int nearestPos = orgX;

        for (int i = -1; i < 2; i++)
        {
            float scan = orgX + ((manager.WorldWidth * 16) * i);
            float dist = Mathf.Abs(playerx - scan);

            if (dist < nearest)
            {
                nearest = dist;
                nearestPos = (int)scan;
            }
        }


        transform.position = new Vector2(nearestPos, 0);
    }

    public void UpdateWalls()
    {
        int sibling = transform.GetSiblingIndex();
        int sPlus = sibling + 1;
        int sMinus = sibling - 1;

        if (sMinus < 0)
        {
            sMinus += manager.WorldWidth;
        }
        if (sPlus >= manager.WorldWidth)
        {
            sPlus -= manager.WorldWidth;
        }

        if (!transform.parent.GetChild(sMinus).GetComponent<ChunkController>().loaded)
        {
            manager.chunksLimits.GetChild(0).position = new Vector2(transform.position.x, manager.WorldHeight / 2);
        }
        if (!transform.parent.GetChild(sPlus).GetComponent<ChunkController>().loaded)
        {
            manager.chunksLimits.GetChild(1).position = new Vector2(transform.position.x + 16, manager.WorldHeight / 2);
        }
    }

    public void UpdateChunk()
    {
        if (loaded && !loading)
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
                    if (manager.TileCollisionType[aux1] != "#" && manager.TileCollisionType[aux1] != "~")
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

                if (TileGrid[e] == 4 && TileObject[e].GetComponent<Timer>() == null && TileGrid[e - 1] == 5 && !hasSolidOnTop)
                {
                    Timer timer = TileObject[e].AddComponent<Timer>();
                    timer.InvokeTimer(10, new string[] { "ChangeBrick", e + "",}, this);
                    TileObject[e].GetComponent<BlockAnimationController>().PlayAnimation();
                }
                else if (TileGrid[e] == 4 && TileObject[e].GetComponent<Timer>() != null && TileGrid[e - 1] != 5)
                {
                    Destroy(TileObject[e].GetComponent<BlockAnimationController>());
                    TileGrid[e] = 0;
                    ManagingFunctions.DropItem(4, TileObject[e].transform.position);
                    Destroy(TileObject[e].GetComponent<Timer>());
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
                {
                    TileObject[e].GetComponent<SpriteRenderer>().sprite = manager.tiles[TileGrid[e]];
                    TileObject[e].GetComponent<SpriteRenderer>().sortingOrder = 0;
                }
                else
                {
                    TileObject[e].GetComponent<SpriteRenderer>().sortingOrder = 1;
                }
                    

                string cT = manager.TileCollisionType[TileGrid[e]];

                TileObject[e].GetComponent<BoxCollider2D>().enabled = true;
                TileObject[e].GetComponent<BoxCollider2D>().isTrigger = cT == "~" || cT == "/" || cT == @"\";

                if(cT == "/" || cT == @"\")
                {
                    TileObject[e].GetComponent<BoxCollider2D>().size = Vector2.one * 1.05f;
                }
                else
                {
                    TileObject[e].GetComponent<BoxCollider2D>().size = Vector2.one * 1f;
                }

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

                    case "/":
                        TileObject[e].layer = 17;
                        if (TileObject[e].GetComponent<PlatformEffector2D>() != null)
                        {
                            ModifyTile(false, "platform", TileObject[e]);
                        }
                        break;

                    case @"\":
                        TileObject[e].layer = 18;
                        if (TileObject[e].GetComponent<PlatformEffector2D>() != null)
                        {
                            ModifyTile(false, "platform", TileObject[e]);
                        }
                        break;
                }
            }

            for (int x = 0; x < 16; x++)
            {
                bool isStillSunny = true;
                for (int y = manager.WorldHeight - 1; y > -1; y--)
                {
                    int idx = (x * manager.WorldHeight) + y;
                    if (manager.TileCollisionType[TileGrid[(x * manager.WorldHeight) + y]] == "#") isStillSunny = false;
                    if (isStillSunny)
                    {
                        LightMap[idx] = 2;
                    }
                    else
                    {
                        if (LightMap[idx] < 3)
                            LightMap[idx] = 0;
                        else
                            LightMap[idx] = LightMap[idx] - 2f;
                    }
                    if (TileGrid[idx] == 70)
                    {
                        LightMap[idx] = 1;
                    }
                    else if (TileGrid[idx] == 84)
                    {
                        LightMap[idx] = 0.5f;
                    }
                    else if (TileGrid[idx] == 88)
                    {
                        LightMap[idx] = 1;
                        LightMap[idx - 1] = 3;
                    }
                    else if (TileGrid[idx] == 62)
                    {
                        if (idx == LightMap.Length - 1)
                        {
                            LightMap[idx] = 2 * 0.6f;
                        }
                        else
                        {
                            LightMap[idx] = LightMap[idx + 1] * 0.6f;
                        }
                    }
                    else if (TileGrid[idx] == 21)
                    {
                        LightMap[idx] = 1f;
                    }
                }
            }

            UpdateWalls();
        }
    }

    public void TileMod(int e)
    {
        GameObject tile = TileObject[e];
        TileProperties tileProperties = tile.GetComponent<TileProperties>();

        if (tileProperties)
        {
            if(tileProperties.parentTile != TileGrid[e])
            {
                if (tileProperties.canDropStoredItems)
                {
                    foreach(string item in tileProperties.storedItems)
                    {
                        string text = item;
                        if (text[text.Length - 1] == ';')
                            text = text.Remove(item.Length - 1);
                        int[] datas = ManagingFunctions.ConvertStringToIntArray(text.Split(':'));
                        ManagingFunctions.DropItem(datas[0], tile.transform.position, new Vector2(Random.Range(-4f, 4f), 6), datas[1]);
                    }
                }

                TilePropertiesArr[e] = "null";
                manager.allMapProp[tilesToChunk + e] = "null";
                tileProperties.destroy = true;
                Destroy(tileProperties);
            }
        }

        if (tile.transform.childCount > 0)
        {
            for (int i = 0; i < tile.transform.childCount; i++)
            {
                if (tile.transform.GetChild(i).gameObject.name != TileGrid[e] + "")
                {
                    GameObject block = tile.transform.GetChild(i).gameObject;
                    block.transform.parent = null;
                    Destroy(block);
                }
            }
        }

        if (TileGrid[e] == 1 && tile.transform.childCount < 1)
        {
            GameObject grassBlock = Instantiate(grassObject, tile.transform);
            grassBlock.transform.localPosition = Vector2.zero;
            grassBlock.name = "1";
        }

        if (TileGrid[e] == 15 && tile.transform.childCount < 1)
        {
            GameObject boxBlock = Instantiate(boxObject, tile.transform);
            boxBlock.transform.localPosition = Vector2.zero;
            boxBlock.name = "15";
        }

        if (TileGrid[e] == 55 && tile.transform.childCount < 1)
        {
            GameObject leavesBlock = Instantiate(leavesObject, tile.transform);
            leavesBlock.transform.localPosition = Vector2.zero;
            leavesBlock.name = "55";
        }

        if (TileGrid[e] == 84 && tile.transform.childCount < 1)
        {
            GameObject fireBlock = Instantiate(fireObject, tile.transform);
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
                ManagingFunctions.DropItem(85, tile.transform.position);
                TileGrid[e] = 0;
            }
            else
            {
                if (tile.GetComponent<BlockAnimationController>() == null)
                {
                    BlockAnimationController animationController = tile.AddComponent<BlockAnimationController>();
                    animationController.animationData = manager.tileAnimation[TileGrid[e]];
                    animationController.rootBIdx = e;
                    animationController.rootChunk = this;
                }

                if (tile.transform.childCount < 1)
                {
                    GameObject doorBlock = Instantiate(doorObject, tile.transform);
                    doorBlock.name = "86";
                    doorBlock.transform.localPosition = Vector2.zero;
                }
            }
        }

        if (TileGrid[e] == 87)
        {
            if (TileGrid[e - 1] != 86)
            {
                ManagingFunctions.DropItem(85, tile.transform.position - Vector3.up);
                TileGrid[e] = 0;
            }
        }

        if (TileGrid[e] == 89 && tile.transform.childCount < 1)
        {
            GameObject grabberObject = Instantiate(resourceGrabberObject, tile.transform);
            grabberObject.name = "89";
            grabberObject.transform.localPosition = Vector2.zero;
        }

        if (TileGrid[e] == 98)
        {
            if (manager.GetTileAt(tilesToChunk + e + manager.WorldHeight) != 99)
            {
                ManagingFunctions.DropItem(100, tile.transform.position + Vector3.right * 0.5f);
                TileGrid[e] = 0;
            }
        }

        if (TileGrid[e] == 99)
        {
            if (manager.GetTileAt(tilesToChunk + e - manager.WorldHeight) != 98)
            {
                ManagingFunctions.DropItem(100, tile.transform.position + Vector3.left * 0.5f);
                TileGrid[e] = 0;
            }
        }

        if (TileGrid[e] == 100)
        {
            try
            {
                if (manager.GetTileAt(tilesToChunk + e + manager.WorldHeight) == 0)
                {
                    TileGrid[e] = 98;
                    manager.SetTileAt(tilesToChunk + e + manager.WorldHeight, 99);
                }
                else
                {
                    throw new System.Exception("UH NO");
                }
            }
            catch
            {
                TileGrid[e] = 0;
                StackBar.AddItem(100);
            }
        }

        if (TileGrid[e] == 101 && tile.transform.childCount < 1)
        {
            GameObject unitCenter = Instantiate(unitCenterObject, tile.transform);
            unitCenter.name = "101";
            unitCenter.transform.localPosition = Vector2.zero;
        }

        if (TileGrid[e] == 102 && tile.transform.childCount < 1)
        {
            GameObject boxBlock = Instantiate(boxObject, tile.transform);
            boxBlock.transform.localPosition = Vector2.zero;
            boxBlock.name = "102";
        }

        if (TileGrid[e] == 109 && tile.transform.childCount < 1)
        {
            GameObject ladderBlock = Instantiate(ladderObject, tile.transform);
            ladderBlock.transform.localPosition = Vector2.zero;
            ladderBlock.name = "109";
        }

        if (TileGrid[e] == 110 && tile.transform.childCount < 1)
        {
            GameObject ladderBlock = Instantiate(ladderObject, tile.transform);
            ladderBlock.transform.localPosition = Vector2.zero;
            ladderBlock.name = "110";
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
                            int dir = Random.Range(-1, 2);
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
        if (!manager.isNetworkClient)
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < manager.WorldHeight; y++)
                {
                    if (TileGrid[x * manager.WorldHeight + y] == 0 && y > 0 && y < manager.WorldHeight - 1 && manager.TileCollisionType[TileGrid[x * manager.WorldHeight + y - 1]] == "#")//Nano1
                    {
                        if (Random.Range(0, (int)(350 * manager.dayLuminosity)) == 0 && manager.dayLuminosity > 0.5f)
                        {
                            if (manager.TileCollisionType[TileGrid[x * manager.WorldHeight + y + 1]] == "" && Vector2.Distance(new Vector2(x + transform.position.x, y + 0.3f), manager.player.transform.position) > 20)
                            {
                                ENTITY_NanoBotT1.StaticSpawn(null, new Vector2(x + transform.position.x, y + 0.3f));
                            }
                        }
                    }
                    if (TileGrid[x * manager.WorldHeight + y] == 0 && y > 0 && TileGrid[x * manager.WorldHeight + y - 1] == 6 && y < manager.WorldHeight * 0.5f)//Nano2
                    {
                        if (Random.Range(0, 250) == 0)
                        {
                            if (manager.TileCollisionType[TileGrid[x * manager.WorldHeight + y + 1]] == "" && Vector2.Distance(new Vector2(x + transform.position.x, y + 0.3f), manager.player.transform.position) > 20)
                            {
                                ENTITY_NanoBotT2.StaticSpawn(null, new Vector2(x + transform.position.x, y + 0.3f));
                            }
                        }
                    }
                    if (TileGrid[x * manager.WorldHeight + y] == 0 && y > 0 && y < manager.WorldHeight - 1 && TileGrid[x * manager.WorldHeight + y - 1] == 6)//Nano3
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

    public IEnumerator SpawnChunk()
    {
        loading = true;
        int counter = 0;
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

                int tileSet = TileGrid[tileIdx];
                string properties = TilePropertiesArr[tileIdx];
                
                RegisterTile(tileIdx, tileSet, properties, manager.TileCollisionType[tileSet], newTile);
                tileY++;
                tileName++;
                tileIdx++;

                if (Mathf.Abs(transform.position.x + 16 - manager.player.transform.position.x) > 32 && counter >= manager.tileSpawnRate)
                {
                    yield return new WaitForEndOfFrame();
                    counter = 0;
                }
                counter++;
            }

            tileY = 0;
            tileX++;
        }

        
        loading = false;
        loaded = true;
        UpdateChunk();
    }

    public void DestroyChunk()
    {
        if(!loading && loaded)
        {
            StartCoroutine(DestroyCunkIE());
        }
    }

    private IEnumerator DestroyCunkIE()
    {
        loading = true;
        int counter = 0;

        for(int e = 0; e < TileGrid.Length; e++)
        {
            manager.allMapGrid[tilesToChunk + e] = TileGrid[e];
            if (TileObject[e].GetComponent<TileProperties>())
            {
                TilePropertiesArr[e] = TileObject[e].GetComponent<TileProperties>().Export();
                manager.allMapProp[tilesToChunk + e] = TilePropertiesArr[e];
            }
        }

        for (int e = 0; e < TileObject.Length; e++)
        {
            GameObject obj = TileObject[e];


            Destroy(obj);

            if (Mathf.Abs(transform.position.x + 16 - manager.player.transform.position.x) > 32 && counter >= manager.tileSpawnRate)
            {
                yield return new WaitForEndOfFrame();
                counter = 0;
            }
            counter++;
        }

        
        loading = false;
        loaded = false;
        UpdateWalls();
        gameObject.SetActive(false);
    }

    void Update () {
        if (manager.InGame && loaded && !loading)
        {
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
        else if (!loading && !loaded)
        {
            StartCoroutine(SpawnChunk());
        }
    }

    public void ClickedTile(GameObject obj)
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


        if (msg[0] == "ChangeBrick")
        {
            TileGrid[System.Convert.ToInt32(msg[1])] = 0;
            TileGrid[System.Convert.ToInt32(msg[1]) - 1] = 14;
            LightController.lightController.AddRenderQueue(player.transform.position);
        }

        UpdateChunk();
    }
}
