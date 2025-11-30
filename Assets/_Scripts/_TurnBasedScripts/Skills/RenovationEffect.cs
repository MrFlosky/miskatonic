using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenovationEffect : MonoBehaviour, ITurnObserver
{
    public int TurnsElapsed;
    public int maxTurns;
    public int targetIndex;
    public SkillRenovation skillRenovation;
    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
        TurnsElapsed = 0;
    }

    
   
   void ITurnObserver.PlayerTurnInitiated()
    {
        if (TurnsElapsed == maxTurns)
        {


            skillRenovation.EndRenovation(targetIndex);
            
        }

    }
    
    void ITurnObserver.EnemyTurnInitiated()
    {
        TurnsElapsed++;
    }
}
