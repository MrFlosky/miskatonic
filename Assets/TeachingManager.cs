using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeachingManager : MonoBehaviour, ITurnObserver
{


    public static TeachingManager S;

    void Start()
    {
        if (S == null) S = this;
        TurnBasedSystem.SubscribeAsObserver(this);
        TURN_ZERO_TUTORIAL_EVENT();
    }


    public void EnemyTurnInitiated()
    {

    }


    public void PlayerTurnInitiated()
    {
        Debug.LogWarning(TurnBasedSystem.GET_TURN_COUNT(TurnType.Player));
    }


    private void TURN_ZERO_TUTORIAL_EVENT()
    {
        
    }


    private void TURN_ONE_TUTORIAL_EVENT()
    {
        
    }
}
