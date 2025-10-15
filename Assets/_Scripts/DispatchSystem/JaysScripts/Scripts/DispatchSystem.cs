using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for initiating and ending the dealing of cards.
/// </summary>
public class DispatchSystem : MonoBehaviour , ITurnObserver
{

    // true if player chose mission this turn, false if they didnt
    static public bool dealerDealtWith = false;
    // Canvas that holds dealer canvas with cards
    [SerializeField] private Canvas dealerCanvas;


    void Start()
    {
        // TurnBasedSystem.SubscribeAsObserver(this);
    }

    /// <summary>
    /// Called by dealer button press, enable dealer canvas and display available missions.
    /// </summary>
    public void DealCards()
    {
        //dealerCanvas.gameObject.SetActive(true);
        UIManagerLite.TransitionToState(UIManagerLite.GameState.Dealer);

        MissionCollection.S.DISPLAY_AVAILABLE_MISSIONS();
    }

    // <summary>
    // Called by missions:
    // After a player has chosen a mission, deactive canvas and allow them to end turn.
    // </summary>
    static public void DealingOver()
    {
        UIManagerLite.TransitionToState(UIManagerLite.GameState.BoardBase);
        dealerDealtWith = true;
        //initiateCardDealerBStatic.gameObject.SetActive(false);
        //dealerCanvasStatic.gameObject.SetActive(false);
        //UIManagerLite.EnableEndTurnButton();
    }

    /// <summary>
    /// Every player turn, force interaction with dealer
    /// before allowing end turn.
    /// </summary>
    void ITurnObserver.PlayerTurnInitiated()
    {
        dealerDealtWith = false;
        //initiateCardDealerButton.gameObject.SetActive(true);
    }
    void ITurnObserver.EnemyTurnInitiated()
    {
        // Make sure end turn button is disabled before shown to player.
        // For a moment its disabled if placed in function above.
        //UIManagerLite.DisableEndTurnButton();
    }
}
