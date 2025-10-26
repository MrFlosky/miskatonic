using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AncientAdvertising : MonoBehaviour, ITurnObserver
{
    public static AncientAdvertising S;
    public bool payingOff = false;
    public int turnsLeft = 0;
    public CultistPoints cps;
    private int payoffPerTurn;

    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
        
        if (S == null) S = this;
    }


    public void StartPaying(int amt)
    {
        turnsLeft = 5;
        payingOff = true;
        payoffPerTurn = amt;
    }
    
    void ITurnObserver.PlayerTurnInitiated()
    {
        if (payingOff)
        {
            // reward cultist
            cps.AddPoints(payoffPerTurn);
            turnsLeft--;
            if (turnsLeft == 0) payingOff = false;
        }
    }

    
    void ITurnObserver.EnemyTurnInitiated() {}
}
