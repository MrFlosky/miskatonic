using Cinemachine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.AI;
using Random = UnityEngine.Random;

/// <summary>
/// Recursive maze carving
/// Room placement with randomized types and dimesions
/// Path validation ro wnsure full maze connectivity
/// Dynamic wall management and updated
/// Room entrances and special tile handiling
/// </summary>
public class MazeGenerator : MonoBehaviour {

    
    public static MazeGenerator Instance;

    [Header("Maze Size")]
    [Range(5, 500)]
    public int mazeWidth = 5, mazeHeight = 5;           // The dimensions of the maze.

    [Header("Starting point")]
    public int startX, startY;                          // The position our algorithm will start from.
    
    [Header("Room Settings")]
    [Range(0, 10)] public int roomCount = 1;            // The numver of rooms in the maze
    [Range(2,20)] public int roomXSizeMin, roomXSizeMax, roomYSizeMin, roomYSizeMax = 5;    // Randomize rooms' size 

    MazeCell[,] maze;                                   // An array of maze cells representing the maze grid.
    Vector2Int currentCell;                             // The maze cell we are currently looking at.

    private List<string> _roomTypes;                    // All the room types
      
    public List<Room> rooms = new List<Room>();         // Sucessfully created room

    [Header("Decoration Settings")]
    [Range(0f, 1f)] public float wallPropChance = 0.2f;   // 20% chance per wall
    public List<GameObject> wallProps;                    // assign in Inspector
    public Transform propParent;                         // parent container for organization    

    private void Start()
    {
        Instance = this;
    }


    /// <summary>
    /// Generates a new maze with rooms and return the maze grid
    /// Retries generation if room placement fails
    /// </summary>
    /// <returns></returns>
    public MazeCell[,] GetMaze() 
    {
        int maxAttempts = 100; // Limit the number of retries to avoid infinite loops
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // Initialize the maze
            maze = new MazeCell[mazeWidth, mazeHeight];
            for (int x = 0; x < mazeWidth; x++) 
            {
                for (int y = 0; y < mazeHeight; y++) 
                {
                    maze[x, y] = new MazeCell(x, y);
                }
            }

            // Start carving the maze path
            CarvePath(startX, startY);
        
            // Create all room types
            _roomTypes = new List<string> {
                "Library", "Dungeon", "Dining Room", "Cemetery", "Ritual Room", "Bedroom"
            };
        
            // Try to create the specified number of rooms
            int createdRoomCount = 0;
            for (int i = 0; i < roomCount; i++)
            {
                if (CreateRoom(maze))
                {
                    createdRoomCount++;
                }
            }

            // If we successfully created the required number of rooms, return the maze
            if (createdRoomCount == roomCount)
            {
                UpdateAllOpposingWalls();
                PlaceWallProps();
                return maze;
            }
        
            // Increment the attempt counter and retry
            rooms.Clear();
            attempts++;
        }

