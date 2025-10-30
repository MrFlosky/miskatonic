using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager Instance;
    
    public AgentNavigation[] agents;
    
    [SerializeField] private float turnTimeout = 60f; // Maximum time for all agents to complete turns
    private float turnStartTime;
    private bool agentTurnInProgress = false;

    private void Start()
    {
        Instance = this;
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
    
    private void Update()
    {
        // Check for stuck agents during their turn
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
        
        // All agents finished, start player turn
        Debug.Log("All agents finished - starting player turn");
        agentTurnInProgress = false;
        TurnBasedSystem.StartPlayerTurn();
    }
    
    public void StartAgentTurn()
    {
        // Called when player turn ends and agent turns begin
        Debug.Log("Starting agent turns");
        agentTurnInProgress = true;
        turnStartTime = Time.time;
        
        // Reset all agents for their turn
        foreach (var agent in agents)
        {
            agent.turnFinished = false;
            Debug.Log($"{agent.name}: turnFinished reset to false");
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
        // Force all agents to finish their turns
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