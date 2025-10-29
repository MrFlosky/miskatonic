using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardRenovation : SkillReward
{ 

    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillRenovation>();
    }
}
