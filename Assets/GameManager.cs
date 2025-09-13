using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using UnityEngine;
using Yarn.Unity;


/// <summary>
/// This class seems to be responsible for:
/// - Keeping track if AI's have reached their goal room.
/// - Interface for AgentNavigation, other scripts, to access the maze. 
/// - Manages ALL win/lose conditions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public MazeCell[,] centralMaze; // Central maze representation
    [SerializeField] private MazeRenderer mazeRenderer;
    [SerializeField] private MazeGenerator mazeGenerator;
    [SerializeField] private WinScript winScript;
    [SerializeField] private LoseScript loseScript;
    [SerializeField] private ClickAI clickAI;
    private Dictionary<AgentNavigation, bool> agentGoalRoom = new Dictionary<AgentNavigation, bool>();
    public float CellSize;
    bool won = false;
    public CinemachineVirtualCamera playerCam;
    public GameObject endTurnButton; // Needs to be destroyed seperately for win/lose

    DialogueRunner dialogueRunner;


    // Editable variables.
    [Header("The amount of culminated fear needed to win game")]
    public int winFear = 100;
    /// Number of rounds until defeat.
    [Header("Number of rounds until player defeat.")]
    public int roundsUntilDefeat = 10;

    private void Awake()
    {
        Instance = this;

        if (mazeRenderer != null)
        {
            // subscribe to event
            mazeRenderer.OnMazeGenerated += OnMazeGenerated;
        }
        else
        {
            Debug.LogError("MazeRenderer instance is null.");
        }

        if (!playerCam) { playerCam = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CinemachineVirtualCamera>(); }
    }

    private void Start()
    {
        AgentNavigation[] agents = FindObjectsOfType<AgentNavigation>();
        foreach (AgentNavigation agent in agents)
        {
            agentGoalRoom[agent] = false;
        }
        dialogueRunner = FindObjectOfType<DialogueRunner>();
    }

    #region Win and Lose condition checks.
    public void CHECK_FEAR_WIN(int totalFear)
    {
        if (totalFear >= winFear)
        {
            Win("Successful summon");
        }
    }

    /// <summary>
    /// Check if player has reached the ending round. BEING CALLED IN UNITY EVENT
    /// </summary>
    public void CHECK_ROUND_LOSE()
    {
        if(TurnBasedSystem.GET_TURN_COUNT() == roundsUntilDefeat && TurnBasedSystem.IsPlayerTurn())
        {
            // Gameover
            Lose("Summon opening missed");
        }
    }

    
    private bool CHECK_GOAL_ROOM_LOSE()
    {
        // check if all agents foundGoal bool is true
        // PARAM => WHAT IS RETURNED
        return agentGoalRoom.Values.All(foundGoal => foundGoal);
    }



    public bool CheckAgentGoal(AgentNavigation ag)
    {
        if (agentGoalRoom[ag] == true)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Initiate a game WIN
    /// </summary>
    public void Win(string reason)
    {
        won = true;
        foreach (var agent in agentGoalRoom.Keys)
        {
            agent.StopMovement();
        }

        SkillsCollection.S.DISABLE_BUTTONS();
        Destroy(endTurnButton);
        clickAI.EndConversation();
        Destroy(clickAI.gameObject);
        winScript.Win(reason);
    }

    /// <summary>
    /// Initiate a game LOSE
    /// </summary>
    public void Lose(string reason)
    {
        if (won) return;
        foreach (var agent in agentGoalRoom.Keys)
        {
            agent.StopMovement();
        }
        SkillsCollection.S.DISABLE_BUTTONS();
        Destroy(endTurnButton);
        clickAI.EndConversation();
        Destroy(clickAI.gameObject);
        loseScript.Lose(reason);
    }
    #endregion

    private void OnMazeGenerated()
    {
        CellSize = mazeRenderer.CellSize;
        centralMaze = mazeRenderer.GetMaze();
    }

    /// <summary>
    /// Returns current list of rooms.
    /// </summary>
    public List<Room> GetRooms()
    {
        return mazeGenerator.rooms;
    }

    public MazeCell[,] GetCentralMaze()
    {
        return centralMaze;
    }

    public int GetWinFear()
    {
        return winFear;
    }
    public int GetRoundsTillDefeat()
    {
        return roundsUntilDefeat;
    }


    public void SetGoalBool(AgentNavigation agent, bool foundRoom)
    {
        if (agentGoalRoom.ContainsKey(agent))
        {
            agentGoalRoom[agent] = foundRoom;

            //PLACEHOLDER: Activates the key game object of an agent
            agent.GetComponent<keyTracker>().Key.SetActive(true);
            //PLACEHOLDER: Plays a yarn spinner script
            DialogueBank DB = agent.GetComponent<DialogueBank>();
            DB.FindKey();
            

        }
    }



    /// <summary>
    /// Debug function for displaying if AI's have found their goal room.
    /// </summary>
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



    private void LateUpdate()
    {
        DisplayAgentsGoalStatus();
        if (CHECK_GOAL_ROOM_LOSE())
            Lose("Humans conquered fear");
    }
}
