using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// All possible turn types.
/// Neverset is used as a default for function args and bug catching.
/// </summary>
public enum TurnType{Player, AI, Event, NeverSet};

public class TurnBasedSystem : MonoBehaviour
{
    static private TurnBasedSystem S;

    ///***DEBUG
    public TextMeshProUGUI txt;

    /// <summary>
    /// Tracking # of turns of each type taken, as well as total turns.
    /// </summary>
    private int totalTurnCount = 0;
    private int playerTurnCount = 0;
    private int aiTurnCount = 0;
    private int eventTurnCount = 0;

    /// Number of rounds until defeat.
    [Header("Number of rounds until player defeat.")]
    public int roundsUntilDefeat = 10;

    /// List of items to destroy after x turns
    List<DestroyAfterRounds> destroyablesList = new List<DestroyAfterRounds>();
    
    // Start is called before the first frame update
    void Start()
    {
        S = this;
        S.txt.text = "Total Turns: " + S.totalTurnCount + "\n" + "Player Turns: " + S.playerTurnCount + "\n" + "AI Turns: " + S.aiTurnCount; ///***DEBUG
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    /// <summary>
    /// Gets private members associated with system class.
    /// Defaults to totalTurnCount if no arguments provided.
    /// </summary>
    /// <param name="tt">
    /// Type of turn you want the count of. Leave empty if want total combined turns.
    /// </param>
    /// <returns>
    /// Int count of provided turn type.
    /// </returns>
    static public int GET_TURN_COUNT(TurnType tt = TurnType.NeverSet) // must be set to static to be used as singleton function in other classes.
    {

            switch(tt)
        {
            case TurnType.Player:
            return S.playerTurnCount;

            case TurnType.AI:
            return S.aiTurnCount;

            case TurnType.Event:
            return S.eventTurnCount;

            default:
            return S.totalTurnCount;
            
        }
    }


    /// <summary>
    /// Adds respective turn taken to data stored in system class.
    /// </summary>
    /// <param name="tt">
    /// Type of turn taken.
    /// </param>
        static public void ADD_TURN(TurnType tt)
    {
        S.totalTurnCount += 1;

        switch(tt)
        {
            case TurnType.Player:
            S.playerTurnCount ++;
            Debug.Log("LETS GO");
            break;

            case TurnType.AI:
            S.aiTurnCount ++;
            break;

            case TurnType.Event:
            S.eventTurnCount ++;
            break;

            default:
            Debug.Log("Well. That didn't work.");
            break;

        }


        S.txt.text = "Total Turns: " + S.totalTurnCount + "\n" + "Player Turns: " + S.playerTurnCount + "\n" + "AI Turns: " + S.aiTurnCount;

        // Each round added, check if any destroyable need be destroyed.
        foreach(DestroyAfterRounds destroyable in S.destroyablesList)
        {
            destroyable.CheckDestroy();
        }

    }

    /// <summary>
    /// Check if player has reached the ending round.
    /// </summary>
    static public void ROUND_CHECK()
    {
        if(S.totalTurnCount == S.roundsUntilDefeat)
        {
            // Gameover
            Destroy(S.gameObject); ///*** DEBUG
        }
    }


    // This is super cool! Right now it destroys, but can make it so it deactivates.
    public class DestroyAfterRounds
    {
        int destroyOnRound;
        GameObject obToDestroy;
        public DestroyAfterRounds(int rounds, GameObject obj)
        {
            this.destroyOnRound = rounds + S.playerTurnCount;
            this.obToDestroy = obj;
            Debug.Log(obj.name);
            S.destroyablesList.Add(this);
        }

        public void CheckDestroy()
        {
            if(destroyOnRound == S.playerTurnCount)
            {
                Destroy(obToDestroy);

            }
        }
    }


}
