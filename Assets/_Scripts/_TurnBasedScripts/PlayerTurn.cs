using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueEditor;
using UnityEngine.Events;

public class PlayerTurn : Turns, ITurnObserver
{

    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
    }

    /// <summary>
    /// What should this turn type do when its initiated.
    /// Called as observer by TurnBasedSystem.
    /// </summary>
    void ITurnObserver.PlayerTurnInitiated()
    {
        turnText.text = "PLAYER TURN!";
        endTurnButton.SetActive(true);
        skillsUI.DISPLAY_SKILLS();
        WorshipPoints.DISPLAY_PTS();
        IdolPoints.DISPLAY_PTS();
        GameManager.Instance.CHECK_ROUND_LOSE();
    }


    void ITurnObserver.EnemyTurnInitiated()
    {
        // NOTHING
    }

}

// do the turnevents in the base class get invoked on top of ones here?