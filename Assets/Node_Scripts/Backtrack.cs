using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class Backtrack : ActionNode
{
    protected override void OnStart() {
        // Use the backtracking logic
        context.agentNavigation.Backtrack();
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
