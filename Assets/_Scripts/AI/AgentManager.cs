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

        Debug.Log("Map changed - recalculating agent paths");
        
        if (PathCoordinator.Instance != null)
        {
            PathCoordinator.Instance.CoordinateAgentPaths(agents);
            
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
            Debug.Log($"{agent.name}: turnFinished reset to false");
        }
    
        if (PathCoordinator.Instance != null)
        {
            PathCoordinator.Instance.CoordinateAgentPaths(agents);
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