using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class NodeManager : MonoBehaviour
{
    public void RunSimulation()
    {
        // Create the nodes
        SourceNode source1 = new SourceNode
        {
            TargetOutputPower = 100,
            OutputPowerDuration = 10 // Duration of power supply in seconds
        };

        SourceNode source2 = new SourceNode
        {
            TargetOutputPower = 50,
            OutputPowerDuration = 5 // Another source with different duration
        };

        Node intermediaryNode = new Node();
        EndPointNode endPoint = new EndPointNode();

        // Connect the nodes
        source1.AddConnection(intermediaryNode);
        source2.AddConnection(intermediaryNode);
        intermediaryNode.AddConnection(endPoint);

        // Simulate the system over time (for example, 1-second steps)
        float simulationTime = 0;
        float deltaTime = 1; // Time step of 1 second

        // Create a set to track updated nodes
        HashSet<Node> updatedNodes = new HashSet<Node>();

        // Update the power distribution for each source
        source1.UpdatePower(updatedNodes);
        source2.UpdatePower(updatedNodes);

        // Increase the simulation time
        simulationTime += deltaTime;
    }
}
