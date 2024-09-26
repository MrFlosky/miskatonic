using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour {

    [Range(5, 500)]
    public int mazeWidth = 5, mazeHeight = 5;           // The dimensions of the maze.
    public int startX, startY;                          // The position our algorithm will start from.
    MazeCell[,] maze;                                   // An array of maze cells representing the maze grid.

    [Range(0, 10)] public int roomCount = 1;
    [Range(2,20)] public int roomXSizeMin, roomXSizeMax, roomYSizeMin, roomYSizeMax = 5;
    Vector2Int currentCell;                             // The maze cell we are currently looking at.

    private List<string> _roomTypes;                     // all the room types
    
    public MazeCell[,] GetMaze() 
    {
        
        maze = new MazeCell[mazeWidth, mazeHeight];

        for (int x = 0; x < mazeWidth; x++) {
            for (int y = 0; y < mazeHeight; y++) {

                maze[x, y] = new MazeCell(x, y);

            }
        }

        // Start carving our maze path.
        CarvePath(startX, startY);
        
        // create all roomtypes
        _roomTypes = new List<string> {
            "Dining Hall",
            "Library",
            "Ballroom",
            "Study",
            "Lounge",
            "Kitchen",
            "Conservatory",
            "Billiard Room",
            "Hall",
            "Observatory",
            "Cellar",
            "Music Room",
            "Trophy Room",
            "Garden",
            "Parlor"
        };
        
        for (int i = 0; i < roomCount; i++)
        {
            CreateRoom(maze);
        }
        
        return maze;

    }

    List<Direction> directions = new List<Direction> {
 
        Direction.Up, Direction.Down, Direction.Left, Direction.Right,
    
    };

    List<Direction> GetRandomDirections() {

        // Make a copy of our directions list that we can mess around with.
        List<Direction> dir = new List<Direction>(directions);

        // Make a directions list to put our randomised directions into.
        List<Direction> rndDir = new List<Direction>();

        while (dir.Count > 0) {                         // Loop until our rndDir list is empty.

            int rnd = Random.Range(0, dir.Count);       // Get random index in list.
            rndDir.Add(dir[rnd]);                       // Add the random direction to our list.
            dir.RemoveAt(rnd);                          // Remove that direction so we can't choose it again

        }

        // When we've got all four directions in a random order, return the queue.
        return rndDir;

    }

    bool IsCellValid (int x, int y) {

        // If the cell is outside of the map or has already been visited, we consider it not valid.
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1 || maze[x, y].visited) return false;
        else return true;

    }
    
    bool IsRoomValid (int x, int y) {
    
            // If the cell is outside of the map or has already been visited, we consider it not valid.
            if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1 || maze[x, y].roomTile || maze[x, y].outsideTile) return false;
            else return true;
    
        }

    Vector2Int CheckNeighbours() {

        List<Direction> rndDir = GetRandomDirections();

        for (int i = 0; i < rndDir.Count; i++) {

            // Set neighbour coordinates to current cell for now.
            Vector2Int neighbour = currentCell;

            // Modify neighbour coordinates based on the random directions we're currently trying.
            switch (rndDir[i]) {

                case Direction.Up:
                    neighbour.y++;
                    break;
                case Direction.Down:
                    neighbour.y--;
                    break;
                case Direction.Right:
                    neighbour.x++;
                    break;
                case Direction.Left:
                    neighbour.x--;
                    break;
            }

            // If the neighbour we just tried is valid, we can return that neighbour. If not, we go again.
            if (IsCellValid(neighbour.x, neighbour.y)) return neighbour;

        }

        // If we tried all directions and didn't find a valid neighbour, we return the currentCell values.
        return currentCell;
    }

    // Takes in two maze positions and sets the cells accordingly.
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

    // Starting at the x, y passed in, carves a path through the maze until it encounters a "dead end"
    // (a dead end is a cell with no valid neighbours).   
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
    
    public void CreateRoom(MazeCell[,] maze)
    {
        int xLength = Random.Range(roomXSizeMin, roomXSizeMax); // get random x size of room
        int yLength = Random.Range(roomYSizeMin, roomYSizeMax); // get random y size of room

        int startX = Random.Range(Mathf.RoundToInt(xLength / 2), mazeWidth - Mathf.RoundToInt(xLength / 2));
        int startY = Random.Range(Mathf.RoundToInt(yLength / 2), mazeHeight - Mathf.RoundToInt(yLength / 2));

        int farLeftX = startX - Mathf.RoundToInt(xLength / 2);
        int farRightX = startX + Mathf.RoundToInt(xLength / 2) - 1;

        int farBottomY = startY - Mathf.RoundToInt(yLength / 2);
        int farTopY = startY + Mathf.RoundToInt(yLength / 2) - 1;

        bool success = true;
        int tries = 0;
        while (tries < 10)
        {
            success = true;
            for (int x = farLeftX; x < farRightX + 1; x++)
            {
                for (int y = farBottomY; y < farTopY + 1; y++)
                {
                    if (maze[x, y].roomTile || maze[x, y].outsideTile)
                    {
                        print("womp womp");
                        success = false;
                    }
                }
            }
            if (success) break;
            
            xLength = Random.Range(roomXSizeMin, roomXSizeMax); // get random x size of room
            yLength = Random.Range(roomYSizeMin, roomYSizeMax); // get random y size of room

            startX = Random.Range(Mathf.RoundToInt(xLength / 2), mazeWidth - Mathf.RoundToInt(xLength / 2));
            startY = Random.Range(Mathf.RoundToInt(yLength / 2), mazeHeight - Mathf.RoundToInt(yLength / 2));

            farLeftX = startX - Mathf.RoundToInt(xLength / 2);
            farRightX = startX + Mathf.RoundToInt(xLength / 2) - 1;

            farBottomY = startY - Mathf.RoundToInt(yLength / 2);
            farTopY = startY + Mathf.RoundToInt(yLength / 2) - 1;
            
            tries++;
        }
        if (!success) return;
        
        // set all tiles within random range and 1 out as outsideTiles
        // loop through x,y in random range
        for (int x = farLeftX - 1; x < farRightX + 2; x++)
        {
            for (int y = farBottomY - 1; y < farTopY + 2; y++)
            {
                // if this cant become a new room, move onto next
                if (!IsRoomValid(x, y)) continue;
                // set this tile as an outside tile
                maze[x,y].outsideTile = true;
            }
            
        }
        
        
        var rnd = Random.Range(0, _roomTypes.Count);       // Get random room type index in list.
        
        // loop through x,y in random range
        for (int x = farLeftX; x < farRightX + 1; x++)
        {
            for (int y = farBottomY; y < farTopY + 1; y++)
            {
                var selectedCell = maze[x, y];
                // Clear all walls first
                //if (!IsCellValid(x, y)) continue;

                selectedCell.rightWall = selectedCell.leftWall = selectedCell.topWall = selectedCell.bottomWall = false;
                
                // Corrected conditions for determining far edges:
                if (x == farLeftX) // if far left
                {
                    selectedCell.leftWall = true;
                    //print(x + " is the far left");
                }
                if (x == farRightX) // if far right
                {
                    selectedCell.rightWall = true;
                    //print(x + " is the far right");
                }
                if (y == farTopY) // if top
                {
                    selectedCell.topWall = true;
                    //print(y + " is the far top");
                }
                if (y == farBottomY) // if bottom
                {
                    selectedCell.bottomWall = true;
                    //print(y + " is the far bottom");
                }
                maze[x,y].roomTile = true;
                
                maze[x, y].roomType = _roomTypes[rnd];             // Add the room title
            }
        }
        
        _roomTypes.RemoveAt(rnd);   // after setting all the tiles, destroy 
    }
    
}

public enum Direction {

    Up,
    Down,
    Left,
    Right

}

public class MazeCell {

    public bool visited;
    public bool roomTile;
    public bool outsideTile;
    public int x, y;

    public string roomType = "Hallway"; // default roomtype

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
