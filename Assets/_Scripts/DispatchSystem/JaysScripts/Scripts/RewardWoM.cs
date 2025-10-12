using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RewardWoM : RewardCultist, ITurnObserver
{
   // public static RewardWoM S { get; private set; }
    public bool activated;
    public int maxGain;
    


    private void Start()
    {
        
        TurnBasedSystem.SubscribeAsObserver(this);
    }




    void ITurnObserver.PlayerTurnInitiated()
    {
        if (!activated) return;
        cultistPointSystem= CultistPoints.S;
        int currentCult = cultistPointSystem.cultistPoints;
        currentCult = Mathf.FloorToInt(currentCult / 10);
        if(currentCult > maxGain) currentCult = maxGain;

        cultistsToReward = currentCult;
        Debug.LogError(cultistsToReward);
        GrantReward();

        Debug.LogError(cultistPointSystem.cultistPoints);
    }


    void ITurnObserver.EnemyTurnInitiated() { }
}
