using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileProperties : MonoBehaviour
{
    public int parentTile = 0;
    public ChunkController chunk;
    public ITilePropertiesAttach attach;

    public bool canDropStoredItems = true;
    public bool destroy = false;
    public List<string> storedItems = new List<string>();
    public List<string> info = new List<string>();

    public int rotation;

    public void Start()
    {
        chunk = transform.GetComponentInParent<ChunkController>();
    }

    public void CallAttach()
    {
        if (attach != null) attach.Break();
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
            info = new List<string>();
        rotation = System.Convert.ToInt32(decode[4]);
    }

    public void CommitToChunk()
    {
        string a = Export();
        chunk.TilePropertiesArr[transform.GetSiblingIndex()] = a;
        GameManager.gameManagerReference.allMapProp[chunk.tilesToChunk + transform.GetSiblingIndex()] = a;
    }
}

public interface ITilePropertiesAttach
{
    void Break();
}
