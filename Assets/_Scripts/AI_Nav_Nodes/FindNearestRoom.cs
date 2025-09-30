using System.Collections;
 using System.Collections.Generic;
 using System.Linq;
 using UnityEngine;
 using TheKiwiCoder;
 
public class FindNearestRoom : ActionNode
{
    private Vector2Int targetRoomPosition;
    private bool validTargetFound = false;
    private bool wasInRoomAtStart = false;
    
    protected override void OnStart() {
        validTargetFound = false;
        wasInRoomAtStart = false;
        
        // Reset tile counter at start of turn
        context.agentNavigation.ResetTurnMovement();
        
        Vector2Int currentPos = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);
        MazeCell currentCell = context.agentNavigation.gameManager.centralMaze[currentPos.x, currentPos.y];
        
        // Check if we're already standing in an unvisited room (could happen from room swap)
        if (currentCell.roomTile && !context.agentNavigation.IsRoomVisited(currentCell.roomType))
        {
            Debug.Log($"{context.gameObject.name}: Already standing in unvisited {currentCell.roomType} - marking as visited");
            context.agentNavigation.MarkRoomAsVisited(currentCell.roomType);
            context.agentNavigation.EnterRoom(currentCell.roomType);
            wasInRoomAtStart = true;
            
            // Turn ends immediately when discovering a room
            return;
        }
        
        // Check if we have a stored path from previous turn (paused mid-journey)
        if (context.agentNavigation.storedPath.Count > 0)
        {
            Debug.Log($"{context.gameObject.name}: Resuming stored path with {context.agentNavigation.storedPath.Count} tiles remaining");
            context.agentNavigation.RestorePath();
            validTargetFound = true;
            return;
        }
        
        // Find nearest unvisited room and create full path
        targetRoomPosition = FindNearestUnvisitedRoom();
        
        if (targetRoomPosition != Vector2Int.zero)
        {
            context.agentNavigation.SetRoomDestination(targetRoomPosition);
            validTargetFound = true;
        }
    }

    protected override void OnStop() {
        // Clean up if needed
    }

    protected override State OnUpdate() {
        if (wasInRoomAtStart)
        {
            // Discovered a room at start of turn - end turn immediately
            return State.Success;
        }
        
        if (!validTargetFound)
        {
            // No rooms found
            return State.Failure;
        }

        // Check if agent has moved max tiles this turn (path is paused)
        if (context.agentNavigation.HasMovedMaxTiles())
        {
            Debug.Log($"{context.gameObject.name}: Reached tile limit for this turn");
            return State.Success; // Turn ends, will resume next turn
        }

        // Check if we've reached the final destination
        if (context.agentNavigation.HasReachedDestination())
        {
            return State.Success;
        }

        // Still moving toward room
        return State.Running;
    }
    
    private Vector2Int FindNearestUnvisitedRoom()
    {
        Vector2Int agentPos = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);
        
        // Special case: If agent found their goal room AND ritual room has been discovered, go there
        bool foundGoalRoom = GameManager.Instance.CheckAgentGoal(context.agentNavigation);
        if (foundGoalRoom && context.agentNavigation.IsRoomVisited("Ritual Room"))
        {
            // Find Ritual Room location directly from centralMaze (not from Room objects)
            Vector2Int ritualPos = FindRoomInMaze("Ritual Room");
            if (ritualPos != Vector2Int.zero)
            {
                Debug.Log($"{context.gameObject.name}: Has key and Ritual Room discovered, heading there at {ritualPos}");
                return ritualPos;
            }
        }
        
        // Find nearest unvisited room by scanning centralMaze directly
        Vector2Int nearestRoom = Vector2Int.zero;
        float shortestDistance = float.MaxValue;
        HashSet<string> checkedRoomTypes = new HashSet<string>();
        
        MazeCell[,] maze = GameManager.Instance.centralMaze;
        int width = context.agentNavigation.mazeGenerator.mazeWidth;
        int height = context.agentNavigation.mazeGenerator.mazeHeight;
        
        // Scan entire maze for room tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell = maze[x, y];
                
                // If this is a room tile and we haven't visited this room type
                if (cell.roomTile && !context.agentNavigation.IsRoomVisited(cell.roomType))
                {
                    // Only check each room type once (find nearest tile of that room)
                    if (!checkedRoomTypes.Contains(cell.roomType))
                    {
                        checkedRoomTypes.Add(cell.roomType);
                        
                        Vector2Int roomPos = new Vector2Int(x, y);
                        float distance = Vector2Int.Distance(agentPos, roomPos);
                        
                        if (distance < shortestDistance)
                        {
                            shortestDistance = distance;
                            nearestRoom = roomPos;
                        }
                    }
                }
            }
        }
        
        if (nearestRoom != Vector2Int.zero)
        {
            MazeCell targetCell = maze[nearestRoom.x, nearestRoom.y];
            Debug.Log($"{context.gameObject.name}: Targeting nearest unvisited {targetCell.roomType} at {nearestRoom}");
        }
        else
        {
            Debug.Log($"{context.gameObject.name}: No unvisited rooms found in maze");
        }
        
        return nearestRoom;
    }
    
    private Vector2Int FindRoomInMaze(string roomType)
    {
        MazeCell[,] maze = GameManager.Instance.centralMaze;
        int width = context.agentNavigation.mazeGenerator.mazeWidth;
        int height = context.agentNavigation.mazeGenerator.mazeHeight;
        Vector2Int agentPos = context.agentNavigation.WorldToGridPosition(context.agentNavigation.transform.position);
        
        Vector2Int nearestTile = Vector2Int.zero;
        float shortestDistance = float.MaxValue;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell = maze[x, y];
                if (cell.roomTile && cell.roomType == roomType)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    float distance = Vector2Int.Distance(agentPos, pos);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestTile = pos;
                    }
                }
            }
        }
        
        return nearestTile;
    }
}