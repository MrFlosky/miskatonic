using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class RandomMovement : ActionNode
{
    protected override void OnStart() {
       // Vector2Int randomTarget = context.agentNavigation.GetRandomWalkableTile();
        //context.agentNavigation.SetDestination(randomTarget);
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.agentNavigation.HasReachedDestination()) {
            return State.Success;
        } else {
            return State.Running;
        }
    }
}
