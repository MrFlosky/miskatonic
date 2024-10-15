using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EndAgentTurn : ActionNode
{
    protected override void OnStart() {
        // This is called when the node is first activated.
    }

    protected override void OnStop() {
        // This is called when the node is stopped.
    }

    protected override State OnUpdate() {
        // Notify the GameManager to switch the turn back to the player
        Turns.s.playerTurn.InitiateTurn();
        // Always return success since this node is meant to just trigger the turn end
        return State.Success;
    }
}
