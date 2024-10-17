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
    
    static public TurnType currentTurn;

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
    List<DestroyAfterRounds> readyToDestroy = new List<DestroyAfterRounds>(); // NEED THIS FOR PROPER DELETION CANT LOOP THRU LIST DELETING FROM!
    
    // Start is called before the first frame update
    void Start()
    {
        S = this;
        S.txt.text = "Total Turns: " + S.totalTurnCount + "\n" + "Player Turns: " + S.playerTurnCount + "\n" + "AI Turns: " + S.aiTurnCount; ///***DEBUG
        currentTurn = TurnType.Player;
    }
    
    static public bool IsAITurn() {
        return currentTurn == TurnType.AI;
    }
    
    static public bool IsPlayerTurn() {
        return currentTurn == TurnType.Player;
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
            {
                return S.playerTurnCount;
            }

            case TurnType.AI:
            {
                return S.aiTurnCount;
            }

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
            currentTurn = TurnType.Player; 
            S.playerTurnCount ++;
            Debug.Log("LETS GO");
            break;

            case TurnType.AI:
            currentTurn = TurnType.AI;
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
        // Once added to destroyable list, destroy them. must be done this way. 
        foreach(DestroyAfterRounds destroy in S.readyToDestroy)
        {
            S.destroyablesList.Remove(destroy);
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
        WallType wallType;
        public DestroyAfterRounds(int rounds, GameObject obj, WallType wT)
        {
            destroyOnRound = rounds + S.playerTurnCount;
            obToDestroy = obj;
            wallType = wT;
            Debug.Log(obj.name + "TO DESTROY");
            S.destroyablesList.Add(this);
        }

    
        public void CheckDestroy()
        {
            if(destroyOnRound == S.playerTurnCount)
            {
                MazeCellObject cell = obToDestroy.GetComponentInParent<MazeCellObject>();
                
                MazeGenerator.Instance.SetWallInternalState(cell.GetXCoord(), cell.GetYCoord(), wallType, false); // Deactivate this wall internally in the maze array
                obToDestroy.SetActive(false); // need set active so can be reselected letter
                S.readyToDestroy.Add(this);
                
            }
        }
    }


}
