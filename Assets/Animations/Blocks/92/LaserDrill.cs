using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDrill : MonoBehaviour, INodeEndPoint
{
    public NodeInstance nodeInstance;
    public TileProperties tileProperties;

    void Start()
    {
        nodeInstance = GetComponent<NodeInstance>();
        
    }

    void Update()
    {
        if(tileProperties == null)
            tileProperties = GetComponentInParent<TileProperties>();
    }

    public void Update(EndPointNode endPoint)
    {
        
    }
}
