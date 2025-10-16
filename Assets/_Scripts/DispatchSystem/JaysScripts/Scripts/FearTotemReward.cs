using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearTotemReward : SkillReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillFearTotem>();
    }
}
