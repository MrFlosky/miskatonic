using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class SetDestination : ActionNode
{
    protected override void OnStart()
    {
        // Check if the AI has a random destination set
        if (context.agentNavigation.IsDestinationSet()) return;
        
        // If not, set a random destination
        Vector2Int randomPosition = context.agentNavigation.GetRandomWalkableTile();
        context.agentNavigation.SetDestination(randomPosition);
    }

    protected override void OnStop() {}

    protected override State OnUpdate() {
        // This node always succeeds after setting the position
        return State.Success;
    }
}
