using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class PathToLure : ActionNode
{
    private Vector2Int targetLurePosition;
    private bool validTargetFound;
    private bool wasAtLureAtStart;
    
    protected override void OnStart()
    {
        Debug.Log($"[BT] {context.gameObject.name}: PathToLure.OnStart() called");
        
        validTargetFound = false;
        wasAtLureAtStart = false;
        
        context.agentNavigation.ResetTurnMovement();
        
        if (LureManager.Instance == null || !LureManager.Instance.IsLureActive())
        {
            return;
        }
        
        Vector2Int? lurePos = LureManager.Instance.GetLurePosition();
        if (!lurePos.HasValue)
        {
            Debug.Log($"{context.gameObject.name}: Lure position is invalid");
            return;
        }
        
        targetLurePosition = lurePos.Value;
        Vector2Int currentPos = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);
        
        if (currentPos == targetLurePosition)
        {
            Debug.Log($"{context.gameObject.name}: Already at lure position");
            wasAtLureAtStart = true;
            return;
        }
        
        if (PathCoordinator.Instance != null)
        {
            var plannedPath = PathCoordinator.Instance.GetPlannedPath(context.agentNavigation);
            
            if (plannedPath.Count > 0)
            {
                List<MazeCell> cellPath = new List<MazeCell>();
                foreach (var (position, timeStep) in plannedPath)
                {
                    MazeCell cell = context.agentNavigation.gameManager.centralMaze[position.x, position.y];
                    cellPath.Add(cell);
                }
                
                context.agentNavigation.path = cellPath;
                
                if (cellPath.Count > 0)
                {
                    context.agentNavigation.ReserveNextCell();
                    context.agentNavigation.StartMovementWithPath();
                    
                    validTargetFound = true;
                    Debug.Log($"{context.gameObject.name}: Using pre-planned path to lure with {cellPath.Count} tiles");
                }
            }
        }
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (wasAtLureAtStart)
        {
            return State.Success;
        }
        
        if (!validTargetFound)
        {
            return State.Failure;
        }
        
        if (context.agentNavigation.HasMovedMaxTiles())
        {
            Debug.Log($"{context.gameObject.name}: Reached tile limit while going to lure");
            return State.Success;
        }
        
        Vector2Int currentPos = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);
        if (currentPos == targetLurePosition)
        {
            Debug.Log($"{context.gameObject.name}: Reached lure position!");
            context.agentNavigation.StopMovement();
            return State.Success;
        }
        
        if (context.agentNavigation.HasReachedDestination())
        {
            Debug.Log($"{context.gameObject.name}: Reached closest position to lure");
            return State.Success;
        }
        
        return State.Running;
    }
}