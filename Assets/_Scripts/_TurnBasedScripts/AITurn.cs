using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITurn : Turns, ITurnObserver
{
    // Start is called before the first frame update
    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
    }


    void ITurnObserver.EnemyTurnInitiated()
    {
        // TODO: WILL NEED TO END CONVERSATION IF IN ONE HERE!

        skillsUI.DEACTIVATE_SKILLS();
        turnText.text = "AI TURN!";
        endTurnButton.SetActive(false);
        skillsUI.HIDE_SKILLS();
        WorshipPoints.HIDE_PTS();
        IdolPoints.HIDE_PTS();
    }

    void ITurnObserver.PlayerTurnInitiated()
    {
        // NOTHING
    }

}
