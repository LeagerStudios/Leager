using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileProperties : MonoBehaviour
{
    public int parentTile = 0;
    public GameObject associatedTile;

    public bool canDropStoredItems;
    public List<string> storedItems = new List<string>();
    public List<string> info = new List<string>();

    public float rotation;

    public void Start()
    {

    }

    public string Export()
    {
        List<string> toEncrypt = new List<string>();
        toEncrypt.Add(parentTile.ToString());
        toEncrypt.Add(canDropStoredItems.ToString().ToLower());
        toEncrypt.Add(string.Join("#", storedItems.ToArray()));
        toEncrypt.Add(string.Join("#", info.ToArray()));
        toEncrypt.Add(rotation.ToString());

        return string.Join("@", toEncrypt);
    }

    public void Load(string args)
    {
        string[] decode = args.Split('@');
    }
}
