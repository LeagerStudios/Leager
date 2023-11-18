using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour {

    public bool isOpen = false;

	void Start ()
    {
		if(transform.parent.GetComponent<BlockAnimationController>().frame == 0)
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
	
	void Update ()
    {
        if (GInput.GetMouseButtonDown(1))
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
                if (isOpen)
                {
                    GetComponent<BoxCollider2D>().enabled = true;
                    if(GameManager.gameManagerReference.player.transform.position.x < transform.position.x)
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
                isOpen = !isOpen;
            }
        }
	}
}
