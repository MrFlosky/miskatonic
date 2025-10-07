using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardBarrier : SkillReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillBarrier>();
    }
}
