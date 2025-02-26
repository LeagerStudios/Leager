using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkItemController : MonoBehaviour {

    [SerializeField] public LayerMask tilesMasks;
    [SerializeField] public LayerMask entitiesMasks;
    [SerializeField] GameObject CraftMenu;
    [SerializeField] GameObject AdvTechCraftMenu;
    [SerializeField] public Sprite[] spritesRenders;
    [SerializeField] public bool[] canInteract;
    GameObject workIcon;

    void Start ()
    {
        workIcon = transform.GetChild(0).gameObject;
	}

    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
        {
            Vector3Int mousePos = Vector3Int.FloorToInt((Vector2)GameManager.gameManagerReference.mouseCurrentPosition + Vector2.one * 0.5f);
            int idx = mousePos.x * GameManager.gameManagerReference.WorldHeight + mousePos.y;
            GameObject tile = GameManager.gameManagerReference.GetTileObjectAt(idx);
            int tileSelected = GameManager.gameManagerReference.GetTileAt(idx);

            workIcon.GetComponent<SpriteRenderer>().sprite = GameManager.gameManagerReference.tiles[0];

            if (tile != null)
                if (!GameManager.gameManagerReference.doingAnAction)
                {
                    if (canInteract[tileSelected] == true)
                    {
                        workIcon.GetComponent<SpriteRenderer>().sprite = spritesRenders[tileSelected];
                        if (GInput.GetMouseButtonDown(1) || (GInput.leagerInput.platform == "Mobile" && !GInput.CancelPlacing && GInput.GetMouseButtonDown(0)))
                        {
                            DisplayForTile(tileSelected, tile);
                        }
                    }
                }
                else
                {
                    if (GInput.GetMouseButton(0))
                    {
                        Vector4 tileSize = GameManager.gameManagerReference.tileSize[StackBar.stackBarController.currentItem];
                        Vector3 offset = new Vector3(0.5f * (tileSize.x - 1) - tileSize.z, 0.5f * (tileSize.y - 1) - tileSize.w);

                        if (!GameManager.gameManagerReference.building || (!Physics2D.OverlapBox(tile.transform.position + offset, tileSize, 0, entitiesMasks) || GameManager.gameManagerReference.TileCollisionType[StackBar.stackBarController.currentItem] != 1))
                            tile.transform.parent.GetComponent<ChunkController>().ClickedTile(tile);
                    }
                }
        }
    }

    public void DisplayForTile(int tile, GameObject tileObj)
    {
        if (tile == 15)
        {
            Box box = tileObj.transform.GetComponentInChildren<Box>();

            if (!box.firstFrame)
                box.ToggleItems();
        }


        if (tile == 16)
        {
            if (MenuController.menuController.uiMenus.Find("CraftMenu") == null /*&& TechManager.techTree.fullyUnlockedItems.Count > 0*/)
            {
                GameObject a = Instantiate(CraftMenu, MenuController.menuController.uiMenus);
                a.name = "CraftMenu";
                a.GetComponent<CraftMenuController>().InvokeMenu(tileObj.transform);
            }
        }

        if (tile == 89)
        {
            if (GameManager.gameManagerReference.InGame)
            {
                MenuController.menuController.PlanetMenuDeploy(tileObj.transform.GetComponentInChildren<ResourceLauncher>());
            }
        }

        if (tile == 102)
        {
            tileObj.transform.GetComponentInChildren<Box>().ToggleItems();
        }

        if (tile == 125)
        {
            if(StackBar.stackBarController.currentItem == 30)
            {
                tileObj.transform.GetComponentInChildren<EnergyGenerator>().shock = true;
                StackBar.LoseItem();
            }
        }

        if (tile == 130)
        {
            if (MenuController.menuController.uiMenus.Find("AdvTechCraftMenu") == null /*&& TechManager.techTree.fullyUnlockedItems.Count > 0*/)
            {
                GameObject a = Instantiate(AdvTechCraftMenu, MenuController.menuController.uiMenus);
                a.name = "AdvTechCraftMenu";
                a.GetComponent<CraftMenuController>().InvokeMenu(tileObj.transform);
            }
        }
    }
}
