using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class ExploreUnvisitedRoom : ActionNode
{
    public int explorationRadius = 10;
    
    protected override void OnStart() {
        // Decide whether to move into an unexplored room or explore randomly
        Vector2Int nextMove = context.agentNavigation.ExploreMaze();
        context.agentNavigation.SetDestination(nextMove);
    }

    protected override void OnStop() {}

    protected override State OnUpdate() {
        if (context.agentNavigation.HasReachedDestination()) {
            Vector2Int gridPosition = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);
            context.agentNavigation.MarkRoomAsVisited(gridPosition);  // Mark the entire room
            return State.Success;
        }
        return State.Running;
    }
}
