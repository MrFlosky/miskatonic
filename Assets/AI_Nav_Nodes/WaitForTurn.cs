using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class WaitForTurn : ActionNode
{
    protected override void OnStart() {
        // Initialization, if needed
    }

    protected override void OnStop() {
        // Clean up logic if needed
    }

    protected override State OnUpdate() {
        // Check if it's the player's turn (assuming you have a GameManager managing turns)
        if (TurnBasedSystem.IsPlayerTurn() || context.agentNavigation.turnFinished) {
            return State.Running; // Keep waiting while it's the player's turn
        } else {
            return State.Success; // Proceed when it's the agent's turn
        }
    }
}
