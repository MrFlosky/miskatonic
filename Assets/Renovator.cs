using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Renovator : MonoBehaviour, ITurnObserver
{
    [SerializeField]public int turnsRemaining;

    void Awake()
    {
        
    }

    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
    }
    void ITurnObserver.PlayerTurnInitiated()
    {
        if (turnsRemaining <= 0)
        {   
            SkillRenovation sr = GetComponentInParent<SkillRenovation>();
            sr.EndRenovation();   
        }


    }

    void ITurnObserver.EnemyTurnInitiated()
    {
        Debug.LogError("lil' help!?");
        turnsRemaining--;
    }
    
}
