using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitiesManager : MonoBehaviour {

    public static EntitiesManager self;
    public int currentEntities = 0;
    public int entityLimit = 0;
    public bool canNaturalSpawn = true;

    private void Awake()
    {
        currentEntities = 0;
        entityLimit = 9999;
        canNaturalSpawn = true;
        self = this;
    }


    public void UpdateEntities(GameObject[] loadedChunks)
    {
        EntityUnloader[] loadedEntities = transform.GetComponentsInChildren<EntityUnloader>(true);

        for (int i = 0; i < loadedEntities.Length; i++)
        {
            Transform child = loadedEntities[i].target.gameObject.transform;
            bool makeActive = false;

            foreach (GameObject loadedChunk in loadedChunks)
            {
                if (child.position.x >= loadedChunk.transform.position.x && child.position.x <= loadedChunk.transform.position.x + 16)
                {
                    makeActive = true;
                }
            }

            if (loadedChunks.Length > 0)
                if (child.position.x < 0 || child.position.x > GameManager.gameManagerReference.WorldWidth * 16 || child.position.y < 0f)
                {
                    Destroy(child.gameObject);
                }

            child.gameObject.SetActive(makeActive);

            if(!makeActive)
            if (loadedEntities[i].canDespawn)
            {
                    Destroy(child.gameObject);
            }
        }

        currentEntities = 0;
        entityLimit = MenuController.menuController.chunksOnEachSide * 10;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                currentEntities++;
            }
        }
        canNaturalSpawn = currentEntities < entityLimit;
    }
}
