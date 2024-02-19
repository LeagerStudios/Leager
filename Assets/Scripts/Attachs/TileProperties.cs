using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileProperties : MonoBehaviour
{
    public int parentTile = 0;
    public GameObject associatedTile;

    public bool canDropStoredItems = true;
    public List<string> storedItems = new List<string>();
    public List<string> info = new List<string>();

    public int rotation;

    public void Start()
    {

    }

    public string Export()
    {
        List<string> toEncrypt = new List<string>
        {
            parentTile.ToString(),
            canDropStoredItems.ToString().ToLower(),
            string.Join("#", storedItems.ToArray()),
            string.Join("#", info.ToArray()),
            rotation.ToString()
        };

        return string.Join("@", toEncrypt);
    }

    public void Load(string args)
    {
        string[] decode = args.Split('@');
        parentTile = System.Convert.ToInt32(decode[0]);
        canDropStoredItems = System.Convert.ToBoolean(decode[1]);
        if (decode[2] != "")
            storedItems = new List<string>(decode[2].Split('#'));
        else
            storedItems = new List<string>();
        if (decode[3] != "")
            info = new List<string>(decode[3].Split('#'));
        else
            storedItems = new List<string>();
        rotation = System.Convert.ToInt32(decode[4]);

    }
}
