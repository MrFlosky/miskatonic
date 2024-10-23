using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITurn : Turns
{
    // Start is called before the first frame update
    void Start()
    {
        turnType = TurnType.AI;
    }

    public override void InitiateTurn()
    {
        base.InitiateTurn(); // Upcall
        turnEvents.Invoke();
    }
}
