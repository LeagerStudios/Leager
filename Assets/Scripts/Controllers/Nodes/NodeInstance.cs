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
            node.index = new Vector3Int((int)ManagingFunctions.ClampX(transform.position.x), (int)transform.position.y, i);
            node = NodeManager.self.RegisterNode(node);

            if (node.GetType() == NodeManager.self.endPointNode)
            {
                ((EndPointNode)node).endPoints.Add(GetComponent<INodeEndPoint>());
            }

            nodes[i] = node;
        }
        GameManager.OnWorldRounding += RefreshPositions;
    }

    private void OnDestroy()
    {
        GameManager.OnWorldRounding -= RefreshPositions;
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

    public void RefreshPositions()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].position = nodeObjects[i].transform.position;
        }
    }
}
