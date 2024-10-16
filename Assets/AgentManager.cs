using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager Instance;
    
    public AgentNavigation[] agents;

    private void Start()
    {
        Instance = this;
        if (agents.Length == 0)
        {
            agents = FindObjectsOfType<AgentNavigation>();
        }
    }
    
    public void AgentCall()
    {
        if (!CheckAgents()) return;
        foreach (var agent in agents)
        {
            agent.turnFinished = false;
        }
        Turns.NextTurn(TurnType.Player);
    }
    
    private bool CheckAgents()
    {
        return agents.All(agent => agent.turnFinished); // check if all agents are finished
    }
            
}
