using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDrill : MonoBehaviour, INodeEndPoint
{
    public NodeInstance nodeInstance;


    void Start()
    {
        nodeInstance = GetComponent<NodeInstance>();

        if (transform.parent.GetComponent<TileProperties>())
        {
            TileProperties properties = transform.parent.GetComponent<TileProperties>();
            properties.attach = GetComponent<NodeInstance>();
        }
        else
        {
            TileProperties properties = transform.parent.gameObject.AddComponent<TileProperties>();
            properties.parentTile = 92;
            properties.canDropStoredItems = true;
            properties.attach = nodeInstance;
        }
    }

    void Update()
    {
        
    }

    public void Update(EndPointNode endPoint)
    {

    }
}
