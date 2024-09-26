using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{

    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] GameObject MazeCellPrefab;
    
    // A dictionary to keep track of instantiated room objects
    private Dictionary<string, Transform> instantiatedRooms = new Dictionary<string, Transform>();

    // This the physical size of our maze cells. Getting this wrong will result in overlapping
    // or visible gaps between each cell.
    public float CellSize = 1f;

    private void Start()
    {
        GenerateMaze();
    }

    public void DestroyChildren()
    {
        while (transform.childCount > 0)
        {
            print("Current Children: " + transform.childCount);
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
            print("Current Children: " + transform.childCount);

        }
        
        instantiatedRooms.Clear(); // remove all rooms from dictionary
    }

    public void GenerateMaze()
    {
        if (transform.childCount > 0) return;
        
        while (transform.childCount > 0)
        {
            print("Current Children: " + transform.childCount);
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
            print("Current Children: " + transform.childCount);

        }
        // Get our MazeGenerator script to make us a maze.
        MazeCell[,] maze = mazeGenerator.GetMaze();

        // Loop through every cell in the maze.
        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                var roomType = maze[x, y].roomType;
                
                // Check if a parent for this room type already exists
                if (!instantiatedRooms.ContainsKey(maze[x, y].roomType))
                {
                    // Create a new parent GameObject for this room type
                    var roomParentObject = new GameObject(roomType);
                    roomParentObject.transform.SetParent(transform);

                    // Store the parent in the dictionary
                    instantiatedRooms.Add(roomType, roomParentObject.transform);
                }
                
                // Instantiate a new maze cell prefab as a child of the MazeRenderer object.
                GameObject newCell = Instantiate(MazeCellPrefab,
                    new Vector3((float)x * CellSize, 0f, (float)y * CellSize), Quaternion.identity, instantiatedRooms[roomType]);
                
                newCell.name = roomType + "[" + x + ", " + y + "]";
                
                // Get a reference to the cell's MazeCellPrefab script.
                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();

                // Determine which walls need to be active.
                bool top = maze[x, y].topWall;
                bool left = maze[x, y].leftWall;

                // Bottom and right walls are deactivated by default unless we are at the bottom or right
                // edge of the maze.
                bool right = maze[x, y].rightWall;
                bool bottom = maze[x, y].bottomWall;
                if (x == mazeGenerator.mazeWidth - 1) right = true;
                if (y == 0) bottom = true;
                
                // checks if it is a room tile
                bool roomTile = maze[x, y].roomTile;
                
                // create object
                mazeCell.Init(top, bottom, right, left, roomTile, maze[x, y].roomType);
            }
        }
    }

}