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
        nodes = new List<Node>();

        Load();
    }

    public void Start()
    {
        
        //TODO Load nodes

        //    public List<Node> connections = new List<Node>();
        //public float Power { get; set; }
        //public int connectionLimit = 4;
        //public bool isBattery = false;
        //public Vector3Int index;
        //public Vector2 position;
        //public bool updatedThis = false;

        //source node extra
        //public float TargetOutputPower { get; set; }
        //public float OutputPower { get; private set; }
        //public float OutputPowerDuration { get; set; }


    }

    public void Save()
    {
        List<string> nodes = new List<string>();
      

        foreach(Node nodeData in this.nodes)
        {
            List<string> node = new List<string>();

            switch (nodeData)
            {
                case SourceNode source:
                    node.Add("source");
                    break;
                case EndPointNode endPoint:
                    node.Add("endpoint");
                    break;
                default:
                    node.Add("node");
                    break;
            }

            List<string> connections = new List<string>();
            foreach (Node connection in nodeData.connections)
            {
                connections.Add(connection.index.x + "/" + connection.index.y + "/" + connection.index.z);
            }
            node.Add(string.Join(":", connections));
            node.Add(nodeData.connectionLimit + "");
            node.Add(nodeData.isBattery + "");
            node.Add(nodeData.index.x + "/" + nodeData.index.y + "/" + nodeData.index.z);
            node.Add(nodeData.position.x + "/" + nodeData.position.y);

            if(nodeData.GetType() == sourceNode)
            {
                node.Add(((SourceNode)nodeData).TargetOutputPower + "");
                node.Add(((SourceNode)nodeData).OutputPowerDuration + "");
            }


            nodes.Add(string.Join(";", node));
        }
        
        DataSaver.SaveStats(nodes.ToArray(), GameManager.gameManagerReference.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldName + @"/nodes.lgrsd");
    }

    public void Load()
    {
        nodes = new List<Node>();
        nodesDictionary = new Dictionary<Vector3Int, Node>();

        if (DataSaver.CheckIfFileExists(GameManager.gameManagerReference.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldName + @"/nodes.lgrsd"))
        {
            string[] savedData = DataSaver.LoadStats(GameManager.gameManagerReference.persistentDataPath + @"/worlds/" + GameManager.gameManagerReference.worldName + @"/nodes.lgrsd").SavedData;

            // First pass: Create nodes without connections
            foreach (string nodeData in savedData)
            {
                string[] nodeDetails = nodeData.Split(';');

                // Identify node type
                Node newNode;
                switch (nodeDetails[0])
                {
                    case "source":
                        newNode = new SourceNode();
                        break;
                    case "endpoint":
                        newNode = new EndPointNode();
                        break;
                    default:
                        newNode = new Node();
                        break;
                }

                // Load node properties (except connections for now)
                newNode.connectionLimit = int.Parse(nodeDetails[2]);
                newNode.isBattery = bool.Parse(nodeDetails[3]);

                // Load index
                print(nodeDetails[4]);
                string[] indexCoords = nodeDetails[4].Split('/');
                newNode.index = new Vector3Int(int.Parse(indexCoords[0]), int.Parse(indexCoords[1]), int.Parse(indexCoords[2]));

                // Load position
                string[] positionCoords = nodeDetails[5].Split('/');
                newNode.position = new Vector2(float.Parse(positionCoords[0]), float.Parse(positionCoords[1]));

                // Load source-specific properties if applicable
                if (newNode is SourceNode sourceNode)
                {
                    sourceNode.TargetOutputPower = float.Parse(nodeDetails[6]);
                    sourceNode.OutputPowerDuration = float.Parse(nodeDetails[7]);
                }

                nodes.Add(newNode);
                nodesDictionary.Add(newNode.index, newNode);
            }


            // Second pass: Establish connections
            for (int i = 0; i < savedData.Length; i++)
            {
                string[] nodeDetails = savedData[i].Split(';');
                Node currentNode = nodes[i];

                // Deserialize connections
                string[] connectionsData = nodeDetails[1].Split(':');
                foreach (string connection in connectionsData)
                {
                    if (connection != "")
                    {
                        string[] coords = connection.Split('/');
                        Vector3Int index = new Vector3Int(int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2]));
                        // Find the connected node by index
                        if (nodesDictionary.TryGetValue(index, out Node connectionNode))
                        {
                            currentNode.AddConnection(connectionNode);
                        }
                    }
                }
            }
        }
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
