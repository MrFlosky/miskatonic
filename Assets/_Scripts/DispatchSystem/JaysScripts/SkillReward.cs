using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// This class is abstract, responsible for stating all additional attributes/functions all Skill Rewards will have.
/// </summary>
public abstract class SkillReward : Reward
{
    // Skill associated with this skill reward
    protected Skill skill;
    // Number of usages to reward for skill
    private int usagesToReward;

    /// <summary>
    /// Associate this skill reward with its respective skill script
    /// </summary>
    public abstract void InitSkill();

    public override void InitRewardUI()
    {
        this.enabled = false;
        GameObject rewardPopup = Instantiate(rewardPopupUI); // instantiate prefab and set ref
        rewardPopupUI = rewardPopup;
        rewardPopupUI.SetActive(false);
        //rewardPopupUI.transform.SetParent(transform);
       
        rewardText = rewardPopupUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        rewardText.text = usagesToReward.ToString() + " " + rewardName + " usages";
    }

    /// <summary>
    /// Add the skill usage points provided by the mission data 
    /// </summary>
    public override void GrantReward()
    {
        skill.AddSkillUsage(usagesToReward);
        DisplayRewardMessage();
    }

    /// <summary>
    /// Set number of usages to reward upon calling grant reward
    /// </summary>
    public void SetUsages(int usages)
    {
        usagesToReward += usages;
    }

    /// <summary>
    /// interesting, if this provides override then children dont have to
    /// </summary>
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
