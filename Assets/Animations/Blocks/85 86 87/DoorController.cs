using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour {

    public bool isOpen = false;
    TileProperties tileProperties;


    void Start ()
    {
        tileProperties = transform.parent.GetComponent<TileProperties>();

        if (tileProperties == null)
        {
            tileProperties = transform.parent.gameObject.AddComponent<TileProperties>();
            tileProperties.parentTile = 86;
            tileProperties.info.Add("true");
            tileProperties.info.Add("true");


            GetComponent<BoxCollider2D>().enabled = false;
            isOpen = true;
        }
        else
        {
            if (tileProperties.info[0] == "true")
            {
                GetComponent<BoxCollider2D>().enabled = false;
                isOpen = true;
            }
            else
            {
                GetComponent<BoxCollider2D>().enabled = true;
                isOpen = false;
            }
        }

        Refresh(tileProperties.info[1] == "true");
    }
	
	void Update ()
    {
        if (GInput.GetMouseButtonDown(1) || (GInput.leagerInput.platform == "Mobile" && GInput.GetMouseButtonDown(0) && !GInput.CancelPlacing))
        {
            bool canInteract = true;

            if (Mathf.Abs(GameManager.gameManagerReference.mouseCurrentPosition.x - transform.position.x) > 0.5f) canInteract = false;
            else if (Mathf.Abs(GameManager.gameManagerReference.mouseCurrentPosition.y - (transform.position.y + 0.5f)) > 1f) canInteract = false;

            if (canInteract && isOpen)
            {
                canInteract = false;
                if (Mathf.Abs(GameManager.gameManagerReference.player.transform.position.x - transform.position.x) > 0.75f) canInteract = true;
                else if (Mathf.Abs(GameManager.gameManagerReference.player.transform.position.y - (transform.position.y + 0.5f)) > 1.65f) canInteract = true;
            }

            if (canInteract)
            {
                isOpen = !isOpen;
                tileProperties.info[0] = isOpen.ToString().ToLower();
                tileProperties.info[1] = (GameManager.gameManagerReference.player.transform.position.x < transform.position.x).ToString().ToLower();
                Refresh(tileProperties.info[1] == "true");
                tileProperties.CommitToChunk();
            }
        }
	}

    public void Refresh(bool left)
    {
        if (!isOpen)
        {
            GetComponent<BoxCollider2D>().enabled = true;
            GetComponent<BoxCollider2D>().offset = new Vector2(0.38f * ManagingFunctions.ParseBoolToInt(!left), 0.5f);
            if (left)
            {
                transform.parent.GetComponent<BlockAnimationController>().GotoFrame(1);
                transform.parent.parent.GetChild(transform.parent.GetSiblingIndex() + 1).GetComponent<BlockAnimationController>().GotoFrame(1);
            }
            else
            {
                transform.parent.GetComponent<BlockAnimationController>().GotoFrame(2);
                transform.parent.parent.GetChild(transform.parent.GetSiblingIndex() + 1).GetComponent<BlockAnimationController>().GotoFrame(2);
            }
        }
        else
        {
            GetComponent<BoxCollider2D>().enabled = false;
            transform.parent.GetComponent<BlockAnimationController>().GotoFrame(0);
            transform.parent.parent.GetChild(transform.parent.GetSiblingIndex() + 1).GetComponent<BlockAnimationController>().GotoFrame(0);
        }
    }
}
