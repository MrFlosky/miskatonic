using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockBarrier : UnlockReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillBarrier>();
    }
}
