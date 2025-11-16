using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardInceptLoneliness : Reward
{

    public override void DisplayRewardMessage()
    {
        rewardPopupUI.SetActive(true);
        timeElapsed = 0;
        this.enabled = true;
    }

    public override void GrantReward()
    {
        var skill = MissionCollection.skillObjectStatic.GetComponent<SkillInceptLoneliness>();
        skill.ActivateSkill();
        DisplayRewardMessage();
    }

    public override void InitRewardUI()
    {
        this.enabled = false;
        GameObject rewardPopup = Instantiate(rewardPopupUI); // instantiate prefab and set ref
        rewardPopupUI = rewardPopup;
        rewardPopupUI.SetActive(false);
        //rewardPopupUI.transform.SetParent(transform);

        rewardText = rewardPopupUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        rewardText.text = rewardName + " UNLOCKED!";
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
