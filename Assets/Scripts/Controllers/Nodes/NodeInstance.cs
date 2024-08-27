using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Node, SourceNode, EndPointNode
}

public class NodeInstance : MonoBehaviour, ITilePropertiesAttach
{
    public GameObject[] nodeObjects;
    public NodeType[] nodeTypes;
    public Node[] nodes;
    public int tile;

    void Start()
    {
        if (transform.parent.GetComponent<TileProperties>() == null)
        {
            TileProperties properties = transform.parent.gameObject.AddComponent<TileProperties>();
            properties.parentTile = tile;
            properties.canDropStoredItems = true;
            properties.attach = this;
        }
        else
        {
            TileProperties properties = GetComponentInParent<TileProperties>();
            properties.attach = this;
        }

        nodes = new Node[nodeObjects.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            Node node;

            if (nodeTypes[i] == NodeType.Node)
            {
                node = new Node();
            }
            else if (nodeTypes[i] == NodeType.SourceNode)
            {
                node = new SourceNode();
            }
            else
            {
                node = new EndPointNode();
            }

            node.position = nodeObjects[i].transform.position;
            node.index = new Vector3Int((int)transform.position.x, (int)transform.position.y, i);
            NodeManager.self.RegisterNode(node);

            if (node.GetType() == NodeManager.self.endPointNode)
            {
                ((EndPointNode)node).endPoints.Add(GetComponent<INodeEndPoint>());
            }

            nodes[i] = node;
        }
    }

    public void Update()
    {
        if (GInput.GetMouseButtonDown(1))
        {
            if (Vector2.Distance(transform.position, GameManager.gameManagerReference.mouseCurrentPosition) < 0.2f)
            {
                if (NodeManager.self.nodeSelected == null)
                {
                    NodeManager.self.nodeSelected = nodes[0];
                }
                else if(NodeManager.self.nodeSelected != nodes[0])
                {
                    NodeManager.self.nodeSelected.AddConnectionRecursive(nodes[0]);
                    NodeManager.self.nodeSelected = nodes[0];
                }
            }
        }
        
    }

    private void OnDestroy()
    {
        if (NodeManager.self != null)
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null)
                    if (nodes[i].GetType() == NodeManager.self.endPointNode)
                    {
                        ((EndPointNode)nodes[i]).endPoints.Remove(GetComponent<INodeEndPoint>());
                    }
            }
    }

    public void Break()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            NodeManager.self.DeleteNode(nodes[i]);
        }
    }
}
