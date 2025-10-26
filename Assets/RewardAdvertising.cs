using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardAdvertising : RewardCultist
{

    public override void GrantReward()
    {
        AncientAdvertising.S.StartPaying(cultistsToReward);
        DisplayRewardMessage();
    }
}
