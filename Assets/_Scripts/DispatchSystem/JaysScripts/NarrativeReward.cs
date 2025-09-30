using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NarrativeReward : Reward
{
    private string narrativeClue;

    public override void InitRewardUI()
    {
        this.enabled = false;
        GameObject rewardPopup = Instantiate(rewardPopupUI); // instantiate prefab and set ref
        rewardPopupUI = rewardPopup;
        rewardPopupUI.SetActive(false);

        rewardText = rewardPopupUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        //rewardText.text = "Insert Narrative Clue Here";
        rewardText.text = narrativeClue;
    }

    public override void DisplayRewardMessage()
    {
        rewardPopupUI.SetActive(true);
        timeElapsed = 0;
        this.enabled = true;
    }

    public override void GrantReward()
    {
        DisplayRewardMessage();
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

    public void InitNarrativeClue(string narrative)
    {
        narrativeClue = narrative;
    }


}
