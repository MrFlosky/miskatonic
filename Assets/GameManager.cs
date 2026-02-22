using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


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
    }

    #region Win and Lose condition checks.
    public void CHECK_FEAR_WIN(int totalFear)
    {
        if (totalFear >= winFear)
        {
            Win("Successful summon");
            TelemetryManager.S.Won();
        }
    }

    /// <summary>
    /// Check if player has reached the ending round. BEING CALLED IN UNITY EVENT
    /// </summary>
    public void CHECK_ROUND_LOSE()
    {
        if(TurnBasedSystem.GET_TURN_COUNT() == roundsUntilDefeat && TurnBasedSystem.IsPlayerTurn())
        {
            Lose("Summon opening missed");
            TelemetryManager.S.Lost(true);
        }
    }

    /// <summary>
    /// Check if all agents are in the ritual room with their keys
    /// </summary>
    private bool CHECK_RITUAL_ROOM_LOSE()
    {
        bool allHaveKeys = agentGoalRoom.Values.All(foundGoal => foundGoal);
        
        if (!allHaveKeys)
        {
            return false;
        }
        
        int agentsInRitualRoom = 0;
        int totalAgents = agentGoalRoom.Count;
        
        foreach (var agent in agentGoalRoom.Keys)
        {
            if (IsAgentInRitualRoom(agent))
            {
                agentsInRitualRoom++;
            }
        }
        
        if (agentsInRitualRoom == totalAgents && totalAgents > 0)
        {
            Debug.Log($"All {totalAgents} agents are in Ritual Room with their keys!");
            return true;
        }
        
        if (agentsInRitualRoom > 0)
        {
            Debug.Log($"Ritual Room Progress: {agentsInRitualRoom}/{totalAgents} agents present (all have keys)");
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if a specific agent is currently in the Ritual Room
    /// </summary>
    private bool IsAgentInRitualRoom(AgentNavigation agent)
    {
        if (agent == null || centralMaze == null)
        {
            return false;
        }
        
        Vector2Int agentPos = agent.WorldToGridPosition(agent.transform.position);
        
        if (agentPos.x < 0 || agentPos.x >= centralMaze.GetLength(0) ||
            agentPos.y < 0 || agentPos.y >= centralMaze.GetLength(1))
        {
            return false;
        }
    
        MazeCell currentCell = centralMaze[agentPos.x, agentPos.y];
        
        if (currentCell.roomTile && currentCell.roomType == "Ritual Room")
        {
            return IsStandingSpot(agentPos, "Ritual Room");
        }
    
        return false;
    }
    
    private bool IsStandingSpot(Vector2Int position, string roomType)
    {
        List<Room> allRooms = GetRooms();
    
        foreach (Room room in allRooms)
        {
            if (room.roomType == roomType)
            {
                if (room.standingSpots != null && room.standingSpots.Contains(position))
                {
                    return true;
                }
                break;
            }
        }
    
        return false;
    }
    
    /// <summary>
    /// Get count of agents currently in ritual room
    /// </summary>
    public int GetAgentsInRitualRoomCount()
    {
        int count = 0;
        foreach (var agent in agentGoalRoom.Keys)
        {
            if (IsAgentInRitualRoom(agent))
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Get count of agents with keys (for UI/debugging)
    /// </summary>
    public int GetAgentsWithKeysCount()
    {
        return agentGoalRoom.Values.Count(hasKey => hasKey);
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
    /// Whether or not we are in demo mode,
    /// which gives a specific lose message.
    /// </summary>
    public bool demo;

    /// <summary>
    /// Text for demo thanks
    /// </summary>
    public TMP_Text demoThankYouTxt;  

    private IEnumerator FadeDemoText(float waitSecs)
    {
        yield return new WaitForSeconds(waitSecs); 
        float duration = 1f;
        Color c = demoThankYouTxt.color;
        c.a = 0f;
        demoThankYouTxt.color = c;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            demoThankYouTxt.color = c;
            yield return null;
        }

        c.a = 1f;
        demoThankYouTxt.color = c;
    }

    /// <summary>
    /// Initiate a game WIN
    /// </summary>
    public void Win(string reason)
    {
        if (won) return; // prevent re entry, causing stack overflow
        won = true;
        foreach (var agent in agentGoalRoom.Keys)
        {
            agent.StopMovement();
        }

        SkillsCollection.S.DISABLE_BUTTONS();
        Destroy(endTurnButton);
        clickAI.EndConversation();
        Destroy(clickAI.gameObject);

        if(demo)
        {
            demoThankYouTxt.text = "Thank you for playing! Look forward to full release in Summer 2026.";
            StartCoroutine(FadeDemoText(9f));
        } 
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

        if(demo)
        {
            loseScript.Lose("Thank you for playing! Look forward to full release in Summer 2026.");
        } 
        else
        {
            loseScript.Lose(reason);
        }
        
        
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
        
        if (CHECK_RITUAL_ROOM_LOSE())
        {
            Lose("Ritual completed - humans conquered fear");
            TelemetryManager.S.Lost(false);
        }
    }
}