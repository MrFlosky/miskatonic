using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class AgentNavigation : MonoBehaviour
{
    public MazeGenerator mazeGenerator;
    public GameManager gameManager;
    
    public float speed = 3.5f;
    public float mazeHeight = 0.5f;

    public List<MazeCell> path = new List<MazeCell>();
    public List<MazeCell> storedPath = new List<MazeCell>();

    private bool isMoving = false;
    public bool hasDestination = false;
    public bool turnFinished = false;

    private Stack<Vector2Int> pathHistory = new Stack<Vector2Int>();
    private List<String> visitedRooms = new List<String>();
    private Dictionary<Vector2Int, int> visitCount = new Dictionary<Vector2Int, int>();

    public Vector3 destination;

    [SerializeField] private RoomName FearRoom;
    [SerializeField] private RoomName GoalRoom;

    private string currentRoom;
    private Dictionary<string, System.Action> roomFearEffects;

    [SerializeField] private Fear agentFear;
    
    [Header("Movement Per Turn")]
    [Tooltip("Number of tiles the agent can move per turn. Set to 0 for unlimited movement.")]
    public int tilesPerTurn = 5;
    
    private int tilesMoved = 0; // Track tiles moved this turn
    
    public bool HasMovedMaxTiles()
    {
        return tilesPerTurn > 0 && tilesMoved >= tilesPerTurn;
    }
    
    public void ResetTurnMovement()
    {
        tilesMoved = 0;
    }
    
    private static HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    private void Start()
    {
        roomFearEffects = new Dictionary<string, System.Action>();
        roomFearEffects[GetRoomName(FearRoom)] = IncreaseFear;
        roomFearEffects[GetRoomName(GoalRoom)] = DecreaseFear;
    }

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager instance is null.");
        }
    }
    
    private void Update()
    {
        FollowPath();

        if (visitedRooms.Count == 0) return;

        string roomInfo = string.Join(", ", visitedRooms);
        Debug.Log(gameObject.name + "'s Visited Rooms: " + roomInfo);
    }

    public void SetDestination(Vector2Int target) {
        hasDestination = true;

        if (storedPath.Count > 0) {
            RestorePath();
            return;
        }

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
            tilesMoved++; // Increment tiles moved counter

            Vector2Int gridPosition = WorldToGridPosition(transform.position);
            MazeCell currentCell = gameManager.centralMaze[gridPosition.x, gridPosition.y];

            // Track visit count for exploration
            if (!visitCount.ContainsKey(gridPosition))
                visitCount[gridPosition] = 0;
            visitCount[gridPosition]++;

            string roomType = currentCell.roomType;

            // Check if we've reached a room tile that we haven't visited
            if (!IsRoomVisited(roomType) && currentCell.roomTile)
            {
                MarkRoomAsVisited(roomType);
                EnterRoom(roomType);
                StopMovement();
                return;
            }
            
            // Update current room tracking
            if (currentCell.roomTile && currentRoom != currentCell.roomType)
            {
                EnterRoom(roomType);
            }
            else if (currentCell.roomType == "Hallway")
                currentRoom = null;

            // Check if we've moved the maximum tiles for this turn
            if (HasMovedMaxTiles())
            {
                Debug.Log($"{gameObject.name}: Moved {tilesMoved} tiles this turn - pausing");
                PauseMovement();
                return;
            }

            if (path.Count > 0)
            {
                // Check if next cell is still walkable
                MazeCell nextCell = path[0];
                if (!IsWalkable(currentCell, nextCell))
                {
                    // Path is blocked by walls, need to recalculate
                    Vector2Int targetPos = path[path.Count - 1].position;
                    path.Clear();
                    path = FindPath(gridPosition, targetPos);
                    
                    if (path.Count == 0)
                    {
                        Debug.LogWarning($"{gameObject.name}: Path blocked by walls and cannot recalculate");
                        StopMovement();
                        return;
                    }
                }
                // Check if next cell is occupied by another agent (only for hallways, not rooms)
                else if (!nextCell.roomTile && IsCellOccupiedByOther(nextCell.position))
                {
                    // Hallway is blocked by another agent, need to recalculate
                    Vector2Int targetPos = path[^1].position;
                    path.Clear();
                    path = FindPath(gridPosition, targetPos);
                    
                    if (path.Count == 0)
                    {
                        Debug.LogWarning($"{gameObject.name}: Path blocked by agent and cannot recalculate");
                        StopMovement();
                        return;
                    }
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
    
    private void ReserveNextCell()
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
       
        gameObject.GetComponent<DialogueBank>().alreadyTalked = false;
    }

    public void PauseMovement()
    {
        storedPath = new List<MazeCell>(path);
        path.Clear();
        isMoving = false;
    }
    
    public void RestorePath()
    {
        ValidateStoredPath();

        if (storedPath.Count > 0)
        {
            path = new List<MazeCell>(storedPath);
            storedPath.Clear();
            
            // Verify the first step is still valid
            Vector2Int currentPos = WorldToGridPosition(transform.position);
            MazeCell currentCell = gameManager.centralMaze[currentPos.x, currentPos.y];
            
            if (path.Count > 0 && IsWalkable(currentCell, path[0]) && !IsCellOccupiedByOther(path[0].position))
            {
                ReserveNextCell();
                isMoving = true;
                destination = GetWorldPosition(path[0].position);
            }
            else
            {
                // Path is immediately blocked, clear it
                Debug.LogWarning($"{gameObject.name}: Restored path is immediately blocked");
                path.Clear();
                isMoving = false;
            }
        }
        else
        {
            isMoving = false;
        }
    }

    private void ValidateStoredPath()
    {
        Vector2Int currentPos = WorldToGridPosition(transform.position);
        MazeCell currentCell = gameManager.centralMaze[currentPos.x, currentPos.y];
    
        for (int i = 0; i < storedPath.Count; i++)
        {
            MazeCell nextCell = storedPath[i];

            // Check walls AND agent occupation
            if (!IsWalkable(currentCell, nextCell) || IsCellOccupiedByOther(nextCell.position))
            {
                storedPath.RemoveRange(i, storedPath.Count - i);
                break;
            }

            currentCell = nextCell;
        }
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int gridX = Mathf.RoundToInt(worldPosition.x / gameManager.CellSize);
        int gridZ = Mathf.RoundToInt(worldPosition.z / gameManager.CellSize);
        return new Vector2Int(gridX, gridZ);
    }

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
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
                
                if (neighbor != targetCell && !neighbor.roomTile && IsCellOccupiedByOther(neighbor.position))
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
        
        // Stop any movement - path is no longer valid after swap
        StopMovement();
        
        // If we're in a room after the swap, update our current room tracking
        if (currentCell.roomTile)
        {
            currentRoom = currentCell.roomType;
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
        if (roomFearEffects.ContainsKey(roomType))
        {
            roomFearEffects[roomType]?.Invoke();
        }
    }
    
    private void IncreaseFear()
    {
        agentFear.AddFear(5);
    }

    private void DecreaseFear()
    {
        gameManager.SetGoalBool(this, true);
        agentFear.ReduceFear(5);
    }

    private enum RoomName
    {
        Study,
        DungeonRoom,
        Armory,
        GhostRoom,
        RitualRoom,
        HiddenTreasureRoom
    }
    
    private string GetRoomName(RoomName roomName)
    {
        return roomName switch
        {
            RoomName.Study => "Study",
            RoomName.DungeonRoom => "Dungeon Room",
            RoomName.Armory => "Armory",
            RoomName.GhostRoom => "Ghost Room",
            RoomName.RitualRoom => "Ritual Room",
            RoomName.HiddenTreasureRoom => "Hidden Treasure Room",
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
}