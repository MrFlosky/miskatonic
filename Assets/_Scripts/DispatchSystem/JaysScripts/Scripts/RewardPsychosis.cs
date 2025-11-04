using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardPsychosis : SkillReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillPsychosis>();
    }
}
