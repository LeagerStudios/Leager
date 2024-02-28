using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechStack : MonoBehaviour
{
    public int tile = 0;
    public TechStack parent;
    public GameObject[] nodes;
    public int id = 0;
    public bool unlocked = true;
    public bool fullyUnlocked = true;
    public int[] unlockOnFullUnlock;
    public bool hasSubtech = false;

    void OnValidate()
    {
        transform.GetChild(0).GetComponent<Image>().sprite = GameObject.Find("GameManager").GetComponent<GameManager>().tiles[tile];
    }

    void Start()
    {
        TriggerTech(true);
    }

    public void TriggerTech(bool val)
    {
        unlocked = val;

        GetComponent<Button>().interactable = val;
        GetComponent<Image>().enabled = val;
        transform.GetChild(0).GetComponent<Image>().enabled = val;

        foreach (GameObject node in nodes)
            node.SetActive(val);
    }

    public void UnlockFully()
    {

    }
}
