using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardSwap : SkillReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillSwapRooms>();
    }
}
