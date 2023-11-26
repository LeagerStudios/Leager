using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileProperties : MonoBehaviour
{
    public int parentTile = 0;
    public GameObject associatedTile;

    public bool canDropStoredItems;
    public List<string> storedItems = new List<string>();

    public float rotation;
}
