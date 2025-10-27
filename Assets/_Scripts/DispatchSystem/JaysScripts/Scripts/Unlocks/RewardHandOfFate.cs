using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// This class is responsible for the hand of fate reward, granting player ability to 
/// sacrifice cultists for a reroll.
/// </summary>
public class RewardHandOfFate : Reward
{
    public override void DisplayRewardMessage()
    {
        rewardPopupUI.SetActive(true);
        timeElapsed = 0;
        this.enabled = true;
    }

    public override void GrantReward()
    {
        DispatchSystem.UnlockRerollSacrifice();
    }

    public override void InitRewardUI()
    {
        this.enabled = false;
        GameObject rewardPopup = Instantiate(rewardPopupUI); // instantiate prefab and set ref
        rewardPopupUI = rewardPopup;
        rewardPopupUI.SetActive(false);
        //rewardPopupUI.transform.SetParent(transform);

        rewardText = rewardPopupUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        rewardText.text =  rewardName + " UNLOCKED!";
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
