using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechManager : MonoBehaviour
{
    public static TechManager techTree;
    public List<int> unlockedItems = new List<int>();

    public Color notFullColor;
    public Color fullColor;

    public void Start()
    {
        techTree = this;
    }

    public void StartUnlocks(List<int> unlocks)
    {
        
    }
    
    public void UnlockBlock(int block)
    {
        print("e");
        TechStack[] techStacks = transform.GetComponentsInChildren<TechStack>(true);
        foreach (TechStack techStack in techStacks) techStack.UnlockedItem(block);
    }
}
