using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class MoveToPosition : ActionNode
{
    private int initialPathLength;
    private int stepsTaken;
    
    protected override void OnStart()
    {
        initialPathLength = context.agentNavigation.path.Count;
        stepsTaken = 0;
    }

    protected override void OnStop() {
        // Optionally pause movement when this node stops
        // context.agentNavigation.PauseMovement();
    }

    protected override State OnUpdate() {
        // Calculate how many steps we've taken
        int currentPathLength = context.agentNavigation.path.Count;
        stepsTaken = initialPathLength - currentPathLength;
        
        // If we've taken the desired number of steps, pause and succeed
        if (stepsTaken >= context.agentNavigation.tilesPerTurn)
        {
            //context.agentNavigation.PauseMovement();
            return State.Success;
        }
        
        // If we've reached our final destination before taking all steps
        if (context.agentNavigation.HasReachedDestination())
        {
            return State.Success;
        }
        
        // If agent got stuck (not moving but hasn't reached destination)
        if (!context.agentNavigation.IsMoving() && context.agentNavigation.path.Count == 0 && !context.agentNavigation.HasReachedDestination())
        {
            Debug.LogWarning($"Agent {context.gameObject.name} appears stuck during MoveToPosition");
            return State.Failure;
        }
        
        return State.Running;
    }
}