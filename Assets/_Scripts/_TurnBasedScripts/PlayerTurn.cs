using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueEditor;
using UnityEngine.Events;

public class PlayerTurn : Turns
{

    // Start off as disabled, only enable on button press.
    void Start()
    {
        turnType = TurnType.Player; // Every turn script must set it's turn type.
        this.enabled = false;
    }

    // I can probably remove this...
    float timeElapsed = 0f;
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if(timeElapsed >= delayUntilNext)
        {
            NextTurn(TurnType.AI);
            this.enabled = false;
        }
    }

    /// <summary>
    /// Enable this script, beginning the player turn. gonna need reference to button
    /// </summary>
    public void buttonPressed()
    {
        timeElapsed = 0f;
        this.enabled = true;
        //base.InitiateTurn(); // Upcall DEBUG
        //InitiateTurn();
        
        ConversationManager.Instance.EndConversation();
    }

    /*
    * UnityEvent turnEvents holds all actions to perform when the player's turn is initiated. 
    * Not to be used by the player themselves!!
    * Base functionality of every turn performed, then all those attaached to event.
    */
    public override void InitiateTurn()
    {
        base.InitiateTurn(); // Upcall REINSTATE THIS
        turnEvents.Invoke();
    }

}

// do the turnevents in the base class get invoked on top of ones here?