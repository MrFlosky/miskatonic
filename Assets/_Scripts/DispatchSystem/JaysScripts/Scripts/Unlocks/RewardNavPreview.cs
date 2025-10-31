using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;


/// <summary>
/// This class is responsible for the reward that grants the player the ability to see
/// AI paths for a given number of turns.
/// </summary>
public class RewardNavPreview : Reward
{
    /// <summary>
    /// Number of preview turns granted.
    /// </summary>
    private int previewsGranted = 0;
    
    public void SetPreviewsRewarded(int reward)
    {
        previewsGranted = reward;
    }
    
    public override void DisplayRewardMessage()
    {
        rewardPopupUI.SetActive(true);
        timeElapsed = 0;
        this.enabled = true;
    }

    public override void GrantReward()
    {
        PathVisualizer.Instance.GrantPreviews(previewsGranted);
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
