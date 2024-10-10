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
    public Vector3 destination;
    
    public float mazeHeight = 0.5f; // Y offset to maintain height on the maze

    private List<MazeCell> path = new List<MazeCell>();
    private bool isMoving = false;
    
    private Stack<Vector2Int> pathHistory = new Stack<Vector2Int>();

    [SerializeField] private int distance = 10;
    
    private List<Room> visitedRooms = new List<Room>();
    
    private Dictionary<Vector2Int, int> visitCount = new Dictionary<Vector2Int, int>();

    
    private void Start()
    {
        mazeRenderer.OnMazeGenerated += OnMazeGenerated;
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
            Debug.Log("No rooms visited yet.");
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

        Debug.Log("Visited Rooms: " + roomInfo);
    }

    public void SetDestination(Vector2Int target)
    {
        // Stop any current movement
        StopMovement();

        // Recalculate the agent's grid position
        Vector2Int currentPosition = WorldToGridPosition(transform.position);
        pathHistory.Push(currentPosition);
        
        // Find path from the current position to the target
        Vector2Int start = currentPosition;
        path = FindPath(start, target);

        if (path.Count > 0)
        {
            isMoving = true;
            destination = GetWorldPosition(path[0].position);
        }
    }

    private void FollowPath() {
        if (!isMoving || path.Count == 0) return;

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = destination;

        // Move towards the next cell on the path
        Vector3 direction = (targetPosition - currentPosition).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Maintain the agent's y position
        transform.position = new Vector3(transform.position.x, mazeHeight, transform.position.z);

        // Check if we have reached the current target
        if (Vector3.Distance(currentPosition, targetPosition) < 0.1f) {
            path.RemoveAt(0);

            // Convert the current world position to grid coordinates
            Vector2Int gridPosition = WorldToGridPosition(transform.position);

            // Check if the AI is standing over a new room tile (not a hallway)
            MazeCell currentCell = maze[gridPosition.x, gridPosition.y];
            if (!IsRoomVisited(gridPosition) && currentCell.roomTile) {
                // Only stop movement and mark as visited if it's a room tile
                Debug.Log("AI standing over new room, marking as visited.");
                MarkRoomAsVisited(gridPosition); // Mark the room as visited
                StopMovement(); // Stop further movement when standing over the room tile
                return; // Exit the method to stop further movement
            }

            if (path.Count > 0) {
                destination = GetWorldPosition(path[0].position);
            } else {
                isMoving = false; // Stop movement when path is complete
            }
        }
    }

    // Stops the agent's movement by clearing the path
    public void StopMovement()
    {
        path.Clear();
        isMoving = false;
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
        // Get the current grid position of the agent
        Vector2Int currentPosition = WorldToGridPosition(transform.position);

        // Use a breadth-first search (BFS) to explore walkable tiles within maxSteps
        Queue<(Vector2Int position, int steps)> frontier = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> reachableTiles = new List<Vector2Int>();

        // Start the search from the current position
        frontier.Enqueue((currentPosition, 0));
        visited.Add(currentPosition);

        while (frontier.Count > 0) {
            var (currentPos, steps) = frontier.Dequeue();

            // If we've reached max steps, stop adding new neighbors
            if (steps < distance) {
                // Get all walkable neighbors from the current position
                List<MazeCell> neighbors = GetNeighbors(maze[currentPos.x, currentPos.y]);

                foreach (MazeCell neighbor in neighbors) {
                    Vector2Int neighborPos = neighbor.position;
                    if (!visited.Contains(neighborPos)) {
                        visited.Add(neighborPos);
                        frontier.Enqueue((neighborPos, steps + 1));
                        reachableTiles.Add(neighborPos); // Store the reachable tile
                    }
                }
            }
        }

        // If we have reachable tiles within the step limit, pick one at random
        if (reachableTiles.Count > 0) {
            return reachableTiles[Random.Range(0, reachableTiles.Count)];
        }

        // If no reachable tiles are found within the step limit, return the current position (agent stays in place)
        return currentPosition;
    }
    
    public Vector2Int GetFarWalkableTile() {
        Vector2Int currentPosition = WorldToGridPosition(transform.position);

        // Define the corner distance threshold (e.g., 2 tiles from the corner)
        int cornerThreshold = 2;

        // Calculate the centroid of visited rooms (average position of all visited tiles)
        Vector2Int centroid = GetVisitedRoomsCentroid();

        // Check if the agent is near a corner or edge, and should ignore the centroid
        bool nearCorner = currentPosition.x < cornerThreshold || currentPosition.x > mazeGenerator.mazeWidth - cornerThreshold ||
                          currentPosition.y < cornerThreshold || currentPosition.y > mazeGenerator.mazeHeight - cornerThreshold;

        // Use a breadth-first search (BFS) to explore walkable tiles within maxSteps
        Queue<(Vector2Int position, int steps)> frontier = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> reachableTiles = new List<Vector2Int>();

        frontier.Enqueue((currentPosition, 0));
        visited.Add(currentPosition);

        while (frontier.Count > 0) {
            var (currentPos, steps) = frontier.Dequeue();

            if (steps < distance) {
                List<MazeCell> neighbors = GetNeighbors(maze[currentPos.x, currentPos.y]);

                foreach (MazeCell neighbor in neighbors) {
                    Vector2Int neighborPos = neighbor.position;
                    if (!visited.Contains(neighborPos)) {
                        visited.Add(neighborPos);
                        frontier.Enqueue((neighborPos, steps + 1));
                        reachableTiles.Add(neighborPos);
                    }
                }
            }
        }

        // If we have reachable tiles, calculate their scores based on proximity to unexplored rooms and visit count
        if (reachableTiles.Count > 0) {
            Vector2Int bestTile = reachableTiles[0];
            float bestScore = float.MinValue;

            foreach (Vector2Int tile in reachableTiles) {
                float proximityToUnexploredRoom = GetProximityToUnexploredRoom(tile);
                int tileVisitCount = visitCount.ContainsKey(tile) ? visitCount[tile] : 0;  // Default to 0 if not visited
                float score = proximityToUnexploredRoom - tileVisitCount * 0.5f;  // Penalize previously visited tiles

                // Prioritize unexplored rooms
                if (!nearCorner) {
                    float distanceFromCentroid = Vector2.Distance(tile, centroid);

                    // Define weights for the two factors
                    float distanceWeight = 0.3f;  // Less weight on distance from the centroid
                    float proximityWeight = 0.7f; // More weight on proximity to unexplored rooms

                    // Calculate the weighted score
                    score += (distanceWeight * distanceFromCentroid) + (proximityWeight * proximityToUnexploredRoom);
                }

                if (score > bestScore) {
                    bestScore = score;
                    bestTile = tile;
                }
            }

            return bestTile;  // Return the tile with the best score
        }

        return currentPosition;  // If no suitable tile, stay in place
    }
    
    // Check if the AI is moving in the same direction based on previous direction and current position
    private bool IsMovingInSameDirection(Vector2Int currentPosition, Vector2Int targetPosition, Vector2Int direction)
    {
        Vector2Int movementDirection = targetPosition - currentPosition;
        return movementDirection == direction;
    }
    
    private Vector2Int GetVisitedRoomsCentroid() {
        Vector2Int centroid = Vector2Int.zero;
        int totalTiles = 0;

        foreach (Room visitedRoom in visitedRooms) {
            foreach (Vector2Int tile in visitedRoom.tilesInRoom) {
                centroid += tile;
                totalTiles++;
            }
        }

        if (totalTiles > 0) {
            centroid /= totalTiles;  // Get the average position
        }

        return centroid;
    }
    
    private float GetProximityToUnexploredRoom(Vector2Int tilePosition) {
        List<MazeCell> neighbors = GetNeighbors(maze[tilePosition.x, tilePosition.y]);

        foreach (MazeCell neighbor in neighbors) {
            if (!IsRoomVisited(neighbor.position) && neighbor.roomTile) {
                return 1.0f;  // If the neighbor is unexplored, return a high score for proximity
            }
        }

        return 0.0f;  // If no unexplored neighbors, return a low score
    }
    
    
    public Vector2Int SmartBacktrack() {
        Vector2Int currentPosition = WorldToGridPosition(transform.position);
    
        // Check if we're in a corner or near the edge of the maze
        bool isInCorner = (currentPosition.x == 0 || currentPosition.x == mazeGenerator.mazeWidth - 1) &&
                          (currentPosition.y == 0 || currentPosition.y == mazeGenerator.mazeHeight - 1);
    
        // If in a corner, prioritize moving in the opposite direction of the corner
        if (isInCorner) {
            List<MazeCell> neighbors = GetNeighbors(maze[currentPosition.x, currentPosition.y]);

            // Find the first walkable neighbor that's away from the corner
            foreach (MazeCell neighbor in neighbors) {
                Vector2Int neighborPos = neighbor.position;

                // Prioritize moving away from the corner
                if ((neighborPos.x > currentPosition.x || neighborPos.x < currentPosition.x) &&
                    (neighborPos.y > currentPosition.y || neighborPos.y < currentPosition.y)) {
                    return neighborPos;
                }
            }
        }

        // If not in a corner, backtrack to a previously visited location or get a smart far walkable tile
        if (pathHistory.Count > 0) {
            Vector2Int backtrackPosition = pathHistory.Pop();

            // Avoid backtracking to heavily visited tiles
            if (visitCount.ContainsKey(backtrackPosition) && visitCount[backtrackPosition] > 2) {
                return GetFarWalkableTile();
            }
            return backtrackPosition;
        } else {
            return GetFarWalkableTile();  // Fall back to exploring far tiles
        }
    }

    
    public void Backtrack() {
        // Get a valid backtracking position using SmartBacktrack
        Vector2Int backtrackPosition = SmartBacktrack();

        // Set the AI's destination to the backtrack position
        SetDestination(backtrackPosition);
        Debug.Log("Backtracking to: " + backtrackPosition);
    }
    
    public Vector2Int ExploreMaze() {
        Vector2Int currentPosition = WorldToGridPosition(transform.position);

        // Calculate the centroid of visited rooms
        Vector2Int centroid = GetVisitedRoomsCentroid();

        // Define a threshold to determine if the agent is close to a wall or a corner
        int wallThreshold = 1;  // Defines proximity to the wall
        int cornerThreshold = 2; // Defines proximity to the corner

        // Check if the agent is near a wall or corner
        bool nearWall = currentPosition.x <= wallThreshold || currentPosition.x >= mazeGenerator.mazeWidth - wallThreshold ||
                        currentPosition.y <= wallThreshold || currentPosition.y >= mazeGenerator.mazeHeight - wallThreshold;

        bool nearCorner = currentPosition.x <= cornerThreshold || currentPosition.x >= mazeGenerator.mazeWidth - cornerThreshold ||
                          currentPosition.y <= cornerThreshold || currentPosition.y >= mazeGenerator.mazeHeight - cornerThreshold;

        // If the agent is near a wall or a corner, prioritize exploring underexplored areas
        if (nearWall || nearCorner) {
            Debug.Log("Near a wall or corner, prioritizing underexplored areas.");
            return ExploreUnderexploredAreas(currentPosition);
        }

        // If not near a wall or corner, proceed with normal exploration
        List<MazeCell> neighbors = GetNeighbors(maze[currentPosition.x, currentPosition.y]);

        // Check if any neighbor is an unexplored room
        foreach (MazeCell neighbor in neighbors) {
            if (!IsRoomVisited(neighbor.position) && neighbor.roomTile) {
                Debug.Log("Exploring: " + neighbor.roomType);
                return neighbor.position;
            }
        }

        // If no adjacent unexplored room, explore far walkable tiles, avoiding centroid
        if (Vector2.Distance(currentPosition, centroid) < distance) {
            Debug.Log("Avoiding centroid, exploring far tile.");
            return GetFarWalkableTile();
        }

        // Otherwise, explore normally within the maze
        return GetRandomWalkableTile();
    }


    private Vector2Int ExploreUnderexploredAreas(Vector2Int currentPosition) {
        List<MazeCell> neighbors = GetNeighbors(maze[currentPosition.x, currentPosition.y]);

        // Create a list to store possible destinations and their respective scores
        List<(Vector2Int position, float score)> scoredNeighbors = new List<(Vector2Int, float)>();

        foreach (MazeCell neighbor in neighbors) {
            if (!IsRoomVisited(neighbor.position) && neighbor.roomTile) {
                return neighbor.position; // Directly prioritize any unexplored room
            } else {
                // Score based on proximity to unexplored areas and visit count
                float proximityToUnexplored = GetProximityToUnexploredRoom(neighbor.position);
                int visitCountValue = visitCount.ContainsKey(neighbor.position) ? visitCount[neighbor.position] : 0;

                // Higher score for areas close to unexplored rooms and less visited
                float score = proximityToUnexplored - (visitCountValue * 0.5f);
                scoredNeighbors.Add((neighbor.position, score));
            }
        }

        // Select the best scored tile to explore
        if (scoredNeighbors.Count > 0) {
            scoredNeighbors.Sort((a, b) => b.score.CompareTo(a.score)); // Highest score first
            return scoredNeighbors[0].position;
        }

        // If no suitable tiles, fall back to a random walkable tile
        return GetRandomWalkableTile();
    }

    
    public void MarkRoomAsVisited(Vector2Int tilePosition) {
        foreach (Room room in MazeGenerator.Instance.rooms)
        {
            if (!room.tilesInRoom.Contains(tilePosition) || visitedRooms.Contains(room)) continue;
            Debug.Log("Added " + room);
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
    
    public bool IsAITurn() {
        return GameManager.Instance.IsAITurn();  // Assumes there's a turn manager handling this
    }
}