        // If we reached the maximum number of attempts and still failed, log an error
        Debug.LogError($"Failed to generate a maze with {roomCount} rooms after {maxAttempts} attempts.");
        UpdateAllOpposingWalls();
        PlaceWallProps();
        return maze; // Return the last attempted maze (could be incomplete)
    }

    List<Directions> directions = new List<Directions> {
 
        Directions.Up, Directions.Down, Directions.Left, Directions.Right,
    
    };


    /// <summary>
    /// Randomnizes the order of the four cardinal directions.
    /// </summary>
    /// <returns></returns>
    List<Directions> GetRandomDirections() {

        // Make a copy of our directions list that we can mess around with.
        List<Directions> dir = new List<Directions>(directions);

        // Make a directions list to put our randomised directions into.
        List<Directions> rndDir = new List<Directions>();

        while (dir.Count > 0)                           // Loop until our rndDir list is empty.
        {                         
            int rnd = Random.Range(0, dir.Count);       // Get random index in list.
            rndDir.Add(dir[rnd]);                       // Add the random direction to our list.
            dir.RemoveAt(rnd);                          // Remove that direction so we can't choose it again

        }

        // When we've got all four directions in a random order, return the queue.
        return rndDir;

    }

    /// <summary>
    /// Checks if a cell coordinate is within bounds and unvisited
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    bool IsCellValid (int x, int y) {

        // If the cell is outside of the map or has already been visited, we consider it not valid.
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1 || maze[x, y].visited) return false;
        else return true;

    }


    /// <summary>
    /// Selects a random valid neighbor of the current cell.
    /// </summary>
    /// <returns></returns>
    Vector2Int CheckNeighbours() {

        List<Directions> rndDir = GetRandomDirections();

        for (int i = 0; i < rndDir.Count; i++) 
        {
            // Set neighbour coordinates to current cell for now.
            Vector2Int neighbour = currentCell;

            // Modify neighbour coordinates based on the random directions we're currently trying.
            switch (rndDir[i]) 
            {
                case Directions.Up:
                    neighbour.y++;
                    break;
                case Directions.Down:
                    neighbour.y--;
                    break;
                case Directions.Right:
                    neighbour.x++;
                    break;
                case Directions.Left:
                    neighbour.x--;
                    break;
            }

            // If the neighbour we just tried is valid, we can return that neighbour. If not, we go again.
            if (IsCellValid(neighbour.x, neighbour.y)) return neighbour;

        }

        // If we tried all directions and didn't find a valid neighbour, we return the currentCell values.
        return currentCell;
    }


    /// <summary>
    /// Removes walls between two adjacent cells.
    /// Takes in two maze positions and sets the cells accordingly.
    /// </summary>
    /// <param name="primaryCell"></param>
    /// <param name="secondaryCell"></param>
    void BreakWalls (Vector2Int primaryCell, Vector2Int secondaryCell) {

        // We can only go in one direction at a time so we can handle this using if else statements.
        if (primaryCell.x > secondaryCell.x) { // Primary Cell's Left Wall

            maze[primaryCell.x, primaryCell.y].leftWall = false;

        } else if (primaryCell.x < secondaryCell.x) { // Secondary Cell's Left Wall

            maze[secondaryCell.x, secondaryCell.y].leftWall = false;

        } else if (primaryCell.y < secondaryCell.y) { // Primary Cell's Top Wall

            maze[primaryCell.x, primaryCell.y].topWall = false;

        } else if (primaryCell.y > secondaryCell.y) { // Secondary Cell's Top Wall

            maze[secondaryCell.x, secondaryCell.y].topWall = false;

        }
    }

    /// <summary>
    /// Used by SkillBarrier to set the internal state of a given cells wall within the maze array.
    /// Needed so AI identifies the newly placed walls.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="wallType"></param>
    /// <param name="wantActivated">
    /// This param determines if we want the AI to see this wall or not. In the case of it being destroyed no.
    /// Case of being activated, yes.
    /// </param>
    public void SetWallInternalState(int x, int y, WallType wallType, bool wantActivated)
    {
        switch(wallType)
        {
            case WallType.Top:
            maze[x,y].topWall = wantActivated;
            break;

            case WallType.Bottom:
            maze[x,y].bottomWall = wantActivated;
            break;

            case WallType.Left:
            maze[x,y].leftWall = wantActivated;
            break;

            case WallType.Right:
            maze[x,y].rightWall = wantActivated;
            break;

        }
    }


    /// <summary>
    /// Starting at the x, y passed in, carves a path through the maze until it encounters a "dead end" using DFS
    /// (a dead end is a cell with no valid neighbours).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void CarvePath (int x, int y) {

        // Perform a quick check to make sure our start position is within the boundaries of the map,
        // if not, set them to a default (I'm using 0) and throw a little warning up.
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1) {

            x = y = 0;
            Debug.LogWarning("Starting position is out of bounds, defaulting to 0, 0");

        }

        // Set current cell to the starting position we were passed.
        currentCell = new Vector2Int(x, y);

        // A list to keep track of our current path.
        List<Vector2Int> path = new List<Vector2Int>();

        // Loop until we encounter a dead end.
        bool deadEnd = false;
        while (!deadEnd) {

            // Get the next cell we're going to try.
            Vector2Int nextCell = CheckNeighbours();

            // If that cell has no valid neighbours, set deadend to true so we break out of the loop.
            if (nextCell == currentCell) {

                // If that cell has no valid neighbours, set deadend to true so we break out of the loop.
                for (int i = path.Count - 1; i >= 0; i--) {

                    currentCell = path[i];                              // Set currentCell to the next step back along our path.
                    path.RemoveAt(i);                                   // Remove this step from the path.
                    nextCell = CheckNeighbours();                       // Check that cell to see if any other neighbours are valid.

                    // If we find a valid neighbour, break out of the loop.
                    if (nextCell != currentCell) break;

                }

                if (nextCell == currentCell)
                    deadEnd = true;

            } else {

                BreakWalls(currentCell, nextCell);                      // Set wall flags on these two cells.
                maze[currentCell.x, currentCell.y].visited = true;      // Set cell to visited before moving on.
                currentCell = nextCell;                                 // Set the current cell to the valid neighbour we found.
                path.Add(currentCell);                                  // Add this cell to our path

            }
        }
    }

    /// <summary>
    /// Attempts to place a random room in the maze grid.
    /// </summary>
    /// <param name="maze"></param>
    /// <returns></returns>
    public bool CreateRoom(MazeCell[,] maze)
    {
        int xLength = Random.Range(roomXSizeMin, roomXSizeMax); // get random x size of room
        int yLength = Random.Range(roomYSizeMin, roomYSizeMax); // get random y size of room

        int startX = Random.Range(Mathf.RoundToInt(xLength / 2), mazeWidth - Mathf.RoundToInt(xLength / 2));
        int startY = Random.Range(Mathf.RoundToInt(yLength / 2), mazeHeight - Mathf.RoundToInt(yLength / 2));

        int farLeftX = startX - Mathf.RoundToInt(xLength / 2);
        int farRightX = startX + Mathf.CeilToInt(xLength / 2);

        int farBottomY = startY - Mathf.RoundToInt(yLength / 2);
        int farTopY = startY + Mathf.CeilToInt(yLength / 2);        

        bool success = true;
        int tries = 0;
        while (tries < 100)
        {
            success = true;
            for (int x = farLeftX; x < farRightX + 1; x++)
            {
                for (int y = farBottomY; y < farTopY + 1; y++)
                {
                    if (x < 0 || y < 0 || x >= mazeWidth || y >= mazeHeight || maze[x, y].roomTile || maze[x, y].outsideTile)
                    {
                        success = false;
                        break;
                    }
                }
                if (!success) break;
            }
            if (success) break;
            
            // Try new dimensions and starting position
            xLength = Random.Range(roomXSizeMin, roomXSizeMax);
            yLength = Random.Range(roomYSizeMin, roomYSizeMax);
            startX = Random.Range(Mathf.RoundToInt(xLength / 2), mazeWidth - Mathf.RoundToInt(xLength / 2));
            startY = Random.Range(Mathf.RoundToInt(yLength / 2), mazeHeight - Mathf.RoundToInt(yLength / 2));
            farLeftX = startX - Mathf.RoundToInt(xLength / 2);
            farRightX = startX + Mathf.CeilToInt(xLength / 2);
            farBottomY = startY - Mathf.RoundToInt(yLength / 2);
            farTopY = startY + Mathf.CeilToInt(yLength / 2);
            
            tries++;
        }

        if (!success) return false; // Failed to place the room

        // Create the room
        var rnd = Random.Range(0, _roomTypes.Count);
        Room newRoom = new Room(_roomTypes[rnd]);
        
        for (int x = farLeftX - 1; x < farRightX + 2; x++)
        {
            for (int y = farBottomY - 1; y < farTopY + 2; y++)
            {
                if (x >= 0 && y >= 0 && x < mazeWidth && y < mazeHeight)
                {
                    maze[x, y].outsideTile = true;
                }
            }
        }
        
        for (int x = farLeftX; x < farRightX + 1; x++)
        {
            for (int y = farBottomY; y < farTopY + 1; y++)
            {
                MazeCell selectedCell = maze[x, y];
                selectedCell.rightWall = selectedCell.leftWall = selectedCell.topWall = selectedCell.bottomWall = false;
                if (x == farLeftX) selectedCell.leftWall = true;
                if (x == farRightX) selectedCell.rightWall = true;
                if (y == farTopY) selectedCell.topWall = true;
                if (y == farBottomY) selectedCell.bottomWall = true;
                selectedCell.roomTile = true;
                selectedCell.room = newRoom;
                selectedCell.roomType = _roomTypes[rnd];
                newRoom.AddTile(selectedCell.position);
            }
        }

        // Create entrances
        if (farLeftX > 0)
        {
            int entranceY = (farBottomY + farTopY) / 2;
            maze[farLeftX, entranceY].leftWall = false;
            maze[farLeftX - 1, entranceY].rightWall = false;
        }
        if (farRightX < mazeWidth - 1)
        {
            int entranceY = (farBottomY + farTopY) / 2;
            maze[farRightX, entranceY].rightWall = false;
            maze[farRightX + 1, entranceY].leftWall = false;
        }
        if (farBottomY > 0)
        {
            int entranceX = (farLeftX + farRightX) / 2;
            maze[entranceX, farBottomY].bottomWall = false;
            maze[entranceX, farBottomY - 1].topWall = false;
        }
        if (farTopY < mazeHeight - 1)
        {
            int entranceX = (farLeftX + farRightX) / 2;
            maze[entranceX, farTopY].topWall = false;
            maze[entranceX, farTopY + 1].bottomWall = false;
        }

        // Remove room type from list and add to rooms
        _roomTypes.RemoveAt(rnd);
        rooms.Add(newRoom);        

        return true; // Room successfully created
    }
    
    public MazeCell[,] GenerateValidMaze()
    {
        int maxFixAttempts = 1000; // Maximum number of wall-breaking attempts to avoid infinite loops
        int fixAttempts = 0;

        // Generate the initial maze
        MazeCell[,] generatedMaze = GetMaze();
    
        // Loop until the maze is fully traversable or max attempts are reached
        while (!IsMazeFullyTraversable() && fixAttempts < maxFixAttempts)
        {
            // Assign reachability values to tiles
            int[,] tileValues = AssignTileValues();

            // Attempt to connect unreachable clusters to reachable areas
            ConnectClusters(tileValues);

            // Increment the fix attempt counter
            fixAttempts++;
        }

        return generatedMaze;
    }

    // Assigns values to each tile based on reachability
    private int[,] AssignTileValues()
    {
        int[,] tileValues = new int[mazeWidth, mazeHeight];

        // Use BFS or flood fill to mark reachable tiles starting from (startX, startY)
        Queue<MazeCell> queue = new Queue<MazeCell>();
        HashSet<MazeCell> visited = new HashSet<MazeCell>();
        
        // Start from the initial position
        queue.Enqueue(maze[startX, startY]);
        visited.Add(maze[startX, startY]);
        tileValues[startX, startY] = 1; // Mark as reachable

        // Perform BFS to mark reachable tiles
        while (queue.Count > 0)
        {
            MazeCell current = queue.Dequeue();
            foreach (MazeCell neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor) && !neighbor.outsideTile)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                    tileValues[neighbor.x, neighbor.y] = 1; // Mark as reachable
                }
            }
        }

        return tileValues;
    }

    // Connects clusters of unreachable tiles to reachable areas
    private void ConnectClusters(int[,] tileValues)
    {
        // Find clusters of tiles with value 0 (unreachable)
        List<HashSet<MazeCell>> clusters = FindClusters(tileValues, 0);

        // Try to connect each cluster to a reachable tile
        foreach (var cluster in clusters)
        {
            foreach (MazeCell cell in cluster)
            {
                // Get reachable neighbors
                List<MazeCell> reachableNeighbors = GetReachableNeighbors(cell, tileValues);
                if (reachableNeighbors.Count > 0)
                {
                    // Connect to the first reachable neighbor
                    BreakWallBetween(cell, reachableNeighbors[0]);
                    tileValues[cell.x, cell.y] = 1; // Mark this tile as reachable

                    // Update all tiles in the cluster to 1, as they should now be reachable
                    foreach (MazeCell clusterCell in cluster)
                    {
                        tileValues[clusterCell.x, clusterCell.y] = 1;
                    }
                    break;
                }
            }
        }
    }

    // Finds clusters of tiles with a specified value (e.g., value 0 for unreachable)
    private List<HashSet<MazeCell>> FindClusters(int[,] tileValues, int targetValue)
    {
        List<HashSet<MazeCell>> clusters = new List<HashSet<MazeCell>>();
        HashSet<MazeCell> visited = new HashSet<MazeCell>();

        // Loop through all tiles
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (tileValues[x, y] != targetValue || visited.Contains(maze[x, y]))
                {
                    continue; // Skip tiles that are already visited or do not match the target value
                }

                // Find a cluster of tiles with the target value
                HashSet<MazeCell> cluster = new HashSet<MazeCell>();
                Queue<MazeCell> queue = new Queue<MazeCell>();
                queue.Enqueue(maze[x, y]);
                visited.Add(maze[x, y]);

                while (queue.Count > 0)
                {
                    MazeCell current = queue.Dequeue();
                    cluster.Add(current);

                    // Check neighbors and add to the cluster if they match the target value
                    foreach (MazeCell neighbor in GetNeighbors(current))
                    {
                        if (!visited.Contains(neighbor) && tileValues[neighbor.x, neighbor.y] == targetValue)
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                clusters.Add(cluster);
            }
        }

        return clusters;
    }

    // Gets the reachable neighbors of a cell
    private List<MazeCell> GetReachableNeighbors(MazeCell cell, int[,] tileValues)
    {
        List<MazeCell> neighbors = new List<MazeCell>();

        int x = cell.x;
        int y = cell.y;

        // Check each direction for neighbors with a value of 1 (reachable)
        if (x > 0 && tileValues[x - 1, y] == 1) neighbors.Add(maze[x - 1, y]); // Left
        if (x < mazeWidth - 1 && tileValues[x + 1, y] == 1) neighbors.Add(maze[x + 1, y]); // Right
        if (y > 0 && tileValues[x, y - 1] == 1) neighbors.Add(maze[x, y - 1]); // Down
        if (y < mazeHeight - 1 && tileValues[x, y + 1] == 1) neighbors.Add(maze[x, y + 1]); // Up

        return neighbors;
    }
    
    private bool IsMazeFullyTraversable()
    {
        // Perform flood fill from the starting position to count the number of accessible cells
        int accessibleCount = CountAccessibleSpaces(new Vector2Int(startX, startY));

        // Compare with the total number of traversable cells (excluding outside tiles)
        int totalTraversableCells = mazeWidth * mazeHeight - GetNumberOfOutsideTiles();
        // Log the results for debugging
        // If all traversable cells are accessible, return true
        return accessibleCount == totalTraversableCells;
    }

    private int CountAccessibleSpaces(Vector2Int start)
    {
        // Use a queue for BFS
        Queue<MazeCell> openSet = new Queue<MazeCell>();
        HashSet<MazeCell> visited = new HashSet<MazeCell>();

        // Add the start cell to the open set
        MazeCell startCell = maze[start.x, start.y];
        openSet.Enqueue(startCell);
        visited.Add(startCell);

        int accessibleCount = 0;

        // Perform BFS to count accessible spaces
        while (openSet.Count > 0)
        {
            MazeCell current = openSet.Dequeue();
            accessibleCount++;

            // Check each neighbor
            foreach (MazeCell neighbor in GetNeighbors(current))
            {
                // Skip if already visited or if it is an outside tile
                if (visited.Contains(neighbor) || neighbor.outsideTile) continue;

                // Mark the neighbor as visited
                visited.Add(neighbor);
                openSet.Enqueue(neighbor);
            }
        }

        return accessibleCount;
    }

    private List<MazeCell> GetNeighbors(MazeCell cell)
    {
        List<MazeCell> neighbors = new List<MazeCell>();

        int x = cell.x;
        int y = cell.y;

        // Check each direction and add if no walls block the path
        if (x > 0 && !maze[x, y].leftWall && !maze[x - 1, y].rightWall) neighbors.Add(maze[x - 1, y]); // Left
        if (x < mazeWidth - 1 && !maze[x, y].rightWall && !maze[x + 1, y].leftWall) neighbors.Add(maze[x + 1, y]); // Right
        if (y > 0 && !maze[x, y].bottomWall && !maze[x, y - 1].topWall) neighbors.Add(maze[x, y - 1]); // Down
        if (y < mazeHeight - 1 && !maze[x, y].topWall && !maze[x, y + 1].bottomWall) neighbors.Add(maze[x, y + 1]); // Up

        return neighbors;
    }

    private int GetNumberOfOutsideTiles()
    {
        int count = 0;
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (maze[x, y].outsideTile)
                {
                    count++;
                }
            }
        }
        return count;
    }
    
    private void BreakWallBetween(MazeCell cellA, MazeCell cellB)
    {
        // Determine which wall to break based on the relative position of cellA and cellB
        if (cellA.x == cellB.x)
        {
            // Same column, check rows
            if (cellA.y < cellB.y)
            {
                // cellA is below cellB
                cellA.topWall = false;
                cellB.bottomWall = false;
            }
            else
            {
                // cellA is above cellB
                cellA.bottomWall = false;
                cellB.topWall = false;
            }
        }
        else if (cellA.y == cellB.y)
        {
            // Same row, check columns
            if (cellA.x < cellB.x)
            {
                // cellA is to the left of cellB
                cellA.rightWall = false;
                cellB.leftWall = false;
            }
            else
            {
                // cellA is to the right of cellB
                cellA.leftWall = false;
                cellB.rightWall = false;
            }
        }
    }
    

    public void UpdateAllOpposingWalls()
    {
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                // Update the neighboring walls based on the current cell's walls
                if (maze[x, y].topWall && y < mazeHeight - 1)
                {
                    maze[x, y + 1].bottomWall = true;
                }
                if (maze[x, y].bottomWall && y > 0)
                {
                    maze[x, y - 1].topWall = true;
                }
                if (maze[x, y].leftWall && x > 0)
                {
                    maze[x - 1, y].rightWall = true;
                }
                if (maze[x, y].rightWall && x < mazeWidth - 1)
                {
                    maze[x + 1, y].leftWall = true;
                }
            }
        }

    }

    public void PlaceWallProps()
    {
        if (wallProps == null || wallProps.Count == 0) return;

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                MazeCell cell = maze[x, y];

                // Check each wall
                if (cell.leftWall && Random.value < wallPropChance)
                    SpawnWallProp(cell, Vector3.left);

                if (cell.rightWall && Random.value < wallPropChance)
                    SpawnWallProp(cell, Vector3.right);

                if (cell.topWall && Random.value < wallPropChance)
                    SpawnWallProp(cell, Vector3.forward);

                if (cell.bottomWall && Random.value < wallPropChance)
                    SpawnWallProp(cell, Vector3.back);
            }
        }
    }

    private void SpawnWallProp(MazeCell cell, Vector3 direction)
    {
        Vector3 worldPos = new Vector3(cell.x, 0, cell.y); // adjust Y for height
        worldPos += direction * 0.5f;                      // move to wall surface

        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject prefab = wallProps[Random.Range(0, wallProps.Count)];
        GameObject prop = Instantiate(prefab, worldPos + Vector3.up * 1.5f, rotation, propParent);
    }


}



