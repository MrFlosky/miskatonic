using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public MazeCell[,] centralMaze; // Central maze representation
    public List<Room> rooms = new List<Room>(); // List of rooms

    [SerializeField] private MazeRenderer mazeRenderer;
    [SerializeField] private MazeGenerator mazeGenerator;
    
    [SerializeField] private WinScript winScript;
    [SerializeField] private LoseScript loseScript;

    [SerializeField] private ClickAI clickAI;
    
    private Dictionary<AgentNavigation, bool> agentGoalRoom = new Dictionary<AgentNavigation, bool>();

    public float CellSize;
    
    bool won = false;

    public CinemachineVirtualCamera playerCam;

    private void Awake()
    {
        Instance = this;

        if (mazeRenderer != null)
        {
            mazeRenderer.OnMazeGenerated += OnMazeGenerated;
        }
        else
        {
            Debug.LogError("MazeRenderer instance is null.");
        }
        
        if (!playerCam) {playerCam = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CinemachineVirtualCamera>();}
    }

    private void Start()
    {
        AgentNavigation[] agents = FindObjectsOfType<AgentNavigation>();
        foreach (AgentNavigation agent in agents)
        {
            agentGoalRoom[agent] = false;
        }
    }

    public bool CheckAgentGoal(AgentNavigation ag)
    {
        if(agentGoalRoom[ag] == true)
        {
            return true;
        }
        return false;
    }

    private void OnMazeGenerated()
    {
        CellSize = mazeRenderer.CellSize;
        centralMaze = mazeRenderer.GetMaze();

        rooms = mazeGenerator.rooms;
    }

    public List<Room> GetRooms()
    {
        return rooms;
    }

    public void SwapRooms(GameObject room1Ob, string rm1Type, GameObject room2Ob, string rm2Type)
    {
        // Swap the room positions in the scene
        Vector3 room1Position = room1Ob.transform.position;
        Vector3 room2Position = room2Ob.transform.position;

        // Swap the positions of the room GameObjects
        room1Ob.transform.position = room2Position;
        room2Ob.transform.position = room1Position;

        // Swap room types for the cells in the centralMaze
        foreach (MazeCellObject cell in room1Ob.GetComponentsInChildren<MazeCellObject>())
        {
            Vector2Int cellPosition = new Vector2Int(cell.GetXCoord(), cell.GetYCoord());
            centralMaze[cellPosition.x, cellPosition.y].roomType = rm2Type;
        }

        foreach (MazeCellObject cell in room2Ob.GetComponentsInChildren<MazeCellObject>())
        {
            Vector2Int cellPosition = new Vector2Int(cell.GetXCoord(), cell.GetYCoord());
            centralMaze[cellPosition.x, cellPosition.y].roomType = rm1Type;
        }

        // Swap tiles between rooms while maintaining corresponding positions
        SwapRoomTiles(room1Ob, room2Ob);

        // Swap props associated with the rooms
        //SwapRoomProps(room1Ob, room2Ob);

        // Notify all AI agents to update their maze information
        UpdateAIMaze();

        // Update the room data in the GameManager's central maze and room list
        UpdateRoomData();
    }

    private void SwapRoomTiles(GameObject room1Ob, GameObject room2Ob)
    {
        var room1Type = room1Ob.GetComponentsInChildren<MazeCellObject>()[0].roomType;
        var room2Type = room2Ob.GetComponentsInChildren<MazeCellObject>()[0].roomType;

        // Organize the tiles into a 5x5 grid based on their local positions
        foreach (MazeCellObject cell in room1Ob.GetComponentsInChildren<MazeCellObject>())
        {
            cell.roomType = room2Type;
        }

        foreach (MazeCellObject cell in room2Ob.GetComponentsInChildren<MazeCellObject>())
        {
            cell.roomType = room1Type;
        }
        
        SwapRoomProps(room1Ob, room2Ob);
        
        // Notify all AI agents to update their maze information
        UpdateAIMaze();

        // Update the room data in the GameManager's central maze and room list
        UpdateRoomData();
    }

    private void UpdateAIMaze()
    {
        // Find all AgentNavigation instances and update their maze references
        AgentNavigation[] agents = FindObjectsOfType<AgentNavigation>();
        foreach (AgentNavigation agent in agents)
        {
            // Notify the agent that the maze has been updated
            agent.UpdateMaze(centralMaze);
        }
    }

    private void SwapRoomProps(GameObject room1Ob, GameObject room2Ob)
    {
        // Find the props in the rooms based on the "Prop" tag
        GameObject room1Prop = FindPropWithTag(room1Ob, "Prop");
        GameObject room2Prop = FindPropWithTag(room2Ob, "Prop");

        // Ensure the props and parents are valid
        if (room1Prop != null && room2Prop != null)
        {
            Transform room1Parent = room1Ob.transform;
            Transform room2Parent = room2Ob.transform;

            // Store the local positions before swapping
            Vector3 room1LocalPosition = room1Prop.transform.localPosition;
            Vector3 room2LocalPosition = room2Prop.transform.localPosition;

            // Swap the props between rooms
            room1Prop.transform.SetParent(room2Parent);
            room1Prop.transform.localPosition = room2LocalPosition;

            room2Prop.transform.SetParent(room1Parent);
            room2Prop.transform.localPosition = room1LocalPosition;
        }
    }

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

    private void UpdateRoomData()
    {
        rooms = mazeGenerator.rooms;
    }
    
  
    public void SetGoalBool(AgentNavigation agent, bool foundRoom)
    {
        if (agentGoalRoom.ContainsKey(agent))
        {
            agentGoalRoom[agent] = foundRoom;
        }
    }

    private bool AllAgentsFoundGoal()
    {
        return agentGoalRoom.Values.All(foundGoal => foundGoal);
    }

    private void DisplayAgentsGoalStatus()
    {
        foreach (var entry in agentGoalRoom)
        {
            AgentNavigation agent = entry.Key;
            bool foundGoal = entry.Value;

            // Display each agent's name and goal status
            //Debug.Log($"{agent.gameObject.name} has found goal room: {foundGoal}");
        }
    }

    public void Win(string reason)
    {
        won = true;
        foreach (var agent in agentGoalRoom.Keys)
        {
            agent.StopMovement();
        }

        clickAI.EndConversation();
        Destroy(clickAI.gameObject);
        winScript.Win(reason);
    }
    
    public GameObject endTurnButton;
    public void Lose(string reason)
    {
        if (won) return;
        foreach (var agent in agentGoalRoom.Keys)
        {
            agent.StopMovement();
        }
        SkillsCollection.DISABLE_BUTTONS();
        Destroy(endTurnButton);
        clickAI.EndConversation();
        Destroy(clickAI.gameObject);
        loseScript.Lose(reason);
    }
    
    private void LateUpdate()
    {
        DisplayAgentsGoalStatus();
        if (AllAgentsFoundGoal())
            Lose("Humans conquered fear");
    }
}
