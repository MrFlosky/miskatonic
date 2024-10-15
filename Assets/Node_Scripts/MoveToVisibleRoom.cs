using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class MoveToVisibleRoom : ActionNode
{
    private Vector2Int roomPosition; // Store the found room position
    private Vector2Int targetPosition;
    
    protected override void OnStart() {
        // Find a visible unvisited room
        roomPosition = context.agentNavigation.CheckForRoom();

        if (roomPosition != context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position))
        {
            context.agentNavigation.SetRoomDestination(roomPosition);
        }
    }

    protected override void OnStop() {}

    protected override State OnUpdate()
    {
        // If no room is found, return failure (i.e., move to random movement)
        return roomPosition == context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position) ? State.Failure : State.Success;
    }

}
