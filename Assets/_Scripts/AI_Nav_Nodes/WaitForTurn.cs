using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class WaitForTurn : ActionNode
{
    protected override void OnStart() {
        // This runs when the node first activates
    }

    protected override void OnStop() {
        // Clean up if needed
    }

    protected override State OnUpdate() {
        // Keep waiting while it's the player's turn
        if (TurnBasedSystem.IsPlayerTurn()) {
            return State.Running; // Keep waiting
        }
        
        // Also keep waiting if this agent has already finished its turn
        if (context.agentNavigation.turnFinished) {
            return State.Running; // Keep waiting for other agents
        }
        
        // It's agent turn time AND this agent hasn't taken its turn yet - proceed
        return State.Success;
    }
}
