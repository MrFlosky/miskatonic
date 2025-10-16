using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardCultist : Reward
{
    protected int cultistsToReward;
    protected CultistPoints cultistPointSystem;

    public void Init(CultistPoints cps)
    {
        
        cultistPointSystem = cps;
        print(cultistPointSystem.gameObject.name);
    }

    public void SetCultistsRewarded(int reward)
    {
        cultistsToReward = reward;
    }

    public override void GrantReward()
    {
        cultistPointSystem.AddPoints(cultistsToReward);
        DisplayRewardMessage();
    }

    public override void InitRewardUI()
    {
        this.enabled = false;
        GameObject rewardPopup = Instantiate(rewardPopupUI); // instantiate prefab and set ref
        rewardPopupUI = rewardPopup;
        rewardPopupUI.SetActive(false);

        rewardText = rewardPopupUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        rewardText.text = cultistsToReward.ToString() + " " + rewardName;
    }

    public override void DisplayRewardMessage()
    {
        rewardPopupUI.SetActive(true);
        timeElapsed = 0;
        this.enabled = true;
    }

    // only on when enabled
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= timeToDissapear)
        {
            rewardPopupUI.SetActive(false);
            this.enabled = false;
        }
    }
}
