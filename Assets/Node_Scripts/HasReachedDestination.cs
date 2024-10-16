using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class HasReachedDestination : DecoratorNode
{
    protected override void OnStart() {
        // No setup needed at the start of this decorator
    }

    protected override void OnStop() {
        // No specific logic needed when this decorator stops
    }

    protected override State OnUpdate() {
        // Check if the agent has reached the destination
        if (context.agentNavigation.HasReachedDestination()) {
            // Now run the child node's logic
            return child.Update();
        } else {
            // If the agent hasn't reached its destination
            return State.Success;
        }
    }
}
