using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Turns : MonoBehaviour
{
    public static Turns s;

    [Header("Event associated with this turn type.")]
    public UnityEvent turnEvents;
    public float delayUntilNext = 5f; // Every turn may not require a delay.
    protected TurnType turnType = TurnType.NeverSet; // Every turn has a turnType associated with it. Protected allows to be seen by subclass but not shown in inspector (not visible to any class)

    /// References to each derived class (type of turn).
    public PlayerTurn playerTurn;
    private AITurn aiTurn;

    // Start is called before the first frame update
    void Start()
    {
        s = this;
        //Problem is during upcalls the derived classes dont set these. Solved by turning this class into singleton.
        s.playerTurn = gameObject.GetComponent<PlayerTurn>();
        s.aiTurn = gameObject.GetComponent<AITurn>();

    }

    void Update()
    {
        
        
    }

    /// <summary>
    /// Applied to all turns upon their initiation.
    /// Add their respective turn type to the system and invoke general turn events.
    /// </summary>
    public virtual void InitiateTurn()
    {
       TurnBasedSystem.ADD_TURN(turnType); //turnType treated as that from derived class.
       s.turnEvents.Invoke(); 
    }


    /// <summary>
    /// Initiates given next turn, to be used in each turn type.
    /// Note: this may be better suited in collection class.
    /// </summary>
    /// <param name="tt">
    /// Type of turn to initiate.
    /// </param>
    static public void NextTurn(TurnType tt)
    {
        switch(tt)
        {
            case TurnType.Player:
            s.playerTurn.InitiateTurn();
            break;

            case TurnType.AI:
            s.aiTurn.InitiateTurn();
            break;

        } 
    }
    
    

    // implement function that ends turn, beginning AIs HEREEEEEEEE


}
