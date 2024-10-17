using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AgentNavigation : MonoBehaviour
{
    public MazeGenerator mazeGenerator;
    public MazeRenderer mazeRenderer;
    private MazeCell[,] maze;
    private float CellSize;

    public float speed = 3.5f; // Movement speed
    public float mazeHeight = 0.5f; // Y offset to maintain height on the maze

    public List<MazeCell> path = new List<MazeCell>();
    private List<MazeCell> storedPath = new List<MazeCell>(); // Store the cleared path

    private bool isMoving = false;
    public bool hasDestination = false;
    public bool turnFinished = false;

    private Stack<Vector2Int> pathHistory = new Stack<Vector2Int>();
    private List<Room> visitedRooms = new List<Room>();
    private Dictionary<Vector2Int, int> visitCount = new Dictionary<Vector2Int, int>();

    public Vector3 destination;

    [SerializeField] private RoomName FearRoom;
    [SerializeField] private RoomName GoalRoom;

    private Room currentRoom; // Track the current room the agent is in
    private Dictionary<string, System.Action> roomFearEffects;

    [SerializeField] private Fear agentFear;
    
    private static List<AgentNavigation> allAgents = new List<AgentNavigation>();

    public bool hasSwappedProps = false; // Flag to ensure swapping only happens once
    
    private void Start()
    {
        // Initialize the dictionary with room reactions for this agent
        roomFearEffects = new Dictionary<string, System.Action>();
        
        roomFearEffects[GetRoomName(FearRoom)] = IncreaseFear;
        
        roomFearEffects[GetRoomName(GoalRoom)] = DecreaseFear;
    }
    
    private void Awake()
    {
        allAgents.Add(this);
        
        if (mazeRenderer != null)
        {
            mazeRenderer.OnMazeGenerated += OnMazeGenerated;
        }
        else
        {
            Debug.LogError("MazeRenderer instance is null.");
        }
    }
    
    private void OnDestroy()
    {
        allAgents.Remove(this);
    }


    private void OnMazeGenerated()
    {
        CellSize = mazeRenderer.CellSize;
        maze = mazeRenderer.GetMaze();
        
        // Example of setting a destination
        //SetDestination(new Vector2Int(mazeGenerator.mazeWidth - 1, mazeGenerator.mazeHeight - 1)); // Bottom-right corner of maze
    }

    private void Update()
    {
        FollowPath();
        
        if (visitedRooms.Count == 0) {
            return;
        }

        // Use a StringBuilder for efficiency when concatenating strings
        System.Text.StringBuilder roomInfo = new System.Text.StringBuilder();

        foreach (Room room in visitedRooms) {
            // Append each RoomID to the StringBuilder, followed by a comma
            roomInfo.Append(room.roomType + ", ");  // You can use 'room.Name' if you prefer room names
        }

        // Remove the last comma and space
        if (roomInfo.Length > 0)
            roomInfo.Length -= 2;

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

            // Convert the current world position to grid coordinates
            Vector2Int gridPosition = WorldToGridPosition(transform.position);

            // Update visit count for this tile
            visitCount.TryAdd(gridPosition, 0);
            visitCount[gridPosition]++; // Increment the visit count

            // Check if the AI is standing over a new room tile (not a hallway)
            MazeCell currentCell = maze[gridPosition.x, gridPosition.y];
            Room room = MazeGenerator.Instance.rooms.FirstOrDefault(r => r.tilesInRoom.Contains(gridPosition));
            

            // Collision detection and resolution
            ResolveCollision(gridPosition);

            if (!IsRoomVisited(gridPosition) && currentCell.roomTile)
            {
                Debug.Log("Stopping movement: New room found at " + gridPosition);
                MarkRoomAsVisited(gridPosition); 
                StopMovement(); 
                
                currentRoom = room;
                EnterRoom(room);
                return; 
            }
            
            if (currentCell.roomTile && currentRoom != room)
            {
                currentRoom = room;
                EnterRoom(room);
            }

            if (path.Count > 0)
            {
                destination = GetWorldPosition(path[0].position);
            }
            else
            {
                isMoving = false; // Stop movement when path is complete
            }
        }
    }
    
    private void ResolveCollision(Vector2Int gridPosition)
    {
        // Check for other agents on the same position
        var otherAgents = allAgents.Where(agent => agent != this && WorldToGridPosition(agent.transform.position) == gridPosition).ToList();

        if (otherAgents.Count > 0)
        {
            // Try to move each other agent to an adjacent tile
            foreach (var agent in otherAgents)
            {
                Vector2Int newGridPosition = FindAdjacentTile(agent, gridPosition);

                if (newGridPosition != gridPosition)
                {
                    // Start the sliding coroutine to move smoothly to the new position
                    StartCoroutine(SlideToPosition(agent, GetWorldPosition(newGridPosition)));
                
                    // Update the stored path with the new position
                    agent.UpdateStoredPath(newGridPosition);
                    agent.StopMovement();
                }
            }
        }
    }

    private IEnumerator SlideToPosition(AgentNavigation agent, Vector3 targetPosition)
    {
        Vector3 startPosition = agent.transform.position;

        // Keep sliding towards the target position
        while (Vector3.Distance(agent.transform.position, targetPosition) > 0.01f)
        {
            // Move towards the target position at the defined speed
            Vector3 direction = (targetPosition - agent.transform.position).normalized;
            agent.transform.position += direction * agent.speed * Time.deltaTime;

            // Maintain the agent's y position
            agent.transform.position = new Vector3(agent.transform.position.x, mazeHeight, agent.transform.position.z);

            yield return null; // Wait for the next frame
        }

        // Ensure the final position is exactly the target position
        agent.transform.position = targetPosition;
    }
    
    private Vector2Int FindAdjacentTile(AgentNavigation agent, Vector2Int currentGridPosition)
    {
        // Get all possible adjacent positions
        var possiblePositions = new List<Vector2Int>
        {
            new Vector2Int(currentGridPosition.x + 1, currentGridPosition.y),
            new Vector2Int(currentGridPosition.x - 1, currentGridPosition.y),
            new Vector2Int(currentGridPosition.x, currentGridPosition.y + 1),
            new Vector2Int(currentGridPosition.x, currentGridPosition.y - 1)
        };

        // Shuffle the list to add randomness to the selection
        possiblePositions = possiblePositions.OrderBy(x => Random.value).ToList();

        // Check if any of the adjacent positions are walkable and not occupied
        foreach (var position in possiblePositions)
        {
            if (IsWithinBounds(position) && !IsTileOccupied(position) && IsWalkable(maze[currentGridPosition.x, currentGridPosition.y], maze[position.x, position.y]))
            {
                return position;
            }
        }

        // Return the current position if no valid adjacent tile is found
        return currentGridPosition;
    }

    private void UpdateStoredPath(Vector2Int newGridPosition)
    {
        // If there is a stored path, update the first position to the new grid position
        if (storedPath.Count > 0)
        {
            storedPath[0] = maze[newGridPosition.x, newGridPosition.y];
        }
    }

    private bool IsTileOccupied(Vector2Int position)
    {
        return allAgents.Any(agent => WorldToGridPosition(agent.transform.position) == position);
    }

    private bool IsWithinBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < mazeGenerator.mazeWidth &&
               position.y >= 0 && position.y < mazeGenerator.mazeHeight;
    }
    
    // Stops the agent's movement by clearing the path
    public void StopMovement()
    {
        path.Clear();
        isMoving = false;
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
        MazeCell currentCell = maze[WorldToGridPosition(transform.position).x, WorldToGridPosition(transform.position).y];
    
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
        int gridX = Mathf.RoundToInt(worldPosition.x / CellSize);
        int gridZ = Mathf.RoundToInt(worldPosition.z / CellSize);
        return new Vector2Int(gridX, gridZ);
    }

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * CellSize, mazeHeight, gridPosition.y * CellSize);
    }

    private List<MazeCell> FindPath(Vector2Int start, Vector2Int target)
    {
        List<MazeCell> openSet = new List<MazeCell>();
        HashSet<MazeCell> closedSet = new HashSet<MazeCell>();

        MazeCell startCell = maze[start.x, start.y];
        MazeCell targetCell = maze[target.x, target.y];

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
                MazeCell neighborCell = maze[neighborPosition.x, neighborPosition.y];
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
                MazeCell neighborCell = maze[neighborPosition.x, neighborPosition.y];
            
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

    private bool IsWalkable(MazeCell fromCell, MazeCell toCell)
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

        while (frontier.Count > 0) {
            var (currentPos, steps) = frontier.Dequeue();

            // Removed arbitrary distance limit
            List<MazeCell> neighbors = GetNeighbors(maze[currentPos.x, currentPos.y]);

            foreach (MazeCell neighbor in neighbors) {
                Vector2Int neighborPos = neighbor.position;

                if (!visited.Contains(neighborPos)) {
                    visited.Add(neighborPos);
                    frontier.Enqueue((neighborPos, steps + 1));

                    // Dynamic scoring based on distance, visit frequency, and region
                    float distanceScore = steps;  // Favor tiles that are farther away
                    float visitScore = visitCount.ContainsKey(neighborPos) ? visitCount[neighborPos] : 0;  // Penalize frequently visited tiles

                    // Prioritize regions with fewer rooms
                    float regionPenalty = GetRegionRoomDensity(neighborPos);  // Lower score for areas with more rooms
                    float score = distanceScore - visitScore - regionPenalty;  // Adjust score based on all factors

                    scoredTiles.Add((neighborPos, score));
                }
            }
        }

        if (scoredTiles.Count > 0) {
            // Sort by score, highest score first
            scoredTiles.Sort((a, b) => b.score.CompareTo(a.score));
            return scoredTiles[0].position;
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

    private int CountRoomsInRegion(int regionX, int regionY) {
        // Iterate through visited rooms and count how many are in the region
        int roomCount = 0;
        foreach (Room room in visitedRooms) {
            if (IsRoomInRegion(room, regionX, regionY)) {
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
        MazeCell currentCell = maze[currentPosition.x, currentPosition.y];

        // Get neighbors that are in line of sight
        List<MazeCell> neighbors = GetVisibleNeighbors(currentCell);

        // First, check direct neighbors
        foreach (MazeCell neighbor in neighbors)
        {
            if (neighbor.roomTile && !IsRoomVisited(neighbor.position))
            {
                // If the neighbor is an unvisited room, return its position
                return neighbor.position;
            }

            // Now, get neighbors of this neighbor (2nd layer of neighbors)
            List<MazeCell> secondLayerNeighbors = GetVisibleNeighbors(neighbor);
            foreach (MazeCell secondNeighbor in secondLayerNeighbors)
            {
                if (secondNeighbor.roomTile && !IsRoomVisited(secondNeighbor.position))
                {
                    // If the second-layer neighbor is an unvisited room, return its position
                    return secondNeighbor.position;
                }
            }
        }

        // If no visible unvisited room is found, return the current position (no movement)
        return currentPosition;
    }

    
    public void MarkRoomAsVisited(Vector2Int tilePosition) {
        foreach (Room room in MazeGenerator.Instance.rooms)
        {
            if (!room.tilesInRoom.Contains(tilePosition) || visitedRooms.Contains(room)) continue;
            visitedRooms.Add(room);  // Mark all tiles in the room as visited

            // Update the visit count
            if (!visitCount.ContainsKey(tilePosition)) {
                visitCount[tilePosition] = 0;
            }
            visitCount[tilePosition]++;
            break;
        }
    }
    
    public bool IsRoomVisited(Vector2Int roomPosition)
    {
        foreach (Room room in MazeGenerator.Instance.rooms)
        {
            if (room.tilesInRoom.Contains(roomPosition))
            {
                // If any tile of the room is in visitedRooms, the room is considered visited
                return visitedRooms.Contains(room);
            }
        }
        return false;
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
    
    
    public void UpdateSwappedRooms(GameObject room1Ob, string newRoom1Type, GameObject room2Ob, string newRoom2Type)
    {
        // Get the positions of all cells in room1Ob
        var room1Positions = room1Ob.GetComponentsInChildren<MazeCellObject>()
            .Select(cell => new Vector2Int(cell.GetXCoord(), cell.GetYCoord())).ToList();

        // Get the positions of all cells in room2Ob
        var room2Positions = room2Ob.GetComponentsInChildren<MazeCellObject>()
            .Select(cell => new Vector2Int(cell.GetXCoord(), cell.GetYCoord())).ToList();

        // Find the corresponding rooms in the MazeGenerator
        Room room1 = MazeGenerator.Instance.rooms.FirstOrDefault(r => r.tilesInRoom.SequenceEqual(room1Positions));
        Room room2 = MazeGenerator.Instance.rooms.FirstOrDefault(r => r.tilesInRoom.SequenceEqual(room2Positions));

        if (room1 != null) room1.roomType = newRoom1Type;
        if (room2 != null) room2.roomType = newRoom2Type;

        // Update the visitedRooms list to reflect the new room types
        if (room1 != null && visitedRooms.Contains(room1))
        {
            visitedRooms.Remove(room1);
            room1.roomType = newRoom1Type;
            visitedRooms.Add(room1);
        }

        if (room2 != null && visitedRooms.Contains(room2))
        {
            visitedRooms.Remove(room2);
            room2.roomType = newRoom2Type;
            visitedRooms.Add(room2);
        }
    }

    public void SwapRoomProps(GameObject room1Ob, string room1Type, GameObject room2Ob, string room2Type)
    {
        if (hasSwappedProps) return; // Exit if already swapped

        // Find the props in the rooms based on the "Prop" tag
        GameObject room1Prop = FindPropWithTag(room1Ob, "Prop");
        GameObject room2Prop = FindPropWithTag(room2Ob, "Prop");

        // Ensure the props and parents are valid
        if (room1Prop != null && room2Prop != null)
        {
            Transform room1Parent = room1Ob.transform;
            Transform room2Parent = room2Ob.transform;

            // Store the local positions before destroying
            Vector3 room1LocalPosition = room1Prop.transform.localPosition;
            Vector3 room2LocalPosition = room2Prop.transform.localPosition;

            // Store the prefab references before destroying
            GameObject room1PropPrefab = Instantiate(room1Prop); // Create a copy of room1Prop
            GameObject room2PropPrefab = Instantiate(room2Prop); // Create a copy of room2Prop

            // Destroy the original GameObjects
            Destroy(room1Prop);
            Destroy(room2Prop);

            // Re-instantiate room1Prop under room2's parent
            GameObject newRoom1Prop = Instantiate(room1PropPrefab, room2Parent);
            newRoom1Prop.transform.localPosition = room2LocalPosition; // Set the local position to match the previous room2Prop

            // Re-instantiate room2Prop under room1's parent
            GameObject newRoom2Prop = Instantiate(room2PropPrefab, room1Parent);
            newRoom2Prop.transform.localPosition = room1LocalPosition; // Set the local position to match the previous room1Prop

            // Destroy the temporary prefab copies used for swapping
            Destroy(room1PropPrefab);
            Destroy(room2PropPrefab);

            hasSwappedProps = true;
        }
    }

    // Helper method to find the child GameObject with the specified tag
    private GameObject FindPropWithTag(GameObject parentObject, string tag)
    {
        foreach (Transform child in parentObject.transform)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        return null;
    }
    
    // This method is called when the agent enters a room
    public void EnterRoom(Room room)
    {
        if (!visitedRooms.Contains(room))
        {
            visitedRooms.Add(room); // Add the room to the visitedRooms if not already present
        }

        // Trigger the room fear effect if defined
        if (roomFearEffects.ContainsKey(room.roomType))
        {
            roomFearEffects[room.roomType]?.Invoke();
        }
    }
    
    private void IncreaseFear()
    {
        agentFear.AddFear(100);
        Debug.Log("Increased " + gameObject.name + "'s fear");
    }

    private void DecreaseFear()
    {
        agentFear.ReduceFear(5);
        Debug.Log("Decreased " + gameObject.name + "'s fear");
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
