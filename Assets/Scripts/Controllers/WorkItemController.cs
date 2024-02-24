using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkItemController : MonoBehaviour {

    [SerializeField] GameObject CraftMenu;
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
            RaycastHit2D rayHit = new RaycastHit2D();
            Vector3 mousePos = GameManager.gameManagerReference.mouseCurrentPosition;
            mousePos.y++;
            rayHit = Physics2D.Raycast(mousePos, Vector2.down, 1f);
            if (rayHit.transform != null && rayHit.transform.parent != null)
            {
                if (rayHit.transform.parent.parent != null)
                {
                    workIcon.GetComponent<SpriteRenderer>().sprite = GameManager.gameManagerReference.tiles[0];

                    if (rayHit.transform.parent.parent == GameManager.gameManagerReference.chunkContainer.transform && !GameManager.gameManagerReference.doingAnAction)
                    {
                        ChunkController chunkController = rayHit.transform.parent.GetComponent<ChunkController>();
                        int tileSelected = chunkController.TileGrid[System.Array.IndexOf(chunkController.TileObject, rayHit.transform.gameObject)];

                        if (canInteract[tileSelected] == true)
                        {
                            workIcon.GetComponent<SpriteRenderer>().sprite = spritesRenders[tileSelected];
                            if (GInput.GetMouseButtonDown(1) || (GInput.leagerInput.platform == "Mobile" && !GameManager.gameManagerReference.cancelPlacing))
                            {
                                DisplayForTile(tileSelected, rayHit);
                            }
                        }
                    }
                }
            }
        }
    }

    public void DisplayForTile(int tile, RaycastHit2D rayHit)
    {
        if (tile == 15)
        {
            rayHit.transform.GetChild(0).GetComponent<Box>().ToggleItems();
        }


        if (tile == 16)
        {
            if (GameObject.Find("CraftMenu") == null)
            {
                GameObject a = Instantiate(CraftMenu, GameObject.Find("UI Menus").transform);
                a.name = "CraftMenu";
                a.GetComponent<CraftMenuController>().InvokeMenu(rayHit.transform);
            }
        }

        if (tile == 89)
        {
            if (GameManager.gameManagerReference.InGame)
            {
                MenuController.menuController.PlanetMenuDeploy(rayHit.transform.GetChild(0).GetComponent<ResourceLauncher>());
            }
        }

        if (tile == 102)
        {
            rayHit.transform.GetChild(0).GetComponent<Box>().ToggleItems();
        }
    }
}
