using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkItemController : MonoBehaviour {

    [SerializeField] public LayerMask tilesMasks;
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
            Vector3 mousePos = GameManager.gameManagerReference.mouseCurrentPosition;
            mousePos.y++;
            RaycastHit2D rayHit = Physics2D.Raycast(mousePos, Vector2.down, 1f, tilesMasks);
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
                            if (GInput.GetMouseButtonDown(1) || (GInput.leagerInput.platform == "Mobile" && !GameManager.gameManagerReference.cancelPlacing && GInput.GetMouseButtonDown(0)))
                            {
                                DisplayForTile(tileSelected, rayHit);
                            }
                        }
                    }
                }
            }

            if (GameManager.gameManagerReference.doingAnAction)
            {
                if (GInput.GetMouseButton(0))
                {
                    RaycastHit2D rayHitt = Physics2D.Raycast(mousePos, Vector2.down, 1f, tilesMasks);
                    if (rayHitt.transform != null)
                        if (rayHitt.transform.parent != null)
                            if (rayHitt.transform.parent.GetComponent<ChunkController>() != null)
                                rayHitt.transform.parent.GetComponent<ChunkController>().ClickedTile(rayHitt.transform.gameObject);

                    Debug.DrawRay(GameManager.gameManagerReference.player.transform.position, GameManager.gameManagerReference.mouseCurrentPosition - GameManager.gameManagerReference.player.transform.position, Color.blue);
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
