using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardRnD : Reward
{

    public override void DisplayRewardMessage()
    {
        rewardPopupUI.SetActive(true);
        timeElapsed = 0;
        this.enabled = true;

    }

    public override void GrantReward()
    {
        FearMeter.S.RnD = true;
        DisplayRewardMessage();
    }

    public override void InitRewardUI()
    {
        
    }

}
