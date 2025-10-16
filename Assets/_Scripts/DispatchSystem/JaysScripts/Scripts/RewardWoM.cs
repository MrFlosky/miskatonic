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
    CultistPoints cps;

    private void Awake()
    {
        activated = true;
        
    }

    private void Start()
    {
        maxGain = 3;
        TurnBasedSystem.SubscribeAsObserver(this);



       
    }


    public override void GrantReward()
    {

        if (!activated) return;
        cultistsToReward= Mathf.FloorToInt(cultistPointSystem.cultistPoints/10);
        if(cultistsToReward> maxGain) cultistsToReward=maxGain;
        Debug.LogError(cultistsToReward);
        base.GrantReward();

    }


    void ITurnObserver.PlayerTurnInitiated()
    {
        
        
      
        GrantReward();

    }



    //unused
    void ITurnObserver.EnemyTurnInitiated() { }
}
