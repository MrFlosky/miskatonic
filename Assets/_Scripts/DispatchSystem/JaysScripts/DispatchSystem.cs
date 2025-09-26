using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for initiating and ending the dealing of cards.
/// </summary>
public class DispatchSystem : MonoBehaviour, ITurnObserver
{
    [SerializeField] private Button initiateCardDealerButton;
    static private Button initiateCardDealerBStatic;

    // Canvas that holds dealer canvas with cards
    [SerializeField] private Canvas dealerCanvas;
    // The reason I have this second variable here is so
    // Missions can call DealingOver without a assigned reference to this class.
    static private Canvas dealerCanvasStatic;

    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
        dealerCanvasStatic = dealerCanvas;
        initiateCardDealerBStatic = initiateCardDealerButton;
    }

    /// <summary>
    /// Called by dealer button press, enable dealer canvas and display available missions.
    /// </summary>
    public void DealCards()
    {
        dealerCanvas.gameObject.SetActive(true);
        MissionCollection.S.DISPLAY_AVAILABLE_MISSIONS();
    }

    /// <summary>
    /// Called by missions:
    /// After a player has chosen a mission, deactive canvas and allow them to end turn.
    /// </summary>
    static public void DealingOver()
    {
        initiateCardDealerBStatic.gameObject.SetActive(false);
        dealerCanvasStatic.gameObject.SetActive(false);
        //MissionCollection.S.HIDE_AVAILABLE_MISSIONS(); // this might be redundant
        UIManagerLite.EnableEndTurnButton();
    }

    /// <summary>
    /// Every player turn, force interaction with dealer
    /// before allowing end turn.
    /// </summary>
    void ITurnObserver.PlayerTurnInitiated()
    {
        
        initiateCardDealerButton.gameObject.SetActive(true);
    }
    void ITurnObserver.EnemyTurnInitiated()
    {
        // Make sure end turn button is disabled before shown to player.
        // For a moment its disabled if placed in function above.
        UIManagerLite.DisableEndTurnButton();
    }
}
