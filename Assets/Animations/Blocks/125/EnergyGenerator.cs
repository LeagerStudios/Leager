using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyGenerator : MonoBehaviour
{
    public Sprite on;
    public Sprite off;
    public SpriteRenderer spriteRenderer;
    public NodeInstance nodeInstance;

    public bool shock = false;

    void Start()
    {
        nodeInstance = GetComponent<NodeInstance>();
    }

    // Update is called once per frame
    void Update()
    {
        SourceNode node = (SourceNode)nodeInstance.nodes[0];

        if (shock)
        {
            shock = false;

            node.TargetOutputPower = 1f;
            node.OutputPowerDuration += 5f;
        }

        spriteRenderer.sprite = node.OutputPower > 0f ? on : off;
    }
}
