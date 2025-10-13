using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RewardWoM : RewardCultist, ITurnObserver
{
    /// <summary>
    /// the game object this is attached to will add 1 cultist at the beginning of a players turn for every 10 cultists they have
    /// 
    /// activated ensures that the object must have been activated by the reward card
    /// 
    /// maxGain will create a cap on how many cultists can be gained i.e. limiting it to 3 per turn even if a player has 40 cultists 
    /// </summary>

    public bool activated;
    public int maxGain;
    


    private void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
    }




    void ITurnObserver.PlayerTurnInitiated()
    {
        if (!activated) return;
        cultistPointSystem = CultistPoints.S;
        int currentCult = cultistPointSystem.cultistPoints;
        currentCult = Mathf.FloorToInt(currentCult / 10);
        if (currentCult > maxGain) currentCult = maxGain;

        cultistsToReward = currentCult;
      
        GrantReward();

    }



    //unused
    void ITurnObserver.EnemyTurnInitiated() { }
}
