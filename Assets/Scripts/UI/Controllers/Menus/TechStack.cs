using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechStack : MonoBehaviour
{
    public int tile = 0;
    public TechStack parent;
    public GameObject[] nodes;
    public Sprite obtainedWith;
    public bool unlocked = false;
    public bool fullyUnlocked = false;
    public int[] unlockOnFullUnlock;

    void OnValidate()
    {
        transform.GetChild(0).GetComponent<Image>().sprite = GameObject.Find("GameManager").GetComponent<GameManager>().tiles[tile];
    }

    void Start()
    {
        if (parent)
            TriggerTech(false);
        else
        {
            TriggerTech(false);
        
            TriggerTech(true);
        }
        GetComponent<Button>().onClick.AddListener(() => TechManager.techTree.UnlockBlock(tile));
    }

    public void TriggerTech(bool val)
    {
        unlocked = val;

        if (unlockOnFullUnlock.Length == 0)
        {
            GetComponent<Button>().interactable = val;
            GetComponent<Image>().enabled = val;
            transform.GetChild(0).GetComponent<Image>().enabled = val;

            foreach (GameObject node in nodes)
                node.SetActive(val);
        }
        else
        {
            if (!val)
            {
                GetComponent<Button>().interactable = true;
                //GetComponent<Button>().colors.normalColor = TechManager.techTree.notFullColor;
                GetComponent<Image>().enabled = false;
                transform.GetChild(0).GetComponent<Image>().enabled = false;

                foreach (GameObject node in nodes)
                    node.SetActive(val);
            }
            else
            {
                GetComponent<Button>().interactable = false;
                GetComponent<Image>().enabled = true;
                transform.GetChild(0).GetComponent<Image>().enabled = true;

                foreach (GameObject node in nodes)
                    node.SetActive(true);
            }
        }
    }

    public void UnlockFully()
    {
        fullyUnlocked = true;
        GetComponent<Button>().interactable = true;
        foreach(int toUnlock in unlockOnFullUnlock)
        {
            TechManager.techTree.UnlockBlock(toUnlock);
        }
    }

    public void UnlockedItem(int item)
    {
        if (item == tile)
        {
            if (unlocked == false)
            {
                TriggerTech(true);
            }
            else if (fullyUnlocked == false && unlockOnFullUnlock.Length > 0)
            {
                UnlockFully();
            }
        }
    }
}
