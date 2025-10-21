using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardFearTotem : SkillReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillFearTotem>();
    }
}
