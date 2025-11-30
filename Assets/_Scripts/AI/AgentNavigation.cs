using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class AgentNavigation : MonoBehaviour
{
    public MazeGenerator mazeGenerator;
    public GameManager gameManager;
    
    public float speed = 3.5f;
    public float mazeHeight = 0.5f;

    public List<MazeCell> path = new();

    private bool isMoving = false;
    public bool hasDestination = false;
    public bool turnFinished = false;

    private Stack<Vector2Int> pathHistory = new();
    public List<String> visitedRooms = new();
    private Dictionary<Vector2Int, int> visitCount = new();
    private PlayerVision playerVision;

    public Vector3 destination;

    [SerializeField] private RoomName FearRoom;
    [SerializeField]public List<RoomName> BonusFearRooms;

    public void SetBonusFearRoom()
    {
        foreach(var RM in BonusFearRooms)
        {
            roomFearEffects[GetRoomName(RM)] = RoomIncreaseFear;
            
        }


    }

    [SerializeField] private RoomName GoalRoom;

    private string currentRoom;
    private Dictionary<string, Action> roomFearEffects;

    [SerializeField] public Fear agentFear;
    
    [Header("Movement Per Turn")]
    [Tooltip("Number of tiles the agent can move per turn. Set to 0 for unlimited movement.")]
    public int tilesPerTurn = 5;
    
    private int tilesMoved; // Track tiles moved this turn
    
    public bool HasMovedMaxTiles()
    {
        return tilesPerTurn > 0 && tilesMoved >= tilesPerTurn;
    }
    
    public void ResetTurnMovement()
    {
        tilesMoved = 0;
    }
    
    private static HashSet<Vector2Int> occupiedCells = new();

    private void Start()
    {
        roomFearEffects = new Dictionary<string, System.Action>();
        roomFearEffects[GetRoomName(FearRoom)] = RoomIncreaseFear;
        roomFearEffects[GetRoomName(GoalRoom)] = DecreaseFear;
        BonusFearRooms.Clear();
        
    }

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager instance is null.");
        }

        // Get PlayerVision component
        playerVision = GetComponent<PlayerVision>();

    }
    
    private void Update()
    {
        FollowPath();

        if (visitedRooms.Count == 0) return;

        string roomInfo = string.Join(", ", visitedRooms);
//        Debug.Log(gameObject.name + "'s Visited Rooms: " + roomInfo);
    }

    public void SetDestination(Vector2Int target) {
        hasDestination = true;
        
        StopMovement();

        Vector2Int currentPosition = WorldToGridPosition(transform.position);
        pathHistory.Push(currentPosition);

        Vector2Int start = currentPosition;
        path = FindPath(start, target);

        if (path.Count > 0) {
            // Reserve the target cell
            ReserveNextCell();
            isMoving = true;
            destination = GetWorldPosition(path[0].position);
        } else {
            Debug.LogWarning($"{gameObject.name}: No valid path found to {target}");
            hasDestination = false;
        }
    }
    
    public void SetRoomDestination(Vector2Int target) {
        StopMovement();

        Vector2Int currentPosition = WorldToGridPosition(transform.position);
        pathHistory.Push(currentPosition);

        Vector2Int start = currentPosition;
        path = FindPath(start, target);

        if (path.Count > 0) {
            ReserveNextCell();
            isMoving = true;
            destination = GetWorldPosition(path[0].position);
        } else {
            Debug.LogWarning($"{gameObject.name}: No valid path found to room at {target}");
        }
    }
    
    private void FollowPath()
    {
        if (!isMoving || path.Count == 0) return;
        
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = destination;

        Vector3 direction = (targetPosition - currentPosition).normalized;
        transform.position += direction * (speed * Time.deltaTime);

        transform.position = new Vector3(transform.position.x, mazeHeight, transform.position.z);

        if (Vector3.Distance(currentPosition, targetPosition) < 0.1f)
        {
            // Free the cell we're leaving
            Vector2Int previousGrid = WorldToGridPosition(currentPosition);
            FreeCell(previousGrid);

            path.RemoveAt(0);
            tilesMoved++;

            Vector2Int gridPosition = WorldToGridPosition(transform.position);
            MazeCell currentCell = gameManager.centralMaze[gridPosition.x, gridPosition.y];

            // Track visit count for exploration
            if (!visitCount.ContainsKey(gridPosition))
                visitCount[gridPosition] = 0;
            visitCount[gridPosition]++;

            string roomType = currentCell.roomType;
            
            if (!IsRoomVisited(roomType) && currentCell.roomTile)
            {
                if (IsStandingSpot(gridPosition, roomType))
                {
                    Debug.Log($"{gameObject.name}: Reached designated standing spot at {gridPosition} in {roomType} - entering room");
                    MarkRoomAsVisited(roomType);
                    string noSpacesRoom = roomType.Replace(" ", "");    
                    gameObject.GetComponent<DialogueBank>().QueueSpecialDialog(noSpacesRoom, DialogueBank.DialogueType.Room);
                    EnterRoom(roomType);
                    StopMovement();
                    return;
                }
                else
                {
                    Debug.Log($"{gameObject.name}: Passing through {roomType} at {gridPosition} (not a standing spot)");
                }
            }
            
            if (currentCell.roomTile && currentRoom != currentCell.roomType)
            {
                currentRoom = currentCell.roomType;
            }
            else if (currentCell.roomType == "Hallway")
            {
                gameObject.GetComponent<DialogueBank>().DequeueSpecialDialog(DialogueBank.DialogueType.Room);
                currentRoom = null;
            }
            
            if (HasMovedMaxTiles())
            {
                Debug.Log($"{gameObject.name}: Moved {tilesMoved} tiles this turn - pausing");
                return;
            }

            if (path.Count > 0)
            {
                Vector3 nextPos = GetWorldPosition(path[0].position);
                Vector3 moveDir = (nextPos - transform.position).normalized;
                
                if (playerVision != null)
                {
                    if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.z))
                    {
                        playerVision.facing = moveDir.x > 0 ? Vision.Right : Vision.Left;
                    }
                    else
                    {
                        playerVision.facing = moveDir.z > 0 ? Vision.Up : Vision.Down;
                    }
                }

                if (playerVision != null)
                {
                    Debug.Log("playerVision updated.");
                    playerVision.UpdateVision(true);
                    playerVision.ApplyPsychosis(true);
                }
                
                MazeCell nextCell = path[0];
                if (!IsWalkable(currentCell, nextCell))
                {
                    Debug.LogError($"{gameObject.name}: Path blocked by walls during movement - stopping");
                    StopMovement();
                    return;
                }
                
                ReserveNextCell();
                destination = GetWorldPosition(path[0].position);
            }
            else
            {
                StopMovement();
            }
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
    
    public void ReserveNextCell()
    {
        if (path.Count > 0)
        {
            occupiedCells.Add(path[0].position);
        }
    }
    
    private void FreeCell(Vector2Int position)
    {
        occupiedCells.Remove(position);
    }
    
    private bool IsCellOccupiedByOther(Vector2Int position)
    {
        Vector2Int myPosition = WorldToGridPosition(transform.position);
        if (position == myPosition) return false;
        return occupiedCells.Contains(position);
    }

    public void StopMovement()
    {
        // Free any reserved cells
        if (path.Count > 0)
        {
            FreeCell(path[0].position);
        }
        
        path.Clear();
        isMoving = false;

        // null check or else this will cause stackoverflow
        if (playerVision != null)
        {
            //playerVision.totemCell.Clear();
            playerVision.UpdateVision(false);
            playerVision.ApplyPsychosis(false);
        }
       
        gameObject.GetComponent<DialogueBank>().alreadyTalked = false;
    }
    
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int gridX = Mathf.RoundToInt(worldPosition.x / gameManager.CellSize);
        int gridZ = Mathf.RoundToInt(worldPosition.z / gameManager.CellSize);
        return new Vector2Int(gridX, gridZ);
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * gameManager.CellSize, mazeHeight, gridPosition.y * gameManager.CellSize);
    }
    
    private List<MazeCell> FindPath(Vector2Int start, Vector2Int target)
    {
        // Validate bounds
        if (start.x < 0 || start.x >= mazeGenerator.mazeWidth || 
            start.y < 0 || start.y >= mazeGenerator.mazeHeight ||
            target.x < 0 || target.x >= mazeGenerator.mazeWidth || 
            target.y < 0 || target.y >= mazeGenerator.mazeHeight)
        {
            Debug.LogError($"{gameObject.name}: Path start {start} or target {target} out of bounds");
            return new List<MazeCell>();
        }

        List<MazeCell> openSet = new List<MazeCell>();
        HashSet<MazeCell> closedSet = new HashSet<MazeCell>();

        MazeCell startCell = gameManager.centralMaze[start.x, start.y];
        MazeCell targetCell = gameManager.centralMaze[target.x, target.y];

        openSet.Add(startCell);

        Dictionary<MazeCell, MazeCell> cameFrom = new Dictionary<MazeCell, MazeCell>();
        Dictionary<MazeCell, float> gScore = new Dictionary<MazeCell, float>();
        Dictionary<MazeCell, float> fScore = new Dictionary<MazeCell, float>();

        gScore[startCell] = 0;
        fScore[startCell] = Heuristic(startCell, targetCell);

        while (openSet.Count > 0)
        {
            MazeCell current = openSet[0];
            foreach (MazeCell cell in openSet)
            {
                if (fScore.ContainsKey(cell) && fScore[cell] < fScore[current])
                {
                    current = cell;
                }
            }

            if (current == targetCell)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (MazeCell neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;
                
                if (neighbor != targetCell && IsCellOccupiedByOther(neighbor.position))
                    continue;

                float tentativeGScore = gScore[current] + 1;

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor])
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, targetCell);
            }
        }

        return new List<MazeCell>();
    }

    private List<MazeCell> GetNeighbors(MazeCell cell)
    {
        List<MazeCell> neighbors = new List<MazeCell>();

        foreach (Directions direction in (Directions[])System.Enum.GetValues(typeof(Directions)))
        {
            Vector2Int neighborPosition = cell.position;
            switch (direction)
            {
                case Directions.Up:
                    neighborPosition.y++;
                    break;
                case Directions.Down:
                    neighborPosition.y--;
                    break;
                case Directions.Left:
                    neighborPosition.x--;
                    break;
                case Directions.Right:
                    neighborPosition.x++;
                    break;
            }

            if (neighborPosition.x >= 0 && neighborPosition.x < mazeGenerator.mazeWidth &&
                neighborPosition.y >= 0 && neighborPosition.y < mazeGenerator.mazeHeight)
            {
                MazeCell neighborCell = gameManager.centralMaze[neighborPosition.x, neighborPosition.y];
                if (IsWalkable(cell, neighborCell))
                {
                    neighbors.Add(neighborCell);
                }
            }
        }

        return neighbors;
    }
    
    private List<MazeCell> GetVisibleNeighbors(MazeCell cell)
    {
        List<MazeCell> neighbors = new List<MazeCell>();

        foreach (Directions direction in (Directions[])System.Enum.GetValues(typeof(Directions)))
        {
            Vector2Int neighborPosition = cell.position;
            switch (direction)
            {
                case Directions.Up:
                    neighborPosition.y++;
                    break;
                case Directions.Down:
                    neighborPosition.y--;
                    break;
                case Directions.Left:
                    neighborPosition.x--;
                    break;
                case Directions.Right:
                    neighborPosition.x++;
                    break;
            }

            if (neighborPosition.x >= 0 && neighborPosition.x < mazeGenerator.mazeWidth &&
                neighborPosition.y >= 0 && neighborPosition.y < mazeGenerator.mazeHeight)
            {
                MazeCell neighborCell = gameManager.centralMaze[neighborPosition.x, neighborPosition.y];
            
                if (HasLineOfSight(cell, neighborCell, direction)) {
                    neighbors.Add(neighborCell);
                }
            }
        }

        return neighbors;
    }
    
    private bool HasLineOfSight(MazeCell fromCell, MazeCell toCell, Directions direction)
    {
        switch (direction)
        {
            case Directions.Up:
                return !fromCell.topWall && !toCell.bottomWall;
            case Directions.Down:
                return !fromCell.bottomWall && !toCell.topWall;
            case Directions.Left:
                return !fromCell.leftWall && !toCell.rightWall;
            case Directions.Right:
                return !fromCell.rightWall && !toCell.leftWall;
            default:
                return false;
        }
    }

    public bool IsWalkable(MazeCell fromCell, MazeCell toCell)
    {
        if (toCell.x > fromCell.x && !fromCell.rightWall && !toCell.leftWall) return true;
        if (toCell.x < fromCell.x && !fromCell.leftWall && !toCell.rightWall) return true;
        if (toCell.y > fromCell.y && !fromCell.topWall && !toCell.bottomWall) return true;
        if (toCell.y < fromCell.y && !fromCell.bottomWall && !toCell.topWall) return true;

        return false;
    }

    private float Heuristic(MazeCell a, MazeCell b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<MazeCell> ReconstructPath(Dictionary<MazeCell, MazeCell> cameFrom, MazeCell current)
    {
        List<MazeCell> totalPath = new List<MazeCell> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
    
    public bool HasReachedDestination() {
        return !isMoving && path.Count == 0;
    }
    
    public Vector2Int GetRandomWalkableTile() {
        Vector2Int currentPosition = WorldToGridPosition(transform.position);

        Queue<(Vector2Int position, int steps)> frontier = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<(Vector2Int position, float score)> scoredTiles = new List<(Vector2Int, float)>();

        frontier.Enqueue((currentPosition, 0));
        visited.Add(currentPosition);

        System.Random random = new System.Random(gameObject.GetInstanceID());

        while (frontier.Count > 0) {
            var (currentPos, steps) = frontier.Dequeue();
            
            // Skip if occupied by another agent
            if (IsCellOccupiedByOther(currentPos))
                continue;
                
            List<MazeCell> neighbors = GetNeighbors(gameManager.centralMaze[currentPos.x, currentPos.y]);

            foreach (MazeCell neighbor in neighbors) {
                Vector2Int neighborPos = neighbor.position;

                if (!visited.Contains(neighborPos)) {
                    visited.Add(neighborPos);
                    frontier.Enqueue((neighborPos, steps + 1));

                    // Don't score occupied tiles
                    if (IsCellOccupiedByOther(neighborPos))
                        continue;

                    float distanceScore = steps;
                    float visitScore = visitCount.ContainsKey(neighborPos) ? visitCount[neighborPos] : 0;
                    float regionPenalty = GetRegionRoomDensity(neighborPos);

                    float randomFactor = (float)(random.NextDouble() * 0.5);
                    float score = distanceScore - visitScore - regionPenalty + randomFactor;

                    scoredTiles.Add((neighborPos, score));
                }
            }
        }

        if (scoredTiles.Count > 0) {
            scoredTiles.Sort((a, b) => b.score.CompareTo(a.score));
            int randomIndex = random.Next(Mathf.Min(3, scoredTiles.Count));
            return scoredTiles[randomIndex].position;
        }

        return currentPosition;
    }
    
    private float GetRegionRoomDensity(Vector2Int position) {
        int regionX = position.x / (mazeGenerator.mazeWidth / 3);
        int regionY = position.y / (mazeGenerator.mazeHeight / 3);
    
        int roomsInRegion = CountRoomsInRegion(regionX, regionY);

        return roomsInRegion * 0.5f;
    }

    private int CountRoomsInRegion(int regionX, int regionY) 
    {
        var allRooms = GameManager.Instance.GetRooms();

        int roomCount = 0;

        foreach (Room room in allRooms) 
        {
            if (visitedRooms.Contains(room.roomType) && IsRoomInRegion(room, regionX, regionY)) 
            {
                roomCount++;
            }
        }
        return roomCount;
    }

    private bool IsRoomInRegion(Room room, int regionX, int regionY) {
        foreach (Vector2Int tile in room.tilesInRoom) {
            int tileRegionX = tile.x / (mazeGenerator.mazeWidth / 3);
            int tileRegionY = tile.y / (mazeGenerator.mazeHeight / 3);
            if (tileRegionX == regionX && tileRegionY == regionY) {
                return true;
            }
        }
        return false;
    }
    
    public Vector2Int CheckForRoom()
    {
        Vector2Int currentPosition = WorldToGridPosition(transform.position);
        MazeCell currentCell = gameManager.centralMaze[currentPosition.x, currentPosition.y];

        List<MazeCell> neighbors = GetVisibleNeighbors(currentCell);

        // Check direct neighbors first
        foreach (MazeCell neighbor in neighbors)
        {
            if (neighbor.roomTile && neighbor.room != null && !IsRoomVisited(neighbor.room.roomType))
            {
                return neighbor.position;
            }
        }

        // Check second layer
        foreach (MazeCell neighbor in neighbors)
        {
            List<MazeCell> secondLayerNeighbors = GetVisibleNeighbors(neighbor);
            foreach (MazeCell secondNeighbor in secondLayerNeighbors)
            {
                if (secondNeighbor.roomTile && secondNeighbor.room != null && !IsRoomVisited(secondNeighbor.room.roomType))
                {
                    return secondNeighbor.position;
                }
            }
        }

        return currentPosition;
    }
    
    public void MarkRoomAsVisited(string roomType)
    {
        if (!visitedRooms.Contains(roomType))
        {
            visitedRooms.Add(roomType);
            path.Clear();



        }
    }

    public bool IsRoomVisited(string roomType)
    {
        return visitedRooms.Contains(roomType);
    }
    
    public bool IsStandingInUnvisitedRoom()
    {
        Vector2Int currentPos = WorldToGridPosition(transform.position);
        MazeCell currentCell = gameManager.centralMaze[currentPos.x, currentPos.y];
        
        // If standing in a room tile and haven't visited this room type
        if (currentCell.roomTile && !IsRoomVisited(currentCell.roomType))
        {
            return true;
        }
        
        return false;
    }
    
    public void OnRoomSwapped()
    {
        Vector2Int currentPos = WorldToGridPosition(transform.position);
        MazeCell currentCell = gameManager.centralMaze[currentPos.x, currentPos.y];
    
        Debug.Log($"{gameObject.name}: Room swap detected at position {currentPos}, cell type is now {currentCell.roomType}");
    
        StopMovement();
    
        if (currentCell.roomTile)
        {
            currentRoom = currentCell.roomType;
        
            if (!IsRoomVisited(currentCell.roomType))
            {
                Debug.Log($"{gameObject.name}: Standing in unvisited {currentCell.roomType} after swap - entering it");
                MarkRoomAsVisited(currentCell.roomType);
                EnterRoom(currentCell.roomType);
            }
        
            Debug.Log($"{gameObject.name}: Now in {currentCell.roomType} after swap");
        }
        else
        {
            currentRoom = null;
        }
    }
    
    public void ClearRandomPosition() {
        hasDestination = false;
    }

    public bool IsDestinationSet() {
        return hasDestination;
    }

    public void EnterRoom(string roomType)
    {
        currentRoom = roomType;
        Debug.Log($"{gameObject.name}: EnterRoom called for {roomType}");
    
        if (roomFearEffects.ContainsKey(roomType))
        {
            Debug.Log($"{gameObject.name}: Found fear effect for {roomType}, executing...");
            roomFearEffects[roomType]?.Invoke();
        }
        else
        {
            Debug.Log($"{gameObject.name}: No fear effect registered for {roomType}");
        }
    }

    public void RoomIncreaseFear()
    {
        Debug.LogError(agentFear.currentFear);
        
        agentFear.AddFear(5);
        Debug.LogError(agentFear.currentFear);
    }
    
    public void AddBonusFear(float bonusFear)
    {
        //temporary pending fear being made into float
       int addedFear = Mathf.RoundToInt(bonusFear);

        agentFear.AddFear(addedFear);
    }

    private void DecreaseFear()
    {
        Debug.Log($"{gameObject.name}: DecreaseFear called - this is the GOAL ROOM!");
        bool hadGoalBefore = gameManager.CheckAgentGoal(this);
        Debug.Log($"{gameObject.name}: Had goal before: {hadGoalBefore}");
    
        gameManager.SetGoalBool(this, true);
    
        bool hasGoalAfter = gameManager.CheckAgentGoal(this);
        Debug.Log($"{gameObject.name}: Has goal after: {hasGoalAfter}");
    
        agentFear.ReduceFear(5);
    }

    public enum RoomName
    {
        Library,
        Dungeon,
        DiningRoom,
        Cemetery,
        RitualRoom,
        Bedroom,
        None
    }
    
    private string GetRoomName(RoomName roomName)
    {
        return roomName switch
        {
            RoomName.Library => "Library",
            RoomName.Dungeon => "Dungeon",
            RoomName.DiningRoom => "DiningRoom",
            RoomName.Cemetery => "Cemetery",
            RoomName.RitualRoom => "Ritual Room",
            RoomName.Bedroom => "Bedroom",
            _ => roomName.ToString()
        };
    }
    
    private void OnDestroy()
    {
        Vector2Int currentPos = WorldToGridPosition(transform.position);
        FreeCell(currentPos);
        if (path.Count > 0)
        {
            FreeCell(path[0].position);
        }
    }
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    public List<MazeCell> FindPathWithAwareness(Vector2Int start, Vector2Int target)
    {
        if (start.x < 0 || start.x >= mazeGenerator.mazeWidth || 
            start.y < 0 || start.y >= mazeGenerator.mazeHeight ||
            target.x < 0 || target.x >= mazeGenerator.mazeWidth || 
            target.y < 0 || target.y >= mazeGenerator.mazeHeight)
        {
            Debug.LogError($"{gameObject.name}: Path start {start} or target {target} out of bounds");
            return new List<MazeCell>();
        }

        List<MazeCell> openSet = new List<MazeCell>();
        HashSet<MazeCell> closedSet = new HashSet<MazeCell>();

        MazeCell startCell = gameManager.centralMaze[start.x, start.y];
        MazeCell targetCell = gameManager.centralMaze[target.x, target.y];

        openSet.Add(startCell);

        Dictionary<MazeCell, MazeCell> cameFrom = new Dictionary<MazeCell, MazeCell>();
        Dictionary<MazeCell, float> gScore = new Dictionary<MazeCell, float>();
        Dictionary<MazeCell, float> fScore = new Dictionary<MazeCell, float>();
        Dictionary<MazeCell, int> timeAtCell = new Dictionary<MazeCell, int>(); // Track time steps

        gScore[startCell] = 0;
        fScore[startCell] = Heuristic(startCell, targetCell);
        timeAtCell[startCell] = 0;

        while (openSet.Count > 0)
        {
            MazeCell current = openSet[0];
            foreach (MazeCell cell in openSet)
            {
                if (fScore.ContainsKey(cell) && fScore[cell] < fScore[current])
                {
                    current = cell;
                }
            }

            if (current == targetCell)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            int currentTime = timeAtCell[current];

            foreach (MazeCell neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;
    
                if (!IsWalkable(current, neighbor)) continue;
    
                int neighborTime = currentTime + 1;
                
                if (neighbor != targetCell)
                {
                    if (PathCoordinator.Instance != null && 
                        PathCoordinator.Instance.IsCellReservedAtTime(neighbor.position, neighborTime, this))
                    {
                        continue;
                    }
                }
    
                if (neighbor != targetCell && IsCellOccupiedByOther(neighbor.position))
                {
                    continue;
                }

                float tentativeGScore = gScore[current] + 1;

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor])
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, targetCell);
                timeAtCell[neighbor] = neighborTime;
            }
        }

        return new List<MazeCell>();
    }
    
    public void StartMovementWithPath()
    {
        if (path.Count > 0)
        {
            isMoving = true;
            destination = GetWorldPosition(path[0].position);
            Debug.Log($"{gameObject.name}: Started movement with {path.Count} tiles in path");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Cannot start movement - path is empty");
        }
    }
    
    public string GetGoalRoomName()
    {
        return GetRoomName(GoalRoom);
    }

    public Vector2Int GetActualStopPosition()
    {
        if (path.Count == 0)
        {
            return WorldToGridPosition(transform.position);
        }

        int stopIndex;
        if (tilesPerTurn > 0 && path.Count > tilesPerTurn)
        {
            stopIndex = tilesPerTurn - 1;
        }
        else
        {
            stopIndex = path.Count - 1;
        }

        return path[stopIndex].position;
    }
    
    /// <summary>
    /// Set the number of tiles this AI is allowed to move per turn.
    /// </summary>
    public void SetTilesPerTurn(int tiles)
    {
        tilesPerTurn = tiles;
    }

    public void CheckForClear()
    {
        bool allNone= true;
        foreach(var RoomN in BonusFearRooms)
        {
            if(RoomN!= RoomName.None)
            {
                allNone = false;
                break;
            }
        }

        if (allNone)
        {
           BonusFearRooms.Clear(); 
        }
    }
}