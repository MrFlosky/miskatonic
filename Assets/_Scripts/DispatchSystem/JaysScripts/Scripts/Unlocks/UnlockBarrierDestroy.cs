using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockBarrierDestroy : UnlockReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillBarrierDestroy>();
    }
}
