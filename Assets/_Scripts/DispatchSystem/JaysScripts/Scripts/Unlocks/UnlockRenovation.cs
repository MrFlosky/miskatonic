using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockRenovation : UnlockReward
{
 
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillRenovation>();
    }
}
