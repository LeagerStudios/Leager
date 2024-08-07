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

    public virtual void UpdatePower(HashSet<Node> updatedNodes)
    {
        if (updatedNodes.Contains(this)) return;

        updatedNodes.Add(this);

        foreach (var node in connections)
        {
            node.Power += Power / (connections.Count - 1); 
            node.UpdatePower(updatedNodes); 
        }
    }
}

class SourceNode : Node
{
    public float TargetOutputPower { get; set; }
    public float OutputPower { get; private set; }
    public float OutputPowerDuration { get; set; }

    public override void UpdatePower(HashSet<Node> updatedNodes)
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
            base.UpdatePower(updatedNodes);
        }
    }
}

class EndPointNode : Node
{
    public override void UpdatePower(HashSet<Node> updatedNodes)
    {
        // For this example, the EndPoint just consumes the power it receives
        Debug.Log("EndPoint received power: " + Power);

        updatedNodes.Add(this); // Ensure it's marked as updated
    }
}


