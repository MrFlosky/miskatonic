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
    
    public float speed = 3.5f; // Movement speed
    public float mazeHeight = 0.5f; // Y offset to maintain height on the maze

    public List<MazeCell> path = new List<MazeCell>();
    public List<MazeCell> storedPath = new List<MazeCell>(); // Store the cleared path

    private bool isMoving = false;
    public bool hasDestination = false;
    public bool turnFinished = false;

    private Stack<Vector2Int> pathHistory = new Stack<Vector2Int>();
    private List<String> visitedRooms = new List<String>();
    private Dictionary<Vector2Int, int> visitCount = new Dictionary<Vector2Int, int>();

    public Vector3 destination;

    [SerializeField] private RoomName FearRoom;
    [SerializeField] private RoomName GoalRoom;

    private string currentRoom; // Track the current room the agent is in
    private Dictionary<string, System.Action> roomFearEffects;

    [SerializeField] private Fear agentFear;
    
    public int tileSpeed = 10;

    private void Start()
    {
        // Initialize the dictionary with room reactions for this agent
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
        // Follow the path
        FollowPath();

        if (visitedRooms.Count == 0) return;

        // Log the visited rooms' names
        string roomInfo = string.Join(", ", visitedRooms);
        Debug.Log(gameObject.name + "'s Visited Rooms: " + roomInfo);
    }

    public void SetDestination(Vector2Int target) {
        hasDestination = true;

        // Check if there is a stored path to restore
        if (storedPath.Count > 0) {
            RestorePath(); // Restore the previously stored path
            return; // Exit the method since we're following the restored path
        }

        // Stop any current movement
        StopMovement();

        // Recalculate the agent's grid position
        Vector2Int currentPosition = WorldToGridPosition(transform.position);
        pathHistory.Push(currentPosition);

        // Find a path from the current position to the target
        Vector2Int start = currentPosition;
        path = FindPath(start, target);

        if (path.Count > 0) {
            isMoving = true;  // Start moving toward the target
            destination = GetWorldPosition(path[0].position);  // Set the first destination in the path
        }
    }
    
    public void SetRoomDestination(Vector2Int target) {
        // Stop any current movement
        StopMovement();

        // Recalculate the agent's grid position
        Vector2Int currentPosition = WorldToGridPosition(transform.position);
        pathHistory.Push(currentPosition);

        // Find a path from the current position to the target
        Vector2Int start = currentPosition;
        path = FindPath(start, target);

        if (path.Count > 0) {
            isMoving = true;  // Start moving toward the target
            destination = GetWorldPosition(path[0].position);  // Set the first destination in the path
        }
    }

    private void FollowPath()
    {
        
        if (!isMoving || path.Count == 0) return;

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = destination;

        // Move towards the next cell on the path
        Vector3 direction = (targetPosition - currentPosition).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Maintain the agent's y position
        transform.position = new Vector3(transform.position.x, mazeHeight, transform.position.z);

        // Check if we have reached the current target
        if (Vector3.Distance(currentPosition, targetPosition) < 0.1f)
        {
            path.RemoveAt(0);

            Vector2Int gridPosition = WorldToGridPosition(transform.position);
            MazeCell currentCell = gameManager.centralMaze[gridPosition.x, gridPosition.y];

            // Get the room type of the current cell
            string roomType = currentCell.roomType;

            // Check if the room type has not been visited
            if (!IsRoomVisited(roomType) && currentCell.roomTile)
            {
                MarkRoomAsVisited(roomType);
                
                EnterRoom(roomType);
                StopMovement();
                return;
            }
            
            if (currentCell.roomTile && currentRoom != currentCell.roomType)
            {
                EnterRoom(roomType);
            }
            else if (currentCell.roomType == "Hallway")
                currentRoom = null;

            // Set the next destination if there are more steps in the path
            if (path.Count > 0)
            {
                destination = GetWorldPosition(path[0].position);
            }
            else
            {
                StopMovement();
            }
        }
    }



    // Stops the agent's movement by clearing the path
    public void StopMovement()
    {
        path.Clear();
        isMoving = false;
       
        //allows new dialogue
        gameObject.GetComponent<DialogueBank>().clicked = false;
    }

    public void PauseMovement()
    {
        // Store the current path in case we need it later
        storedPath = new List<MazeCell>(path);  // Creates a copy of the current path
        
        // Clear the current path
        path.Clear();

        // Stop movement
        isMoving = false;
        
    }
    
    
    // Modified RestorePath method to validate the path before restoring it
    public void RestorePath()
    {
        ValidateStoredPath();  // Validate the stored path

        if (storedPath.Count > 0)
        {
            path = new List<MazeCell>(storedPath);  // Restore the validated path
            storedPath.Clear();  // Clear the stored path to prevent reusing it
            isMoving = true;  // Resume movement
            destination = GetWorldPosition(path[0].position);  // Set the next destination
            
        }
        else
        {
            isMoving = false;  // Stop movement if no valid path remains
        }
    }
    
    // Method to validate the stored path and remove steps if a wall is active
    private void ValidateStoredPath()
    {
        // Start with the cell the agent is currently on
        MazeCell currentCell = gameManager.centralMaze[WorldToGridPosition(transform.position).x, WorldToGridPosition(transform.position).y];
    
        // Iterate through the stored path, starting with the next cell
        for (int i = 0; i < storedPath.Count; i++)
        {
            MazeCell nextCell = storedPath[i];

            // Check if there's a wall blocking the path between currentCell and nextCell
            if (!IsWalkable(currentCell, nextCell))
            {
                // Truncate the path at the current index (including the blocked cell)
                storedPath.RemoveRange(i, storedPath.Count - i);
                break; // Exit the loop as the path is no longer valid beyond this point
            }

            // Move to the next cell
            currentCell = nextCell;
        }
    }
    

    // Converts a world position to grid coordinates (Vector2Int)
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
            // Find the node in openSet with the lowest fScore
            MazeCell current = openSet[0];
            foreach (MazeCell cell in openSet)
            {
                if (fScore.ContainsKey(cell) && fScore[cell] < fScore[current])
                {
                    current = cell;
                }
            }

            // If we've reached the target, construct the path
            if (current == targetCell)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // Check each neighbor of the current cell
            foreach (MazeCell neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                float tentativeGScore = gScore[current] + 1; // Assuming uniform cost for each step

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor])
                {
                    continue;
                }

                // This path is the best so far
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, targetCell);
            }
        }

        // If no path is found
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

            // Ensure the neighbor is within bounds and is walkable (no walls)
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

            // Ensure the neighbor is within bounds
            if (neighborPosition.x >= 0 && neighborPosition.x < mazeGenerator.mazeWidth &&
                neighborPosition.y >= 0 && neighborPosition.y < mazeGenerator.mazeHeight)
            {
                MazeCell neighborCell = gameManager.centralMaze[neighborPosition.x, neighborPosition.y];
            
                // Check line-of-sight (no walls between current cell and neighbor)
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
        // Check walls based on direction
        if (toCell.x > fromCell.x && !fromCell.rightWall && !toCell.leftWall) return true; // Right
        if (toCell.x < fromCell.x && !fromCell.leftWall && !toCell.rightWall) return true; // Left
        if (toCell.y > fromCell.y && !fromCell.topWall && !toCell.bottomWall) return true; // Up
        if (toCell.y < fromCell.y && !fromCell.bottomWall && !toCell.topWall) return true; // Down

        return false;
    }

    private float Heuristic(MazeCell a, MazeCell b)
    {
        // Use Manhattan distance as the heuristic
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
        // Check if the agent has no more path to follow and has reached the last destination
        return !isMoving && path.Count == 0;
    }
    
    public Vector2Int GetRandomWalkableTile() {
        Vector2Int currentPosition = WorldToGridPosition(transform.position);

        Queue<(Vector2Int position, int steps)> frontier = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<(Vector2Int position, float score)> scoredTiles = new List<(Vector2Int, float)>();

        frontier.Enqueue((currentPosition, 0));
        visited.Add(currentPosition);

        System.Random random = new System.Random(gameObject.GetInstanceID());  // Unique seed for each agent

        while (frontier.Count > 0) {
            var (currentPos, steps) = frontier.Dequeue();
            List<MazeCell> neighbors = GetNeighbors(gameManager.centralMaze[currentPos.x, currentPos.y]);

            foreach (MazeCell neighbor in neighbors) {
                Vector2Int neighborPos = neighbor.position;

                if (!visited.Contains(neighborPos)) {
                    visited.Add(neighborPos);
                    frontier.Enqueue((neighborPos, steps + 1));

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
            // Sort by score and pick a random tile from the top 3
            scoredTiles.Sort((a, b) => b.score.CompareTo(a.score));
            int randomIndex = random.Next(Mathf.Min(3, scoredTiles.Count));  // Select one of the top 3 tiles
            return scoredTiles[randomIndex].position;
        }

        return currentPosition;  // Stay in place if no walkable tiles found
    }

    
    private float GetRegionRoomDensity(Vector2Int position) {
        // Divide maze into regions (e.g., left, right, center) or sectors
        // Example: use grid coordinates to classify the region
        int regionX = position.x / (mazeGenerator.mazeWidth / 3);  // Divide width into 3 regions
        int regionY = position.y / (mazeGenerator.mazeHeight / 3); // Divide height into 3 regions
    
        // Get the number of rooms found in this region
        int roomsInRegion = CountRoomsInRegion(regionX, regionY);

        // Higher penalty for regions with more rooms found
        return roomsInRegion * 0.5f;  // Adjust weight based on room density
    }

    private int CountRoomsInRegion(int regionX, int regionY) 
    {
        // Get all rooms from the GameManager
        var allRooms = GameManager.Instance.GetRooms();

        int roomCount = 0;

        // Iterate through the rooms in the GameManager and check if they are visited and in the region
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
        // Get the AI's current grid position
        Vector2Int currentPosition = WorldToGridPosition(transform.position);

        // Get the current cell the AI is in
        MazeCell currentCell = gameManager.centralMaze[currentPosition.x, currentPosition.y];

        // Get neighbors that are in line of sight
        List<MazeCell> neighbors = GetVisibleNeighbors(currentCell);

        // First, check direct neighbors
        foreach (MazeCell neighbor in neighbors)
        {
            Room neighborRoom = gameManager.centralMaze[neighbor.x, neighbor.y].room;

            if (neighborRoom != null && neighbor.roomTile && !IsRoomVisited(neighborRoom.roomType))
            {
                // If the neighbor is an unvisited room, return its position
                return neighbor.position;
            }

            // Now, get neighbors of this neighbor (2nd layer of neighbors)
            List<MazeCell> secondLayerNeighbors = GetVisibleNeighbors(neighbor);
            foreach (MazeCell secondNeighbor in secondLayerNeighbors)
            {
                Room secondNeighborRoom = GameManager.Instance.GetRooms().FirstOrDefault(r => r.tilesInRoom.Contains(secondNeighbor.position));

                if (secondNeighborRoom != null && secondNeighbor.roomTile && !IsRoomVisited(secondNeighborRoom.roomType))
                {
                    // If the second-layer neighbor is an unvisited room, return its position
                    return secondNeighbor.position;
                }
            }
        }

        // If no visible unvisited room is found, return the current position (no movement)
        return currentPosition;
    }
    
    
    public void MarkRoomAsVisited(string roomType)
    {
        if (!visitedRooms.Contains(roomType))
        {
            visitedRooms.Add(roomType); // Add the room type to the set of visited rooms
            path.Clear();
        }
    }

    public bool IsRoomVisited(string roomType)
    {
        return visitedRooms.Contains(roomType);
    }

    
    
    // Method to clear the random position
    public void ClearRandomPosition() {
        // Reset the random position to zero and unset the flag
        hasDestination = false;
    }
    

    // Method to check if a random position is set
    public bool IsDestinationSet() {
        return hasDestination;
    }
    
    // This method is called when the agent enters a room
    public void EnterRoom(string roomType)
    {
        currentRoom = roomType;
        // Trigger the room fear effect if defined
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
}
