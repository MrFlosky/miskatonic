using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardCultistMasses : RewardCultist
{
    private int cultistsToRewardTotal;
    private CultistPoints cps;


    public void Init(CultistPoints cps)
    {
        this.cps = cps;
    }

    public void SetCultistsRewarded()
    {
        // get cultist number from characterFears instead of from mission data
        cultistsToRewardTotal = CharacterFears.Instance.totalFearsDiscovered;
    }

    public override void GrantReward()
    {
        // Call this again to update 
        SetCultistsRewarded();
        cps.AddPoints(cultistsToRewardTotal);
        DisplayRewardMessage();
    }

    public override void InitRewardUI()
    {
        this.enabled = false;
        GameObject rewardPopup = Instantiate(rewardPopupUI); // instantiate prefab and set ref
        rewardPopupUI = rewardPopup;
        rewardPopupUI.SetActive(false);

        rewardText = rewardPopupUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        rewardText.text = cultistsToRewardTotal.ToString() + " " + rewardName;
    }

    public override void DisplayRewardMessage()
    {
        rewardPopupUI.SetActive(true);
        timeElapsed = 0;
        rewardText.text = cultistsToRewardTotal.ToString() + " " + rewardName;
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
