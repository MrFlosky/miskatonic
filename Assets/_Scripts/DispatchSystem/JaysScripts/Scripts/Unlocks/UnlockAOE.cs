using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockAOE : UnlockReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillAoeFear>();
    }
}
