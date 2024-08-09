using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Node> connections;
    public float Power { get; set; }
    public int connectionLimit;
    public bool isBattery = false;

    public void AddConnection(Node connection)
    {
        if(connection != this && !connections.Contains(connection) && connections.Count < connectionLimit)
        {
            connections.Add(connection);
        }
    }

    public void RemoveConnection(Node node)
    {
        if (connections.Contains(node))
        {
            connections.Remove(node);
        }
    }

    public virtual void UpdatePower(float power, HashSet<Node> updatedNodes)
    {
        if (updatedNodes.Contains(this)) return;

        updatedNodes.Add(this);

        foreach (var node in connections)
        {
            float value = Power / (connections.Count - 1);
            node.Power += value; 
            node.UpdatePower(value, updatedNodes); 
        }
    }
}

public class SourceNode : Node
{
    public float TargetOutputPower { get; set; }
    public float OutputPower { get; private set; }
    public float OutputPowerDuration { get; set; }

    public override void UpdatePower(float power, HashSet<Node> updatedNodes)
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

        Power = OutputPower; // The source node's power is its output power

        // Now, distribute power to attached nodes
        if(Power > 0f)
        {
            base.UpdatePower(Power, updatedNodes);
        }
    }
}

public class EndPointNode : Node
{
    public List<INodeEndPoint> endPoints = new List<INodeEndPoint>();

    public override void UpdatePower(float power, HashSet<Node> updatedNodes)
    {
        Power += power;
        Debug.Log("EndPoint received power: " + Power);
        Debug.Log("EndPoint current power: " + Power);

        foreach(INodeEndPoint endPoint in endPoints)
        {
            endPoint.Update(this);
        }

        updatedNodes.Add(this); // Ensure it's marked as updated
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