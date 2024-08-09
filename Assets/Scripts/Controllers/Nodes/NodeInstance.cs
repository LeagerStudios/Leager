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
    Node[] nodes;

    // Start is called before the first frame update
    void Start()
    {
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

            NodeManager.self.RegisterNode(node);

            if (node.GetType() == NodeManager.self.endPointNode)
            {
                ((EndPointNode)node).endPoints.Add(GetComponent<INodeEndPoint>());
            }
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
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
            if (nodes[i].GetType() == NodeManager.self.endPointNode)
            {
                NodeManager.self.DeleteNode(nodes[i]);
            }
        }
    }
}
