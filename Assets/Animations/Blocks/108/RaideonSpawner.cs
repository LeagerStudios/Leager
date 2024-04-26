using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaideonSpawner : MonoBehaviour
{
    bool activated = false;
    public int raideonLimit = 3;
    public List<ENTITY_Raideon> raideons = new List<ENTITY_Raideon>();

    void Start()
    {
        
    }

    void Update()
    {
        if (activated)
            if (!GameManager.gameManagerReference.isNetworkClient)
                if (raideons.Count < raideonLimit)
                {
                    AddOne();
                }

        if (!activated)
        {
            if(Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position) < 3)
            {
                activated = true;
            }
        }
        else
        {
            if (Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position) > 30)
            {
                activated = false;
            }
        }
    }

    public void AddOne()
    {
        ENTITY_Raideon newRaideon = (ENTITY_Raideon)GameManager.gameManagerReference.SpawnEntity(Entities.Raideon, null, transform.position + Vector3.up);
        newRaideon.associatedSpawner = this;
        raideons.Add(newRaideon);
    }

    public void LoseOne(ENTITY_Raideon lose)
    {
        raideons.Remove(lose);
    }
}
