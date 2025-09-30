using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class UnsetRandomPosition : ActionNode
{
    protected override void OnStart()
    {
        // Clear the destination flag regardless of whether we reached it
        // This allows the agent to set a new destination next time
        context.agentNavigation.ClearRandomPosition();
    }

    protected override void OnStop() {
        // Clean up if needed
    }

    protected override State OnUpdate() {
        return State.Success;
    }
}
