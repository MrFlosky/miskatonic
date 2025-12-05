using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeachingManager : MonoBehaviour, ITurnObserver
{   


    public static TeachingManager S;

    public List<GameObject> uiToDisable = new List<GameObject>();
    public GameObject storyboardRef;
    public GameObject abilityPanelRef;
    public bool inTutorial = true;

    void Start()
    {
        if (S == null) S = this;
        TurnBasedSystem.SubscribeAsObserver(this);
        StartTutorial();
    }


    public void EnemyTurnInitiated()
    {

    }


    public void PlayerTurnInitiated()
    {
        Debug.LogWarning("TM TURN: " + TurnBasedSystem.GET_TURN_COUNT(TurnType.Player));
    }


    private void StartTutorial()
    {
        // HIDE MOST UI ELEMENTS
        foreach (GameObject go in uiToDisable)
        {
            go.SetActive(false);
        }


        storyboardRef.SetActive(false);

        ClickAI.S.StartConversation();

        // POP DIALOGUE THAT SAYS: 
        // Welcome to your maze my liege. I wish I could say that your maze is inescapable, but alas it is not.
        
        // Gets any random DB to play the cultist dialogues
        DialogueBank DB = AgentManager.Instance.agents[0].GetComponent<DialogueBank>();
        DB.StartIntro1();

        // If the humans are able to reach their goal rooms and retrieve a key they can then unlock the door in the ritual room, leading to their escape.
        //  - pan to ritual room
        
        // You can find each human's objective in the journal I have provided to you, along with some other information.
        //  - activate glow behind character profiles

        // If you wish to get to know the humans better I implore you to read the backstories I have provided as well.
        //  - glow behind storyboard
    }

    public void EnableStoryboard()
    {
        storyboardRef.SetActive(true);
    }

    public void OpenBookmark()
    {
        abilityPanelRef.SetActive(true);
        ClickAI.S.EndConversation();
    }

    public void EndTutorial()
    {
        ClickAI.S.EndTutorial();

        // SHOW UI ELEMENTS
        foreach (GameObject go in uiToDisable)
        {
            go.SetActive(true);
        }

        inTutorial = false;
    }


    public void StartDispatchTutorial()
    {
        ClickAI.S.StartConversation();

        DialogueBank DB = AgentManager.Instance.agents[0].GetComponent<DialogueBank>();
        DB.StartDSTut1();
    }
}
