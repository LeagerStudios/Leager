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
    public GameObject nodeConnectionPrefab;
    public List<GameObject> nodeConnections;

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

        foreach (Node node in nodes)
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

        foreach (SourceNode node in sources)
        {
            node.UpdatePower(0, new List<Node>());
        }

        foreach (EndPointNode node in endPoints)
        {
            node.Update();
        }
    }

    public bool TryGetNode(Vector3Int index, out Node node)
    {
        if(nodesDictionary.TryGetValue(index, out Node outValue))
        {
            node = outValue;
        }
        node = null;
        return false;
    }

    public void RegisterNode(Node node, Vector3Int index)
    {
        nodes.Add(node);
        nodesDictionary.Add(index, node);
    }

    public void DeleteNode(Node node)
    {
        foreach (Node connection in node.connections)
        {
            connection.RemoveConnection(node);
        }

        nodes.Remove(node);
    }

    public void AddPath(List<Node> path)
    {
        nodesPaths.Add(path);

    }

    void LateUpdate()
    {
        while (nodeConnections.Count < nodesPaths.Count)
        {
            GameObject nodeConnection = Instantiate(nodeConnectionPrefab, transform);
            nodeConnections.Add(nodeConnection);
        }

        while (nodeConnections.Count > nodesPaths.Count)
        {
            Destroy(nodeConnections[nodesPaths.Count]);
            nodeConnections.Remove(nodeConnections[nodesPaths.Count]);
        }

        for (int i = 0; i < nodesPaths.Count; i++)
        {
            List<Node> list = nodesPaths[i];
            LineRenderer lineRenderer = nodeConnections[i].GetComponent<LineRenderer>();
            lineRenderer.positionCount = list.Count;
            for(int idx = 0; i < list.Count; i++)
            {
                lineRenderer.SetPosition(idx, list[idx].position);
            }
        }

        nodesPaths = new List<List<Node>>();
    }
}
