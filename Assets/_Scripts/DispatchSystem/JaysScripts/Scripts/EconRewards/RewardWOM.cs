using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardWOM : RewardCultist, ITurnObserver
{

    int maxGain = 3;
    int mult = 3;
    private void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= timeToDissapear)
        {
            rewardPopupUI.SetActive(false);
            this.enabled = false;
        }
    }

    public override void GrantReward()
    {
        cultistsToReward = mult * Mathf.FloorToInt(cultistPointSystem.cultistPoints / 10);
        if (cultistsToReward > maxGain) cultistsToReward = maxGain;
        base.GrantReward();
    }

    void ITurnObserver.PlayerTurnInitiated()
    {
        //Debug.LogError("You have " + cultistPointSystem.cultistPoints);
        GrantReward();
        //Debug.LogError("You now have " + cultistPointSystem.cultistPoints);
    }

    void ITurnObserver.EnemyTurnInitiated()
    {
        
    }

}
