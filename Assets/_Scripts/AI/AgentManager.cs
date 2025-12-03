using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager Instance;
    
    public AgentNavigation[] agents;
    
    [SerializeField] private float turnTimeout = 60f;
    private float turnStartTime;
    private bool agentTurnInProgress;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (agents.Length == 0)
        {
            agents = FindObjectsOfType<AgentNavigation>();
        }
        if (LoadManager.S.loadData)
        {
            // Grab the positions of all agents.
            PlayerData pd = LoadManager.S.playerDataToLoad;
            Vector3 hp = new Vector3(pd.haroldPos[0], pd.haroldPos[1], pd.haroldPos[2]);
            Vector3 ip = new Vector3(pd.iainPos[0], pd.iainPos[1], pd.iainPos[2]);
            Vector3 mp = new Vector3(pd.mackenziePos[0], pd.mackenziePos[1], pd.mackenziePos[2]);
            Vector3 ep = new Vector3(pd.evelynPos[0], pd.evelynPos[1], pd.evelynPos[2]);
            agents[0].transform.position = hp;
            agents[1].transform.position = ip;
            agents[2].transform.position = mp;
            agents[3].transform.position = ep;

            // Grab fear amounts
            agents[0].agentFear.currentFear = pd.haroldFear;
            agents[1].agentFear.currentFear = pd.iainFear;
            agents[2].agentFear.currentFear = pd.mackenzieFear;
            agents[3].agentFear.currentFear = pd.evelynFear;

            // Grab visited rooms
            agents[0].visitedRooms.AddRange(pd.haroldRooms);
            agents[1].visitedRooms.AddRange(pd.iainRooms);
            agents[2].visitedRooms.AddRange(pd.mackenzieRooms);
            agents[3].visitedRooms.AddRange(pd.evelynRooms);

        }
    }
    
    private void OnEnable()
    {
        MapChangeEvents.OnMapChanged += HandleMapChanged;
    }

    private void OnDisable()
    {
        MapChangeEvents.OnMapChanged -= HandleMapChanged;
    }

    private void OnDestroy()
    {
        MapChangeEvents.OnMapChanged -= HandleMapChanged;
    }

    private void HandleMapChanged()
    {
        if (!TurnBasedSystem.IsPlayerTurn())
        {
            return;
        }

        Debug.Log("Map changed - invalidating agent paths");
    
        if (PathCoordinator.Instance != null)
        {
            PathCoordinator.Instance.InvalidatePlannedPaths();
        
            // If preview is visible, refresh it (which will recalculate automatically)
            if (PathVisualizer.Instance != null && PathVisualizer.Instance.ArePathsVisible())
            {
                PathVisualizer.Instance.RefreshPaths();
            }
        }
        else
        {
            Debug.LogError("PathCoordinator not found during map change!");
        }
    }
    
    private void Update()
    {
        if (agentTurnInProgress)
        {
            if (Time.time - turnStartTime > turnTimeout)
            {
                Debug.LogWarning("Agent turn timeout - forcing turn end");
                ForceEndAgentTurn();
            }
        }
    }
    
    public void AgentCall()
    {
        if (!CheckAgents()) 
        {
            Debug.Log("Not all agents finished yet");
            return;
        }
    
        Debug.Log("All agents finished - starting player turn");
        agentTurnInProgress = false;
    
        if (LureManager.Instance != null)
        {
            LureManager.Instance.DecreaseLureDuration();
        }
    
        if (PathCoordinator.Instance != null)
        {
            PathCoordinator.Instance.ClearAllReservations();
            Debug.Log("Cleared agent paths after turn completion");
        }
    
        TurnBasedSystem.StartPlayerTurn();
    }
    
    public void StartAgentTurn()
    {
        Debug.Log("Starting agent turns");
        agentTurnInProgress = true;
        turnStartTime = Time.time;

        if (PathVisualizer.Instance != null && PathVisualizer.Instance.ArePathsVisible())
        {
            Debug.Log("Hiding path visualization as agent turn starts");
            PathVisualizer.Instance.HidePaths();
        }

        foreach (var agent in agents)
        {
            agent.turnFinished = false;
            agent.ResetTurnMovement();
            agent.ReserveCurrentPosition();
        
            Debug.Log($"{agent.name}: turnFinished reset to false");
        }

        if (PathCoordinator.Instance != null)
        {
            if (!PathCoordinator.Instance.HasValidPlannedPaths())
            {
                Debug.Log("No valid paths exist - calculating now");
                PathCoordinator.Instance.CoordinateAgentPaths(agents, forceRecalculate: true);
            }
            else
            {
                Debug.Log("Using pre-calculated paths from player turn");
            }
        
            PathCoordinator.Instance.ApplyPlannedPathsToAgents(agents);
        }
        else
        {
            Debug.LogError("PathCoordinator not found!");
        }
    }
    
    private bool CheckAgents()
    {
        bool allFinished = agents.All(agent => agent.turnFinished);
        if (!allFinished)
        {
            var unfinishedAgents = agents.Where(agent => !agent.turnFinished).Select(agent => agent.name);
            Debug.Log($"Waiting for agents: {string.Join(", ", unfinishedAgents)}");
        }
        return allFinished;
    }
    
    private void ForceEndAgentTurn()
    {
        Debug.LogWarning("Forcing all agents to end their turns");
        foreach (var agent in agents)
        {
            if (!agent.turnFinished)
            {
                Debug.LogWarning($"Force-ending turn for {agent.name}");
                agent.turnFinished = true;
                agent.StopMovement();
            }
        }
        
        agentTurnInProgress = false;
        TurnBasedSystem.StartPlayerTurn();
    }
}