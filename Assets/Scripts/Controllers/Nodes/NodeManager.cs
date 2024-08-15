using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class NodeManager : MonoBehaviour
{
    public static NodeManager self;
    public List<Node> nodes;
    public System.Type sourceNode = typeof(SourceNode);
    public System.Type endPointNode = typeof(EndPointNode);
    public Dictionary<Vector3Int, Node> nodesDictionary;

    public List<List<Node>> nodesPaths = new List<List<Node>>();

    private void Awake()
    {
        self = this;
    }

    public void Start()
    {
        nodes = new List<Node>();
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
            node.UpdatePower(0, new List<Node>());
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

    public void AddPath(List<Node> path)
    {
        nodesPaths.Add(path);
    }

    public Texture nodeConnectionTexture;

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            Camera camera = Camera.main;

            foreach (List<Node> list in nodesPaths)
            {
                foreach (Node segment in list)
                {
                    
                }
            }

            nodesPaths = new List<List<Node>>();
        }
    }
}
