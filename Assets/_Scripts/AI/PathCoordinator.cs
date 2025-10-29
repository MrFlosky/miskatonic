using System.Collections.Generic;
using UnityEngine;

public class PathCoordinator : MonoBehaviour
{
    public static PathCoordinator Instance;
    
    private Dictionary<AgentNavigation, List<(Vector2Int position, int timeStep)>> plannedPaths;
    private Dictionary<Vector2Int, List<(AgentNavigation agent, int timeStep)>> reservedCells;
    private List<AgentNavigation> pathPlanningOrder;
    
    private Dictionary<Vector2Int, AgentNavigation> destinationReservations;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        plannedPaths = new Dictionary<AgentNavigation, List<(Vector2Int, int)>>();
        reservedCells = new Dictionary<Vector2Int, List<(AgentNavigation, int)>>();
        pathPlanningOrder = new List<AgentNavigation>();
        destinationReservations = new Dictionary<Vector2Int, AgentNavigation>();
    }
    
    public void CoordinateAgentPaths(AgentNavigation[] agents)
    {
        ClearAllReservations();
        
        DeterminePlanningOrder(agents);
        
        foreach (var agent in pathPlanningOrder)
        {
            GenerateAndReservePath(agent);
        }
        
        Debug.Log($"PathCoordinator: Coordinated paths for {pathPlanningOrder.Count} agents");
    }
    
    private void DeterminePlanningOrder(AgentNavigation[] agents)
    {
        pathPlanningOrder.Clear();
        pathPlanningOrder.AddRange(agents);
        
        pathPlanningOrder.Sort((a, b) =>
        {
            float distA = GetDistanceToNearestUnvisitedRoom(a);
            float distB = GetDistanceToNearestUnvisitedRoom(b);
            return distA.CompareTo(distB);
        });
        
        for (int i = 0; i < pathPlanningOrder.Count; i++)
        {
            Debug.Log($"Path planning order {i + 1}: {pathPlanningOrder[i].name}");
        }
    }
    
    private float GetDistanceToNearestUnvisitedRoom(AgentNavigation agent)
    {
        Vector2Int agentPos = agent.WorldToGridPosition(agent.transform.position);
        float shortestDist = float.MaxValue;
        
        MazeCell[,] maze = GameManager.Instance.centralMaze;
        int width = agent.mazeGenerator.mazeWidth;
        int height = agent.mazeGenerator.mazeHeight;
        
        HashSet<string> checkedRooms = new HashSet<string>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell = maze[x, y];
                if (cell.roomTile && !agent.IsRoomVisited(cell.roomType))
                {
                    if (!checkedRooms.Contains(cell.roomType))
                    {
                        checkedRooms.Add(cell.roomType);
                        float dist = Vector2Int.Distance(agentPos, new Vector2Int(x, y));
                        if (dist < shortestDist)
                        {
                            shortestDist = dist;
                        }
                    }
                }
            }
        }
        
        return Mathf.Approximately(shortestDist, float.MaxValue) ? 1000f : shortestDist;
    }
    
    private void GenerateAndReservePath(AgentNavigation agent)
    {
        Vector2Int currentPos = agent.WorldToGridPosition(agent.transform.position);
        Debug.Log($"{agent.name}: Current position {currentPos}");
        
        if (LureManager.Instance == null || !LureManager.Instance.IsLureActive())
        {
            MazeCell currentCell = agent.gameManager.centralMaze[currentPos.x, currentPos.y];
            
            if (currentCell.roomTile && !agent.IsRoomVisited(currentCell.roomType))
            {
                if (IsStandingSpot(currentPos, currentCell.roomType))
                {
                    Debug.Log($"{agent.name}: Already at standing spot in unvisited room, no path needed");
                    return;
                }
                else
                {
                    Debug.Log($"{agent.name}: In {currentCell.roomType} but not at standing spot - generating path to standing spot");
                }
            }
        }
        
        Vector2Int target = FindTargetForAgent(agent);
        
        if (target == Vector2Int.zero)
        {
            Debug.LogWarning($"{agent.name}: No valid target found");
            return;
        }
        
        Debug.Log($"{agent.name}: Target found at {target}");
        
        List<MazeCell> path = agent.FindPathWithAwareness(currentPos, target);
        
        if (path.Count > 0)
        {
            ReservePath(agent, path);
            Debug.Log($"{agent.name}: Generated and reserved path with {path.Count} tiles to {target}");
        }
        else
        {
            Debug.LogWarning($"{agent.name}: No valid path found to {target}");
        }
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
    
    private void ReservePath(AgentNavigation agent, List<MazeCell> path)
    {
        if (!plannedPaths.ContainsKey(agent))
        {
            plannedPaths[agent] = new List<(Vector2Int, int)>();
        }
        
        plannedPaths[agent].Clear();
        
        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int cellPos = path[i].position;
            int timeStep = i;
            
            plannedPaths[agent].Add((cellPos, timeStep));
            
            if (!reservedCells.ContainsKey(cellPos))
            {
                reservedCells[cellPos] = new List<(AgentNavigation, int)>();
            }
            reservedCells[cellPos].Add((agent, timeStep));
        }
        
        int actualStopIndex;
        if (agent.tilesPerTurn > 0 && path.Count > agent.tilesPerTurn)
        {
            actualStopIndex = agent.tilesPerTurn - 1;
        }
        else
        {
            actualStopIndex = path.Count - 1;
        }
        
        Vector2Int actualDestination = path[actualStopIndex].position;
        destinationReservations[actualDestination] = agent;
        
        Debug.Log($"{agent.name}: Reserved destination at {actualDestination} (stop index: {actualStopIndex}/{path.Count})");
    }
    
    public bool IsCellReservedAtTime(Vector2Int position, int timeStep, AgentNavigation requestingAgent)
    {
        if (!reservedCells.ContainsKey(position))
        {
            return false;
        }
        
        foreach (var (agent, reservedTime) in reservedCells[position])
        {
            if (agent != requestingAgent && reservedTime == timeStep)
            {
                return true;
            }
        }
        
        return false;
    }
    
    public bool IsDestinationReserved(Vector2Int position, AgentNavigation requestingAgent)
    {
        if (!destinationReservations.ContainsKey(position))
        {
            return false;
        }
        
        return destinationReservations[position] != requestingAgent;
    }
    
    public Dictionary<AgentNavigation, List<(Vector2Int position, int timeStep)>> GetAllPlannedPaths()
    {
        return new Dictionary<AgentNavigation, List<(Vector2Int, int)>>(plannedPaths);
    }
    
    public List<(Vector2Int position, int timeStep)> GetPlannedPath(AgentNavigation agent)
    {
        if (plannedPaths.ContainsKey(agent))
        {
            return new List<(Vector2Int, int)>(plannedPaths[agent]);
        }
        return new List<(Vector2Int, int)>();
    }
    
    public void ClearAllReservations()
    {
        plannedPaths.Clear();
        reservedCells.Clear();
        pathPlanningOrder.Clear();
        destinationReservations.Clear();
    }
    
    private Vector2Int FindNearestUnvisitedRoomForAgent(AgentNavigation agent)
    {
        Vector2Int agentPos = agent.WorldToGridPosition(agent.transform.position);
        
        // Priority 1: If agent has key and knows where Ritual Room is, go there
        bool foundGoalRoom = GameManager.Instance.CheckAgentGoal(agent);
        if (foundGoalRoom && agent.IsRoomVisited("Ritual Room"))
        {
            Vector2Int ritualPos = FindRoomInteriorTile(agent, "Ritual Room");
            if (ritualPos != Vector2Int.zero)
            {
                Debug.Log($"{agent.name}: Has key and Ritual Room discovered, targeting interior tile at {ritualPos}");
                return ritualPos;
            }
        }
        
        // Priority 2: Find nearest unvisited room (target interior tile)
        Vector2Int nearestRoom = Vector2Int.zero;
        float shortestDistance = float.MaxValue;
        HashSet<string> checkedRoomTypes = new HashSet<string>();
        
        MazeCell[,] maze = GameManager.Instance.centralMaze;
        int width = agent.mazeGenerator.mazeWidth;
        int height = agent.mazeGenerator.mazeHeight;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell = maze[x, y];
                
                if (cell.roomTile && !agent.IsRoomVisited(cell.roomType))
                {
                    if (!checkedRoomTypes.Contains(cell.roomType))
                    {
                        checkedRoomTypes.Add(cell.roomType);
                        
                        // Find interior tile of this room
                        Vector2Int interiorTile = FindRoomInteriorTile(agent, cell.roomType);
                        if (interiorTile != Vector2Int.zero)
                        {
                            float distance = Vector2Int.Distance(agentPos, interiorTile);
                            
                            if (distance < shortestDistance)
                            {
                                shortestDistance = distance;
                                nearestRoom = interiorTile;
                            }
                        }
                    }
                }
            }
        }
        
        // Priority 3: FAILSAFE
        if (nearestRoom == Vector2Int.zero)
        {
            Debug.Log($"{agent.name}: No unvisited rooms found - activating failsafe");
            
            if (!foundGoalRoom)
            {
                string goalRoomName = agent.GetGoalRoomName();
                Vector2Int goalRoomPos = FindRoomInteriorTile(agent, goalRoomName);
                
                if (goalRoomPos != Vector2Int.zero)
                {
                    Debug.Log($"{agent.name}: FAILSAFE - Targeting goal room interior at {goalRoomPos}");
                    return goalRoomPos;
                }
            }
            else
            {
                Vector2Int ritualRoomPos = FindRoomInteriorTile(agent, "Ritual Room");
                
                if (ritualRoomPos != Vector2Int.zero)
                {
                    Debug.Log($"{agent.name}: FAILSAFE - Targeting Ritual Room interior at {ritualRoomPos}");
                    return ritualRoomPos;
                }
            }
        }
        
        return nearestRoom;
    }
    
    private Vector2Int FindRoomInteriorTile(AgentNavigation agent, string roomType)
    {
        Room targetRoom = null;
        foreach (Room room in GameManager.Instance.GetRooms())
        {
            if (room.roomType == roomType)
            {
                targetRoom = room;
                break;
            }
        }

        if (targetRoom == null)
        {
            Debug.LogWarning($"{agent.name}: Could not find room '{roomType}'");
            return Vector2Int.zero;
        }
    
        if (targetRoom.standingSpots == null || targetRoom.standingSpots.Count == 0)
        {
            Debug.LogWarning($"{agent.name}: Room '{roomType}' has no standing spots defined!");
            return Vector2Int.zero;
        }

        Vector2Int agentPos = agent.WorldToGridPosition(agent.transform.position);
        List<(Vector2Int position, float distance)> sortedSpots = new();

        foreach (Vector2Int spot in targetRoom.standingSpots)
        {
            if (IsDestinationReserved(spot, agent))
            {
                Debug.Log($"{agent.name}: Standing spot {spot} in '{roomType}' is already reserved");
                continue;
            }
    
            float distance = Vector2Int.Distance(agentPos, spot);
            sortedSpots.Add((spot, distance));
        }

        if (sortedSpots.Count == 0)
        {
            Debug.LogWarning($"{agent.name}: All standing spots in '{roomType}' are reserved");
            return Vector2Int.zero;
        }
    
        sortedSpots.Sort((a, b) => a.distance.CompareTo(b.distance));

        Debug.Log($"{agent.name}: Targeting standing spot {sortedSpots[0].position} in '{roomType}' (distance: {sortedSpots[0].distance:F2})");

        return sortedSpots[0].position;
    }
    
    private Vector2Int FindTargetForAgent(AgentNavigation agent)
    {
        if (LureManager.Instance != null && LureManager.Instance.IsLureActive())
        {
            Vector2Int? lurePos = LureManager.Instance.GetLurePosition();
            if (lurePos.HasValue)
            {
                Debug.Log($"{agent.name}: Targeting active lure at {lurePos.Value}");
                return lurePos.Value;
            }
        }
        
        return FindNearestUnvisitedRoomForAgent(agent);
    }
}