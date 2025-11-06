using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevovationEffect : MonoBehaviour, ITurnObserver
{
    public int TurnsElapsed;
    [SerializeField] int maxTurns;
    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
        TurnsElapsed = 0;
    }

    
   
   void ITurnObserver.PlayerTurnInitiated()
    {
        if (TurnsElapsed == maxTurns)
        {
            Debug.Log("This room is no longer under the effects of renovation");
            Destroy(gameObject);
        }

    }
    
    void ITurnObserver.EnemyTurnInitiated()
    {
        TurnsElapsed++;
    }
}
