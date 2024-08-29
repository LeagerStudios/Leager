using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class NodeManager : MonoBehaviour
{
    public static NodeManager self;
    public List<Node> nodes;
    public System.Type sourceNode = typeof(SourceNode);
    public System.Type endPointNode = typeof(EndPointNode);
    public Dictionary<Vector3Int, Node> nodesDictionary = new Dictionary<Vector3Int, Node>();

    public List<NodePath> nodesPaths = new List<NodePath>();
    public GameObject nodeConnectionPrefab;
    public List<GameObject> nodeConnections = new List<GameObject>();

    public Node nodeSelected = null;

    public Material nodeMaterial;
    public Material disconnectedNodeMaterial;

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
        if (GameManager.gameManagerReference.InGame)
        {

            if (GInput.GetKeyDown(KeyCode.C))
            {
                nodeSelected = null;
            }

            List<SourceNode> sources = new List<SourceNode>();
            List<EndPointNode> endPoints = new List<EndPointNode>();

            foreach (Node node in nodes)
            {
                System.Type nodeType = node.GetType();

                if (nodeType == sourceNode)
                {
                    sources.Add((SourceNode)node);
                }
                else if(node.updatedThis == false)
                {
                    List<Node> connections = new List<Node>(node.connections);
                    foreach (Node connection in connections)
                    {
                        if (!connection.updatedThis)
                        {
                            connection.RemoveConnection(node);
                            node.RemoveConnection(connection);
                        }
                    }
                }

                if (nodeType == endPointNode)
                {
                    endPoints.Add((EndPointNode)node);
                }


                if (!node.isBattery)
                {
                    node.Power = 0;
                }

                node.updatedThis = false;
            }


            foreach (SourceNode node in sources)
            {
                node.Update();
            }

            foreach (EndPointNode node in endPoints)
            {
                node.Update();
            }

        }
    }

    public Node RegisterNode(Node node)
    {
        if (nodesDictionary.TryGetValue(node.index, out Node outValue))
        {
            node = outValue;
        }
        else
        {
            nodes.Add(node);
            nodesDictionary.Add(node.index, node);
        }
        return node;
    }

    public void DeleteNode(Node node)
    {
        foreach (Node connection in node.connections)
        {
            connection.RemoveConnection(node);
        }

        nodes.Remove(node);
        nodesDictionary.Remove(node.index);
    }

    public void AddPath(NodePath path)
    {
        nodesPaths.Add(path);
    }

    void LateUpdate()
    {
        if (GameManager.gameManagerReference.InGame)
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
                List<Node> list = nodesPaths[i].render;
                LineRenderer lineRenderer = nodeConnections[i].GetComponent<LineRenderer>();
                lineRenderer.positionCount = list.Count;
                lineRenderer.rendererPriority = i;

                if (nodesPaths[i].connected)
                {
                    lineRenderer.material = nodeMaterial;
                }
                else
                {
                    lineRenderer.material = disconnectedNodeMaterial;
                }

                for (int idx = 0; idx < list.Count; idx++)
                {
                    lineRenderer.SetPosition(idx, list[idx].position);
                }
            }

            nodesPaths = new List<NodePath>();
        }
    }
}
