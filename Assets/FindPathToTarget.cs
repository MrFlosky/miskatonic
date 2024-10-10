using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class FindPathToTarget : ActionNode
{
    public Vector2Int targetPosition;

    protected override void OnStart() {
        // Call your SetDestination method from AgentNavigation to find the path
        context.agentNavigation.SetDestination(targetPosition);
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.agentNavigation.HasReachedDestination()) {
            return State.Success;
        } else {
            return State.Failure;
        }
    }
}
