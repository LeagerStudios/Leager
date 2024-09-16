using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Node> connections = new List<Node>();
    public float Power { get; set; }
    public int connectionLimit = 4;
    public bool isBattery = false;
    public Vector3Int index;
    public Vector2 position;
    public bool updatedThis = false;


    public bool AddConnection(Node connection)
    {
        if(connection != this && !connections.Contains(connection) && connections.Count < connectionLimit)
        {
            connections.Add(connection);
            updatedThis = true;
            return true;
        }
        return false;
    }

    public bool AddConnectionRecursive(Node connection)
    {
        if (connection != this && !connections.Contains(connection) && connections.Count < connectionLimit)
        {
            connections.Add(connection);
            updatedThis = true;
            if (connection.AddConnection(this)) return true;
            else
            {
                connections.Remove(connection);
                updatedThis = false;
                return false;
            }
        }
        return false;
    }


    public void RemoveConnection(Node node)
    {
        if (connections.Contains(node))
        {
            connections.Remove(node);
        }
    }

    public void RemoveConnectionRecursive(Node node)
    {
        if (connections.Contains(node))
        {
            connections.Remove(node);
            node.RemoveConnection(this);
        }
    }

    public virtual void UpdatePower(float power, NodePath updatedNodes)
    {
        Power += power;
        int i = 0;
        updatedNodes.Add(this);
        updatedThis = true;
        foreach (Node node in connections)
        {
            if (!updatedNodes.Contains(node))
            {
                i++;

                if (i > 1)
                {
                    updatedNodes.ResetRender();
                    updatedNodes.Add(this);
                }

                float value = Power / (connections.Count - (connections.Count > 1 ? 1 : 0));
                node.UpdatePower(value, updatedNodes.ClonePath());
            }
        }

        if (i == 0)
        {
            if (connections.Count > 0)
            {
                NodeManager.self.AddPath(updatedNodes);
            }
        }
    }
}

public class SourceNode : Node
{
    public float TargetOutputPower { get; set; }
    public float OutputPower { get; private set; }
    public float OutputPowerDuration { get; set; }
    
    public void Update()
    {
        if (OutputPowerDuration > 0)
        {
            OutputPower = TargetOutputPower;
            OutputPowerDuration -= Time.deltaTime;
        }
        else
        {
            OutputPower = 0;
        }

        base.UpdatePower(OutputPower, new NodePath());
    }
}

public class EndPointNode : Node
{
    public List<INodeEndPoint> endPoints = new List<INodeEndPoint>();

    public override void UpdatePower(float power, NodePath updatedNodes)
    {
        Debug.Log("EndPoint previous power: " + Power);
        Power += power;
        Debug.Log("EndPoint received power: " + power);
        Debug.Log("EndPoint current power: " + Power);

        foreach (INodeEndPoint endPoint in endPoints)
        {
            endPoint.Update(this);
        }

        updatedThis = true;
        updatedNodes.Add(this); // Ensure it's marked as updated
        updatedNodes.MarkAsConnected();
        NodeManager.self.AddPath(updatedNodes);
    }

    public void Update()
    {
        foreach(INodeEndPoint endPoint in endPoints)
        {
            endPoint.Update(this);
        }
    }
}


public interface INodeEndPoint
{
    void Update(EndPointNode endPoint);
}

public class NodePath
{
    public List<Node> path = new List<Node>();
    public List<Node> render = new List<Node>();
    public bool connected = false;

    public void Add(Node element)
    {
        path.Add(element);
        render.Add(element);
    }

    public bool Contains(Node element)
    {
        return path.Contains(element);
    }

    public void ResetRender()
    {
        render = new List<Node>();
    }

    public void MarkAsConnected()
    {
        connected = true;
    }

    public NodePath ClonePath()
    {
        NodePath clone = new NodePath
        {
            path = new List<Node>(path),
            render = new List<Node>(render),
            connected = connected
        };
        return clone;
    }
}