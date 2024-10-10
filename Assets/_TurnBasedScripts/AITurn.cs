using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITurn : Turns
{
    // Start is called before the first frame update
    void Start()
    {
        turnType = TurnType.AI;
        this.enabled = false;

    }

    float timeElapsed = 0f;
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if(timeElapsed >= delayUntilNext)
        {

            NextTurn(TurnType.Player); 
            this.enabled = false;

            
        }
    }

    public override void InitiateTurn()
    {
        timeElapsed = 0;
        this.enabled = true;

        base.InitiateTurn(); // Upcall
        turnEvents.Invoke();

    }
}
