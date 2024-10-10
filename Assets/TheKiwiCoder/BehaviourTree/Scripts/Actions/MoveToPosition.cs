using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class MoveToPosition : ActionNode
{
    private float reachThreshold = 0.1f;  // Threshold to determine when the destination is reached

    protected override void OnStart() {
        // Set the agent's destination from the blackboard
        context.agentNavigation.SetDestination(blackboard.moveToPosition);
    }

    protected override void OnStop() {
        // Clean up logic if needed (optional)
    }

    protected override State OnUpdate() {
        // Get the agent's current position and destination
        Vector3 currentPosition = context.agentNavigation.transform.position;
        Vector3 destination = context.agentNavigation.destination;

        // Check if the agent has reached the destination using a proximity threshold
        if (Vector3.Distance(currentPosition, destination) < reachThreshold) {
            return State.Success;  // Reached the destination
        } else {
            return State.Running;  // Still moving towards the destination
        }
    }
}