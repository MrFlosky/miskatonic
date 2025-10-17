using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardCultistMasses : RewardCultist
{
    private int cultistsToReward;
    private CultistPoints cultistPointSystem;


    public void Init(CultistPoints cps)
    {
        cultistPointSystem = cps;
    }

    public void SetCultistsRewarded()
    {
        // get cultist number from gamemanager instead of from mission data
        cultistsToReward = GameManager.Instance.fearsDiscovered;
    }

    public override void GrantReward()
    {
        // Call this again to update 
        SetCultistsRewarded();
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
        rewardText.text = cultistsToReward.ToString() + " " + rewardName;
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
