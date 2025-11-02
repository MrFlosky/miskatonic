using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockPsychosis : UnlockReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillPsychosis>();
    }
}
