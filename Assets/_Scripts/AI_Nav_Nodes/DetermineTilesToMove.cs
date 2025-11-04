using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

/// <summary>
/// This action node is responsible for setting how many tiles an agent should move this turn based on their fear state.
/// </summary>
public class DetermineTilesToMove : ActionNode
{
    protected override void OnStart()
    {
        // Get AI's fear state and use to determine how many tiles it should move.
        FearState currentFearState = context.aiFear.GetFearState();
        AgentNavigation aiNav = context.agentNavigation;
        int tilesToMove = FearCollection.S.GetStateMovement(currentFearState);

        // if tweaking, move random number of tiles between 1 and defined number of tiles to move
        if(currentFearState == FearState.Tweaking)
        {
            tilesToMove = Random.Range(1, tilesToMove);
        }
        aiNav.tilesPerTurn = tilesToMove;
        Debug.LogWarning(context.aiFear.transform.parent.name + "state: " + currentFearState + " MOVED " + FearCollection.S.GetStateMovement(currentFearState));



        // context.dialogueBank
    }

    protected override void OnStop()
    {
        return;
    }

    protected override State OnUpdate()
    {
        return State.Success;
    }
}
