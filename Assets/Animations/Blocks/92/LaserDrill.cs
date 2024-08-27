using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDrill : MonoBehaviour, INodeEndPoint
{
    public NodeInstance nodeInstance;


    void Start()
    {
        nodeInstance = GetComponent<NodeInstance>();
    }

    void Update()
    {
        
    }

    public void Update(EndPointNode endPoint)
    {
        transform.parent.GetComponent<SpriteRenderer>().color = endPoint.Power > 0f ? Color.white : Color.gray;
    }
}
