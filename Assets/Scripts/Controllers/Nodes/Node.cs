using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Node> connections = new List<Node>();
    public float Power { get; set; }
    public int connectionLimit = 3;
    public bool isBattery = false;
    public Vector3Int index;
    public Vector2 position;


    public void AddConnection(Node connection)
    {
        if(connection != this && !connections.Contains(connection) && connections.Count < connectionLimit)
        {
            connections.Add(connection);
        }
    }

    public void AddConnectionRecursive(Node connection)
    {
        if (connection != this && !connections.Contains(connection) && connections.Count < connectionLimit)
        {
            connections.Add(connection);
            connection.AddConnection(this);
        }
    }


    public void RemoveConnection(Node node)
    {
        if (connections.Contains(node))
        {
            connections.Remove(node);
        }
    }

    public virtual void UpdatePower(float power, List<Node> updatedNodes)
    {
        Power += power;
        int i = 0;
        updatedNodes.Add(this);
        foreach (Node node in connections)
        {
            if (!updatedNodes.Contains(node))
            {
                i++;
                float value = Power / (connections.Count - (connections.Count > 1 ? 1 : 0));
                node.UpdatePower(value, new List<Node>(updatedNodes));
            }
        }

        if (i == 0)
        {
            if (connections.Count > 0)
            {
                NodeManager.self.AddPath(new NodePath(updatedNodes, false));
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

        base.UpdatePower(OutputPower, new List<Node>() { this });
    }
}

public class EndPointNode : Node
{
    public List<INodeEndPoint> endPoints = new List<INodeEndPoint>();

    public override void UpdatePower(float power, List<Node> updatedNodes)
    {
        Debug.Log("EndPoint previous power: " + Power);
        Power += power;
        Debug.Log("EndPoint received power: " + power);
        Debug.Log("EndPoint current power: " + Power);

        foreach (INodeEndPoint endPoint in endPoints)
        {
            endPoint.Update(this);
        }

        updatedNodes.Add(this); // Ensure it's marked as updated
        NodeManager.self.AddPath(new NodePath(updatedNodes, true));
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
    public bool connected = false;

    public NodePath(List<Node> pathParam, bool ended)
    {
        path = pathParam;
        connected = ended;
    }
}