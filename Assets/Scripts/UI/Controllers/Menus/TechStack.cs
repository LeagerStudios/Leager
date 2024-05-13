using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechStack : MonoBehaviour
{
    public int tile = 0;
    public TechStack parent;
    public GameObject[] nodes;
    public bool unlocked = false;
    public bool fullyUnlocked = false;
    public int[] unlockOnFullUnlock;

    public Sprite obtainedWith;
    public string description;
    public bool addPlusSimbol = false;

    void OnValidate()
    {
        transform.GetChild(0).GetComponent<Image>().sprite = GameObject.Find("GameManager").GetComponent<GameManager>().tiles[tile];
        transform.GetChild(1).GetComponent<Image>().enabled = addPlusSimbol;
    }

    public void Start1()
    {
        if (parent)
            Lock();
        else
        {
            Lock();
            Unlock();
        }
    }

    public void Lock()
    {
        unlocked = false;
        fullyUnlocked = false;


        GetComponent<Button>().interactable = false;
        GetComponent<Image>().enabled = false;
        transform.GetChild(0).GetComponent<Image>().enabled = false;
        transform.GetChild(1).GetComponent<Image>().enabled = false;

        foreach (GameObject node in nodes)
            node.SetActive(false);
    }

    public void Unlock()
    {
        if (!parent || parent.fullyUnlocked)
        {
            if (!unlocked)
            {
                unlocked = true;

                GetComponent<Button>().interactable = true;
                GetComponent<Image>().enabled = true;
                transform.GetChild(0).GetComponent<Image>().enabled = true;
                transform.GetChild(1).GetComponent<Image>().enabled = addPlusSimbol;
                GetComponent<Button>().colors = TechManager.techTree.notFullColor;

                transform.parent.GetComponent<TechManager>().unlockedItems.Add(tile);

                foreach (GameObject node in nodes)
                    node.SetActive(true);

                if (unlockOnFullUnlock.Length == 0)
                    UnlockFully();
            }
        }
    }

    public void UnlockFully()
    {
        if (!parent || parent.fullyUnlocked)
        {
            if (!fullyUnlocked)
            {
                unlocked = true;
                fullyUnlocked = true;
                GetComponent<Button>().interactable = true;
                GetComponent<Button>().colors = TechManager.techTree.fullColor;
                transform.parent.GetComponent<TechManager>().fullyUnlockedItems.Add(tile);

                foreach (int toUnlock in unlockOnFullUnlock)
                {
                    TechManager.techTree.UnlockBlock(toUnlock, false);
                }
            }
        }
    }

    public void UnlockedItem(int item)
    {
        if (item == tile)
        {
            if (unlocked == false)
            {
                Unlock();
            }
            else if (fullyUnlocked == false)
            {
                UnlockFully();
            }
        }
    }
}
