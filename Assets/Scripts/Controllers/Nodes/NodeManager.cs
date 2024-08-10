using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class NodeManager : MonoBehaviour
{
    public static NodeManager self;
    public List<Node> nodes;
    public System.Type sourceNode = typeof(SourceNode);
    public System.Type endPointNode = typeof(EndPointNode);

    private void Awake()
    {
        self = this;
    }

    public void Update()
    {
        List<SourceNode> sources = new List<SourceNode>();
        List<EndPointNode> endPoints = new List<EndPointNode>();

        foreach(Node node in nodes)
        {
            System.Type nodeType = node.GetType();

            if (nodeType == sourceNode)
            {
                sources.Add((SourceNode)node);
            }

            if (nodeType == endPointNode)
            {
                endPoints.Add((EndPointNode)node);
            }

            if (!node.isBattery)
            {
                node.Power = 0;
            }
        }

        foreach(SourceNode node in sources)
        {
            node.UpdatePower(0, new HashSet<Node>());
        }

        foreach (EndPointNode node in endPoints)
        {
            node.Update();

        }
    }

    public void RegisterNode(Node node)
    {
        nodes.Add(node);
    }

    public void DeleteNode(Node node)
    {
        foreach(Node connection in node.connections)
        {
            connection.RemoveConnection(node);
        }

        nodes.Remove(node);
    }
}
