using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for initiating and ending the dealing of cards.
/// </summary>
public class DispatchSystem : MonoBehaviour , ITurnObserver
{

    // true if player chose mission this turn, false if they didnt
    static public bool dealerDealtWith = false;

    /// <summary>
    /// Text for reroll, including number left.
    /// </summary>
    [SerializeField] private TextMeshProUGUI rerollTxt;

    /// <summary>
    /// Number of rerolls available to player.
    /// </summary>
    int rerolls = 1;

    void Awake()
    {
        rerollTxt.text = "Rerolls x" + rerolls;
    }

    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
    }

    /// <summary>
    /// Called by dealer button press, enable dealer canvas and display available missions.
    /// </summary>
    public void DealCards()
    {
        UIManagerLite.TransitionToState(UIManagerLite.GameState.Dealer);


        MissionCollection.S.DISPLAY_AVAILABLE_MISSIONS();
        MissionCollection.S.ShuffleCheck(); // shuffle second so new pool ready on next turn
    }

    // <summary>
    // Called by missions:
    // After a player has chosen a mission, deactive canvas and allow them to end turn.
    // </summary>
    static public void DealingOver()
    {
        UIManagerLite.TransitionToState(UIManagerLite.GameState.BoardBase);
        dealerDealtWith = true;
        MissionCollection.S.HIDE_ALL_MISSIONS();
    }

    /// <summary>
    /// Called upon reroll button press.
    /// </summary>
    public void Reroll()
    {
        if(rerolls > 0)
        {
            rerolls -= 1;
            MissionCollection.S.Reroll();
            rerollTxt.text = "Rerolls x" + rerolls;
        }
        
    }

    /// <summary>
    /// Every player turn, force interaction with dealer
    /// before allowing end turn.
    /// </summary>
    void ITurnObserver.PlayerTurnInitiated()
    {
        dealerDealtWith = false;

        // add reroll and update text
        rerolls = 1;
        rerollTxt.text = "Rerolls x" + rerolls;
        //initiateCardDealerButton.gameObject.SetActive(true);
    }
    void ITurnObserver.EnemyTurnInitiated()
    {
        // Make sure end turn button is disabled before shown to player.
        // For a moment its disabled if placed in function above.
        //UIManagerLite.DisableEndTurnButton();
    }
}
