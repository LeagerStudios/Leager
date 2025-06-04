using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public int state = -1;

    void Start()
    {
        StartCoroutine(Tutorial());
    }

    void Update()
    {
        
    }

    public IEnumerator Tutorial()
    {
        SetState(0);

        while (!GInput.GetKey(KeyCode.A))
        {
            yield return new WaitForEndOfFrame();
            StackBar.stackBarController.InventoryDeployed = false;
        }

        while (!GInput.GetKey(KeyCode.D))
        {
            yield return new WaitForEndOfFrame();
            StackBar.stackBarController.InventoryDeployed = false;
        }

        while (!GInput.GetKey(KeyCode.W))
        {
            yield return new WaitForEndOfFrame();
            StackBar.stackBarController.InventoryDeployed = false;
        }

        SetState(1);

        while (!StackBar.stackBarController.InventoryDeployed)
        {
            yield return new WaitForEndOfFrame();
        }

        while (StackBar.stackBarController.InventoryDeployed)
        {
            yield return new WaitForEndOfFrame();
        }

        StackBar.stackBarController.idx = Random.Range(3, 9);
        StackBar.stackBarController.UpdateStacks();

        SetState(2);

        bool canExit = false;
        while(!canExit)
        {
            if(StackBar.stackBarController.CurrentItem == 24)
            {
                if (GameManager.gameManagerReference.usingTool)
                {
                    if (TechManager.techTree.unlockedItems.Contains(53))
                    {
                        canExit = true;
                    }
                    else
                    {
                        SetState(4);
                    }
                }
                else
                {
                    SetState(3);
                }
            }
            else
            {
                SetState(2);
            }

            yield return new WaitForEndOfFrame();
        }

        SetState(5);

        while (state < 6)
        {
            yield return new WaitForEndOfFrame();
        }

    }


    public void SetState(int i)
    {
        state = i;

        for (int e = 0; e < transform.Find(GInput.leagerInput.platform).childCount; e++)
        {
            transform.Find(GInput.leagerInput.platform).GetChild(e).gameObject.SetActive(e == state);
        }
    }
}
