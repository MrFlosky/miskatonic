using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockFearTotem : UnlockReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillFearTotem>();
    }

}
