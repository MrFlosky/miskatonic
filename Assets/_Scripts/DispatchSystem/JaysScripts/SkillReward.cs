using System.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// Add the skill usage points provided by the mission data 
    /// </summary>
    public override void GrantReward()
    {
        skill.AddSkillUsage(usagesToReward);
    }

    /// <summary>
    /// Set number of usages to reward upon calling grant reward
    /// </summary>
    public void SetUsages(int usages)
    {
        usagesToReward += usages;
    }
}
