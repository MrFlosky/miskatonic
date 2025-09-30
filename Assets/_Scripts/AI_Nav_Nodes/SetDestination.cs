using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class SetDestination : ActionNode
{
    private bool destinationAttempted = false;
    
    protected override void OnStart()
    {
        destinationAttempted = false;
        
        // Only set destination if one isn't already set and agent isn't moving
        if (!context.agentNavigation.IsDestinationSet() && !context.agentNavigation.IsMoving())
        {
            Vector2Int randomPosition = context.agentNavigation.GetRandomWalkableTile();
            Vector2Int currentPos = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);
            
            // Only set destination if it's different from current position
            if (randomPosition != currentPos)
            {
                context.agentNavigation.SetDestination(randomPosition);
                destinationAttempted = true;
            }
            else
            {
                Debug.LogWarning($"Agent {context.gameObject.name}: Could not find different walkable tile");
                destinationAttempted = false;
            }
        }
    }

    protected override void OnStop() {
        // Clean up if needed
    }

    protected override State OnUpdate() {
        if (!destinationAttempted)
        {
            return State.Failure; // Could not set a valid destination
        }
        
        return State.Success;
    }
}
