using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class UnsetRandomPosition : ActionNode
{
    protected override void OnStart()
    {
        // Check if the agent has reached the destination
        if (!context.agentNavigation.HasReachedDestination()) return;
        // Clear the AI's random destination when the node is triggered
        context.agentNavigation.ClearRandomPosition();
    }

    protected override void OnStop() {
        // Optionally, any cleanup logic can go here if necessary
    }

    protected override State OnUpdate() {
        // This action always returns success after unsetting the position
        return State.Success;
    }
}
