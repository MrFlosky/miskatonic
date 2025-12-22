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
    static public bool dealerDealtWith { get; private set; }

    /// <summary>
    /// Text for reroll, including number left.
    /// </summary>
    [SerializeField] private TextMeshPro rerollTxt;

    /// <summary>
    /// Text displayed when player has option to purchase rerolls with
    /// cultist points.
    /// </summary>
    private string cultistPurchaseText;

    /// <summary>
    /// Text displayed when player hasnt used their per-turn reroll and or
    /// doesn't have cultpoint reroll option unlocked.
    /// </summary>
    private string defaultRerollText;

    /// <summary>
    /// Text displayed when player all out of rerolls and cannot purchase any
    /// if they have cultpoint reroll unlocked.
    /// </summary>
    private string outOfRerollsText;

    /// <summary>
    /// Whether or not player has ability to reroll at the cost of cultists.
    /// </summary>
    static private bool cultistRerollUnlocked;

    /// <summary>
    /// Cultist cost to purchase a reroll
    /// </summary>
    private int cultistRerollCost = 1;

    /// <summary>
    /// True if player went to dealer at least once this turn.
    /// Stops code that should execute only on first dealer
    /// interaction per turn.
    /// </summary>
    private bool dealerFirstTurnInteraction;
    private bool neverInteractedWithDealer = true;


    /// <summary>
    /// Number of rerolls available to player.
    /// </summary>
    int rerolls = 1;

    void Awake()
    {
        cultistPurchaseText = "SACRIFICE " + cultistRerollCost + " CULTIST FOR REROLL";
        defaultRerollText = "Reroll";
        outOfRerollsText = "OUT OF REROLLS";
        rerollTxt.text = defaultRerollText;
        dealerFirstTurnInteraction = false;
        
        // static variables reset
        dealerDealtWith = false;
        cultistRerollUnlocked = false;
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

        if (neverInteractedWithDealer)
        {
            neverInteractedWithDealer = false;
            
        }


        //UIManagerLite.TransitionToState(UIManagerLite.GameState.Dispatch); in clickAI now
        if(dealerFirstTurnInteraction == false)
        {
            dealerFirstTurnInteraction = true;
            MissionCollection.S.DISPLAY_AVAILABLE_MISSIONS();
        }
    }

    // <summary>
    // Called by missions:
    // After a player has chosen a mission, deactive canvas and allow them to end turn.
    // </summary>
    static public void DealingOver()
    {
        //UIManagerLite.TransitionToState(UIManagerLite.GameState.BoardBase);
        dealerDealtWith = true;
        MissionCollection.S.HIDE_ALL_MISSIONS();
    }

    /// <summary>
    /// Called upon reroll button press. Attempt to purchase with cultist points if ability is unlocked. 
    /// If not or the player hasnt used their free reroll, reroll as usual.
    /// </summary>
    public void Reroll()
    {
        AttemptCPReroll();

        if (rerolls > 0)
        {
            rerolls -= 1;
            MissionCollection.S.Reroll();

            // update text to cultist cost
            if (cultistRerollUnlocked && rerolls == 0 && CultistPoints.GetPoints() > 0) // gross if statements, need to clean this later - jay
            {
                rerollTxt.text = cultistPurchaseText;
                rerollTxt.fontSize = 15;
            }
            else if ((!cultistRerollUnlocked && rerolls == 0) || (cultistRerollUnlocked && CultistPoints.GetPoints() == 0))
            {
                rerollTxt.fontSize = 32;
                rerollTxt.text = outOfRerollsText;
            }
        }
    
    }

    /// <summary>
    /// Attempt to grant a reroll at the cost of cultists if ability unlocked and dont already have
    /// a rereoll.
    /// </summary>
    private void AttemptCPReroll()
    {
        if (rerolls > 0 || !cultistRerollUnlocked) { return; } // dont need a cp purchase if have enough rerolls to begin with

        if (CultistPoints.AttemptRerollPurchase(cultistRerollCost))
        {
            rerolls += 1;
        }
    }
    
    /// <summary>
    /// Called by reward when this mission type is completed. Flips bool that allows
    /// player to start buying rerolls with cultist points every turn.
    /// </summary>
    static public void UnlockRerollSacrifice()
    {
        cultistRerollUnlocked = true;
    }

    /// <summary>
    /// Every player turn, force interaction with dealer
    /// before allowing end turn.
    /// </summary>
    void ITurnObserver.PlayerTurnInitiated()
    {
        dealerDealtWith = false;
        dealerFirstTurnInteraction = false;
        // add reroll and update text
        rerolls = 1;
        rerollTxt.text = defaultRerollText;
        rerollTxt.fontSize = 36;
        //initiateCardDealerButton.gameObject.SetActive(true);
    }
    void ITurnObserver.EnemyTurnInitiated()
    {
        // Make sure end turn button is disabled before shown to player.
        // For a moment its disabled if placed in function above.
        //UIManagerLite.DisableEndTurnButton();
    }
}