public enum Directions {

    Up,
    Down,
    Left,
    Right

}

public class MazeCell {
    
    public bool visited;        // Check if the cell has been visited in DFS
    public bool roomTile;       // Is this part of the room
    public bool outsideTile;    // Is this a tile around the room
    public int x, y;            // Coordinates

    public Room room;           // Room this cell is part of (if it is)

    public string roomType = "Hallway"; // default roomtype

    /// Wall flags
    public bool topWall;        
    public bool bottomWall;
    public bool leftWall;
    public bool rightWall;

    // Return x and y as a Vector2Int for convenience sake.
    public Vector2Int position {
        get {

            return new Vector2Int(x, y);

        }
    }

    public MazeCell (int x, int y) {

        // The coordinates of this cell in the maze grid.
        this.x = x;
        this.y = y;

        // Whether the algorithm has visited this cell or not - false to start
        visited = false;
        
        // Whether the cell is currently a room or not - false to start
        roomTile = false;

        // All walls are present until the algorithm removes them.
        topWall = leftWall = true;
        
        bottomWall = rightWall = false;

    }
}

[System.Serializable]
public class Room {
    public List<Vector2Int> tilesInRoom;        // Postion of tiles in the room
    public string roomType;                     // The room type
    public Room(string roomType) {
        this.roomType = roomType;
        tilesInRoom = new List<Vector2Int>();
    }

    public void AddTile(Vector2Int tilePosition) {
        tilesInRoom.Add(tilePosition);
    }
}
