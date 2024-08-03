using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSource : Node
{
    public float power = 0f;
    public float targetPower = 1f;

    void Start()
    {
        
    }

    void Update()
    {

    }

    private void LateUpdate()
    {
        power = targetPower;
    }
}
