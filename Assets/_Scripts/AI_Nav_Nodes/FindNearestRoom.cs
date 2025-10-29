using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class FindNearestRoom : ActionNode
{
    private bool validTargetFound;
    private bool wasInRoomAtStart;
    private Vector2Int targetStandingSpot;
    
    protected override void OnStart() {
        validTargetFound = false;
        wasInRoomAtStart = false;
        targetStandingSpot = Vector2Int.zero;
        
        context.agentNavigation.ResetTurnMovement();
        
        Vector2Int currentPos = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);
        MazeCell currentCell = context.agentNavigation.gameManager.centralMaze[currentPos.x, currentPos.y];

        if (currentCell.roomTile && !context.agentNavigation.IsRoomVisited(currentCell.roomType))
        {
            if (IsStandingSpot(currentPos, currentCell.roomType))
            {
                Debug.Log($"{context.gameObject.name}: Already at standing spot {currentPos} in unvisited {currentCell.roomType}");
                context.agentNavigation.MarkRoomAsVisited(currentCell.roomType);
                context.agentNavigation.EnterRoom(currentCell.roomType);
                wasInRoomAtStart = true;
                return;
            }
            else
            {
                Debug.Log($"{context.gameObject.name}: In {currentCell.roomType} but not at a standing spot - need to move");
            }
        }
        
        if (PathCoordinator.Instance != null)
        {
            var plannedPath = PathCoordinator.Instance.GetPlannedPath(context.agentNavigation);
            
            if (plannedPath.Count > 0)
            {
                targetStandingSpot = plannedPath[^1].position;
                
                Debug.Log($"{context.gameObject.name}: Target standing spot is {targetStandingSpot}");
                
                List<MazeCell> cellPath = new List<MazeCell>();
                foreach (var (position, timeStep) in plannedPath)
                {
                    MazeCell cell = context.agentNavigation.gameManager.centralMaze[position.x, position.y];
                    cellPath.Add(cell);
                }
                
                context.agentNavigation.path = cellPath;
                
                if (cellPath.Count > 0)
                {
                    Vector3 firstCellWorldPos = new Vector3(
                        cellPath[0].position.x * context.agentNavigation.gameManager.CellSize,
                        context.agentNavigation.mazeHeight,
                        cellPath[0].position.y * context.agentNavigation.gameManager.CellSize
                    );
                    
                    context.agentNavigation.ReserveNextCell();
                    context.agentNavigation.destination = firstCellWorldPos;
                    context.agentNavigation.StartMovementWithPath();
                    
                    validTargetFound = true;
                }
            }
            else
            {
                Debug.Log($"{context.gameObject.name}: No planned path available");
                wasInRoomAtStart = true;
            }
        }
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (wasInRoomAtStart)
        {
            return State.Success;
        }
        
        if (!validTargetFound)
        {
            return State.Failure;
        }

        if (context.agentNavigation.HasMovedMaxTiles())
        {
            return State.Success;
        }

        if (context.agentNavigation.HasReachedDestination())
        {
            return State.Success;
        }

        return State.Running;
    }
    
    private bool IsStandingSpot(Vector2Int position, string roomType)
    {
        List<Room> allRooms = GameManager.Instance.GetRooms();
        
        foreach (Room room in allRooms)
        {
            if (room.roomType == roomType)
            {
                if (room.standingSpots != null && room.standingSpots.Contains(position))
                {
                    return true;
                }
                break;
            }
        }
        
        return false;
    }
}