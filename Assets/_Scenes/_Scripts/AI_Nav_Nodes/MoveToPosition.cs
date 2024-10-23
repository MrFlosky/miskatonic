using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class MoveToPosition : ActionNode
{
    private int steps;
    protected override void OnStart()
    {
        steps = context.agentNavigation.path.Count;
    }

    protected override void OnStop() {}

    protected override State OnUpdate() {
        if (steps - 10 == context.agentNavigation.path.Count)
        {
            context.agentNavigation.PauseMovement();
            return State.Success;
        }
        
        if (context.agentNavigation.HasReachedDestination()) {
            // Move towards the random position
            
            return State.Success;
        }
        return State.Running;
    }
}