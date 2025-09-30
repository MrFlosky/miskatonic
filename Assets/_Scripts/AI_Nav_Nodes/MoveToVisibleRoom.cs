using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class MoveToVisibleRoom : ActionNode
{
    private Vector2Int roomPosition;
    private bool destinationSet;
    
    protected override void OnStart() {
        roomPosition = context.agentNavigation.CheckForRoom();
        Vector2Int currentPos = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);

        // Only set destination if we found a different room
        if (roomPosition != currentPos)
        {
            context.agentNavigation.SetRoomDestination(roomPosition);
            destinationSet = true;
        }
        else
        {
            destinationSet = false;
        }
    }

    protected override void OnStop() {
        // Clean up if needed
    }

    protected override State OnUpdate()
    {
        if (!destinationSet)
        {
            return State.Failure; // No room found, proceed to random movement
        }

        // Check if we've reached our destination or are no longer moving
        if (context.agentNavigation.HasReachedDestination())
        {
            return State.Success; // Successfully reached the room
        }

        return State.Running; // Still moving toward room
    }
}
