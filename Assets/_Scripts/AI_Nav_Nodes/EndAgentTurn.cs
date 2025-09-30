using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EndAgentTurn : ActionNode
{
    protected override void OnStart() {
        // Mark this agent's turn as finished
        context.agentNavigation.turnFinished = true;
        
        // Stop any remaining movement
        context.agentNavigation.StopMovement();
        
        Debug.Log($"{context.gameObject.name}: Turn ended");
        
        // Notify the AgentManager to check if all agents are done
        if (AgentManager.Instance != null)
        {
            AgentManager.Instance.AgentCall();
        }
    }

    protected override void OnStop() {
        // Clean up if needed
    }

    protected override State OnUpdate() {
        return State.Success;
    }
}