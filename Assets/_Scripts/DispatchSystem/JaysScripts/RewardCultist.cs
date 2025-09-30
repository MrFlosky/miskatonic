using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCultist : Reward
{
    private int cultistsToReward;
    private CultistPoints cultistPointSystem;

    public void Init(CultistPoints cps)
    {
        cultistPointSystem = cps;
    }

    public void SetCultistsRewarded(int reward)
    {
        cultistsToReward = reward;
    }

    public override void GrantReward()
    {
        cultistPointSystem.AddPoints(cultistsToReward);
    }
}
