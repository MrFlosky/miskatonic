using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockSwap : UnlockReward
{
    public override void InitSkill()
    {
        skill = MissionCollection.skillObjectStatic.GetComponent<SkillSwapRooms>();
    }
}
