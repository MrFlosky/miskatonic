using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Cinemachine;
using System;

public class MazeRenderer : MonoBehaviour
{
    public static MazeRenderer Instance;
    
    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] GameObject MazeCellPrefab;
    
    // List to store the pairing of room types and their prefabs
    [SerializeField] private List<RoomPrefabPair> roomPrefabPairs;
    
    // A dictionary to keep track of instantiated room objects
    public Dictionary<string, Transform> instantiatedRooms = new Dictionary<string, Transform>();
    
    public Dictionary<Vector2Int, GameObject> cellGameObjects = new Dictionary<Vector2Int, GameObject>();

    // This the physical size of our maze cells. Getting this wrong will result in overlapping
    // or visible gaps between each cell.
    public float CellSize = 1f;
    
    private MazeCell[,] maze;

    public CinemachineVirtualCamera roomVirtualCamera;

    // Event to notify maze completion
    public event System.Action OnMazeGenerated;

    public Vector3 cameraOffsets;
    public Quaternion rotationCameraOffsets;

    public CinemachineVirtualCamera bedroomCam;
    public CinemachineVirtualCamera cemeteryCam;
    public CinemachineVirtualCamera diningCam;
    public CinemachineVirtualCamera libraryCam;
    public CinemachineVirtualCamera dungeonCam;
    public CinemachineVirtualCamera ritualCam;

    private void Start()
    {
        Instance = this;
        GenerateMaze();
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public MazeCell[,] GetMaze()
    {
        return maze;
    }

    public void DestroyChildren()
    {
        while (transform.childCount > 0)
        {
            //print("Current Children: " + transform.childCount);
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
            //print("Current Children: " + transform.childCount);

        }
        instantiatedRooms.Clear(); // remove all rooms from dictionary
        mazeGenerator.rooms.Clear();
    }

    public void GenerateMaze()
    {
        // Clear existing maze if any
        if (transform.childCount > 0) DestroyChildren();

        // Generate a fully traversable maze using GenerateValidMaze
        maze = mazeGenerator.GenerateValidMaze();

        // If the maze generation fails, stop the execution
        if (maze == null)
        {
            Debug.LogError("Failed to generate a fully traversable maze.");
            return;
        }

        // Loop through every cell in the maze
        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                var roomType = maze[x, y].roomType;

                // Check if a parent for this room type already exists
                if (!instantiatedRooms.ContainsKey(roomType))
                {
                    // Create a new parent GameObject for this room type
                    var roomParentObject = new GameObject(roomType);
                    roomParentObject.transform.SetParent(transform);

                    // Store the parent in the dictionary
                    instantiatedRooms.Add(roomType, roomParentObject.transform);
                }

                // Instantiate a new maze cell prefab as a child of the MazeRenderer object
                GameObject newCell = Instantiate(MazeCellPrefab,
                    new Vector3((float)x * CellSize, 0f, (float)y * CellSize), Quaternion.identity, instantiatedRooms[roomType]);

                newCell.name = roomType + "[" + x + ", " + y + "]";

                // Get a reference to the cell's MazeCellObject script
                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();

                // Have it hold its x,y coordinates
                mazeCell.SetXCoord(x);
                mazeCell.SetYCoord(y);
                
                Vector2Int cellPosition = new Vector2Int(x, y);
                cellGameObjects[cellPosition] = newCell;

                // Determine which walls need to be active
                bool top = maze[x, y].topWall;
                bool left = maze[x, y].leftWall;

                // Bottom and right walls are deactivated by default unless we are at the bottom or right edge of the maze
                bool right = maze[x, y].rightWall;
                bool bottom = maze[x, y].bottomWall;
                if (x == mazeGenerator.mazeWidth - 1) right = true;
                if (y == 0) bottom = true;

                // Check if it is a room tile
                bool roomTile = maze[x, y].roomTile;

                // Initialize the cell with wall and room information
                mazeCell.Init(top, bottom, right, left, roomTile, maze[x, y].roomType);
                
                // If the current cell is a room tile, check if it is the top-left corner of a room
                if (roomTile && IsTopLeftCorner(x, y))
                {
                    // Find the prefab for the current room type
                    GameObject cornerPrefab = GetPrefabForRoomType(roomType);
                    if (cornerPrefab != null)
                    {
                        // Instantiate the corner prefab at this location
                        Instantiate(cornerPrefab, new Vector3(x * CellSize, 0f, y * CellSize), Quaternion.identity, instantiatedRooms[roomType]);

                        //Adding room Cameras
                        switch(roomType)
                        {
                            case ("Bedroom"):
                                bedroomCam = Instantiate(roomVirtualCamera, new Vector3(x * CellSize + cameraOffsets.x, 0f + cameraOffsets.y, y * CellSize + cameraOffsets.z),
                                new Quaternion(0 + rotationCameraOffsets.x, 0 + rotationCameraOffsets.y, 0 + rotationCameraOffsets.z, 0 + rotationCameraOffsets.w), 
                                instantiatedRooms[roomType]);                                
                                break;
                            case ("Library"):
                                libraryCam = Instantiate(roomVirtualCamera, new Vector3(x * CellSize + cameraOffsets.x, 0f + cameraOffsets.y, y * CellSize + cameraOffsets.z),
                                new Quaternion(0 + rotationCameraOffsets.x, 0 + rotationCameraOffsets.y, 0 + rotationCameraOffsets.z, 0 + rotationCameraOffsets.w),
                                instantiatedRooms[roomType]);
                                break;
                            case ("Ritual Room"):
                                ritualCam = Instantiate(roomVirtualCamera, new Vector3(x * CellSize + cameraOffsets.x, 0f + cameraOffsets.y, y * CellSize + cameraOffsets.z),
                                new Quaternion(0 + rotationCameraOffsets.x, 0 + rotationCameraOffsets.y, 0 + rotationCameraOffsets.z, 0 + rotationCameraOffsets.w),
                                instantiatedRooms[roomType]);
                                break;
                            case ("Dungeon"):
                                dungeonCam = Instantiate(roomVirtualCamera, new Vector3(x * CellSize + cameraOffsets.x, 0f + cameraOffsets.y, y * CellSize + cameraOffsets.z),
                                new Quaternion(0 + rotationCameraOffsets.x, 0 + rotationCameraOffsets.y, 0 + rotationCameraOffsets.z, 0 + rotationCameraOffsets.w),
                                instantiatedRooms[roomType]);
                                break;
                            case ("Dining Room"):
                                diningCam = Instantiate(roomVirtualCamera, new Vector3(x * CellSize + cameraOffsets.x, 0f + cameraOffsets.y, y * CellSize + cameraOffsets.z),
                                new Quaternion(0 + rotationCameraOffsets.x, 0 + rotationCameraOffsets.y, 0 + rotationCameraOffsets.z, 0 + rotationCameraOffsets.w),
                                instantiatedRooms[roomType]);
                                break;
                            case ("Cemetery"):
                                cemeteryCam = Instantiate(roomVirtualCamera, new Vector3(x * CellSize + cameraOffsets.x, 0f + cameraOffsets.y, y * CellSize + cameraOffsets.z),
                                new Quaternion(0 + rotationCameraOffsets.x, 0 + rotationCameraOffsets.y, 0 + rotationCameraOffsets.z, 0 + rotationCameraOffsets.w),
                                instantiatedRooms[roomType]);
                                break;                            
                        }
                                             
                    }
                }
            }
        }

        // Invoke the maze generation completion event
        OnMazeGenerated?.Invoke();
    }
    
    // Check if a tile is the top-left corner of a room
    private bool IsTopLeftCorner(int x, int y)
    {
        // Check if the current cell is a room tile and the top-left neighboring cells are not
        bool isRoomTile = maze[x, y].roomTile;
        bool leftTileNotRoom = (x == 0 || !maze[x - 1, y].roomTile);
        bool topTileNotRoom = (y == mazeGenerator.mazeHeight - 1 || !maze[x, y + 1].roomTile);

        return isRoomTile && leftTileNotRoom && topTileNotRoom;
    }
    
    // Get the prefab for a specific room type from the list of RoomPrefabPairs
    private GameObject GetPrefabForRoomType(string roomType)
    {
        foreach (RoomPrefabPair pair in roomPrefabPairs)
        {
            if (pair.roomType == roomType)
            {
                return pair.prefab;
            }
        }
        return null; // Return null if no prefab is found for the room type
    }
    
    [System.Serializable]
    public class RoomPrefabPair
    {
        public string roomType;
        public GameObject prefab;
    }
}