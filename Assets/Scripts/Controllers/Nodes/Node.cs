using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> connections;
    public int connectionLimit;

    public NodeConnectionStatus AddConnection(Node connection)
    {
        NodeConnectionStatus status = new NodeConnectionStatus();
        status.Initialize();

        if(connection != this)
        {
            if(typeof(NodeSource) == connection.GetType())
            {
                status.closed = true;
            }
            else
            {

            }
        }

        return status;
    }
}

public struct NodeConnectionStatus
{
    public bool closed;

    public void Initialize()
    {
        closed = false;
    }
}
