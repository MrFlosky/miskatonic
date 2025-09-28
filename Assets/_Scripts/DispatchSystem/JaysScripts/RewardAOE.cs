using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardAOE : SkillReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillAoeFear>();
    }
}
