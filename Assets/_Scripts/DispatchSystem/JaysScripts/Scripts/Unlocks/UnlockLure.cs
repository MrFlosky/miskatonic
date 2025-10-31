using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockLure : UnlockReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillLure>();
    }
}
