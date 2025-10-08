using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Yarn.Unity;
using CharacterData;

public class NarrativeReward : Reward
{
    // Handles yarn dialogue management for narrative rewards
    private NarrativeRewardTracker tracker;
    // Character who this narrative clue is for
    Character characterTarget;


    // only way of displaying narrative reward, without yarn spinner
    // public override void InitRewardUI()
    // {
    //     this.enabled = false;
    //     GameObject rewardPopup = Instantiate(rewardPopupUI); // instantiate prefab and set ref
    //     rewardPopupUI = rewardPopup;
    //     rewardPopupUI.SetActive(false);

    //     rewardText = rewardPopupUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    //     //rewardText.text = "Insert Narrative Clue Here";
    //     rewardText.text = narrativeClue;
    // }

    // Not needed here, as they are handled by yarn spinner!
    public override void InitRewardUI()
    {
        return;
    }
    public override void DisplayRewardMessage()
    {
        return;
    }

    public override void GrantReward()
    {
        tracker.PlayRandomDialogue(characterTarget);
    }

    // only on when enabled
    // void Update()
    // {
    //     timeElapsed += Time.deltaTime;

    //     if (timeElapsed >= timeToDissapear)
    //     {
    //         rewardPopupUI.SetActive(false);
    //         this.enabled = false;
    //     }
    // }

    /// <summary>
    /// Set character target to prepare to play one
    /// of their dialogues from NarrativeRewardTracker.
    /// </summary>
    public void InitNarrativeClue(Character chara)
    {
        characterTarget = chara;
        tracker = MissionCollection.trackerStatic;
    }


}
