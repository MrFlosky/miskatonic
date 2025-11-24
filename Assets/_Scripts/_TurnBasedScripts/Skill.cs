using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Base class of skills.
/// Skills will be responsible for activating themselves upon being their button being clicked.  
/// While PlayerTurn is responsible for displaying skill options by invoking function in 
/// SkillsCollection.
/// 
/// For the update with skill unlocks, clicking the button should allow player to use skill
/// so long as they have enough cultist points. Unlocking is handled by SkillUnlockReward.
/// </summary>
public abstract class Skill : MonoBehaviour
{

    /// <summary>
    /// Number of usage points deducted upon skill being used.
    /// </summary>
    //[Header("The point deduction associated with this skill")]
    protected int usageDeduction = 1;

    [Header("The selectable image associated with this skill")]
    public Button skillButton;

    /// <summary>
    /// Im keeping this variable with update to unlocks so that
    /// awarding usages is still supported.
    /// </summary>
    [Header("Number of usages player has remaining for this skill")]
    public int usagesRemaining;

    // end turn button, just hangin out as a loose variable
    public Button endTurnButton;

    // Text displaying usages remaining
    public TextMeshProUGUI usagesRemainTxt;

    /// <summary>
    /// Text displaying how many cultists required to use this skill
    /// </summary>
    public TextMeshProUGUI cultistCostTxt;

    [Header("Number of cultists required to purchase a usage of this skill")]
    [SerializeField] private int cultistCost;

    /// <summary>
    /// Whether or not this skill has been unlocked by the player via the dispatch system.
    /// </summary>
    private bool unlocked = false;

    /// <summary>
    /// Skills that use raycasts hits all layers besides this one.
    /// </summary>
    protected int raycastLayerMask;

    // For some reason the parent class, Skill, does not pass its reference to skillSystem as it should. Try
    // debugging in various way but this my best fix. Simply calling get component again to get skillSystem.
    protected SkillSystem skillSystem;


    /// <summary>
    /// All derived classes should call this base awake, override to expand upon it if needed.
    /// </summary>
    protected virtual void Awake()
    {
        skillSystem = GetComponent<SkillSystem>();
        raycastLayerMask = ~LayerMask.GetMask("NoCastyCastyCollider");
    }

    /// <summary>
    /// Every skill can only be activated if the player has enough points.
    /// Upon activation, deduct its cost and invoke any event associated with
    /// this class. i.e. calls that should happen with all skills.
    /// </summary>
    public virtual void ActivateSkill()
    {
        if (usagesRemaining >= usageDeduction) // Check if the player has enough points
        {
            AudioManager.PlaySFX(SFXType.ButtonSelection);
            PlayerController.Instance.SetActiveSkill(this); // THIS IS CHILD INSTANCE ON base.ActivateSkill() call in children classes

            // this should auto work with exit input, bc attempting to rebuy a skill after exiting will also re-increase 
            // usages
            usagesRemaining -= usageDeduction;

            TelemetryManager.S.CountSkillUsage(GetType().Name, 1);

            UIManagerLite.TransitionToState(UIManagerLite.GameState.SkillInUse);
            //SkillsCollection.S.DISABLE_BUTTONS(); old ui
            endTurnButton.interactable = false;
        } 
    }

    /// <summary>
    /// Upon clicking button associated with this skill,
    /// attempt to purchase a usage for it.
    /// 
    /// TODO: THIS SHOULD ALSO ALLOW FOR SKIP PURCHASE AND USAGE OF SKILL IF PLAYER HAS USAGES
    /// FROM SKILL USAGE REWARD CARDS
    /// </summary>
    public virtual void AttemptPurchase()
    {
        if (CultistPoints.GetPoints() >= cultistCost)
        {
            CultistPoints.DeductPonts(cultistCost);
            AddSkillUsage(1);
            ActivateSkill();
        } 
    }

    /// <summary>
    /// Called once upon player unlocking this skill.
    /// Sets skill button to active, SkillsCollection determines if it is interactable.
    /// </summary>
    public void UnlockSkill()
    {
        unlocked = true;
        skillButton.gameObject.SetActive(true);
        cultistCostTxt.gameObject.SetActive(true);
        UpdateCultistCostText();
    }

    /// <summary>
    /// Return whether or not this skill has any usages remaining.
    /// </summary>
    public bool HasUsages()
    {
        if (usagesRemaining > 0)
        {
            return true;
        }
        return false;
    }    

    /// <summary>
    /// Return whether or not this skill has been unlocked.
    /// </summary>
    public bool IsUnlocked()
    {
        return unlocked;
    }

    /// <summary>
    /// Returns whether this skill can be afforded, used by SkillsCollection
    /// to determine if a bunch should be interactable or not upon being turned on.
    /// </summary>
    public bool CanAffordUsage()
    {
        //Debug.LogError(CultistPoints.GetPoints() >= cultistCost || usagesRemaining > 0);

        return CultistPoints.GetPoints() >= cultistCost || usagesRemaining > 0;
    }


    /// <summary>
    /// Each skill must perform different functions to end itself.
    /// Called upon fully exiting skill usage (right click reverse or confirm) and transitions UI game state.
    /// </summary>
    public virtual void EndSkill()
    {
        PlayerController.Instance.SetActiveSkill(null);
        UIManagerLite.TransitionToState(UIManagerLite.GameState.BoardBase);
    }
    
    /// <summary>
    /// End skill without transition UI game state! 
    /// </summary>
    public virtual void EndSkillNaked()
    {
        PlayerController.Instance.SetActiveSkill(null);
    }

    /// <summary>
    /// Exit any skill on right click only IF they are in MIDDLE of a skill.
    /// i.e., the buttons are not interactable. Give them their spent points back.
    /// </summary>
    protected virtual void ExitInProgress()
    {
        if (!skillButton.interactable) // trick to see if in middle of skill usage
        {
            PlayerController.Instance.SetActiveSkill(null);
            AudioManager.PlaySFX(SFXType.ButtonSelection);
            CultistPoints.AddPointsStatic(cultistCost); // refund cultists if player exits skill usage
            TelemetryManager.S.CountSkillUsage(GetType().Name, -1); // uncount the usage
            EndSkill();
        }
    }

    /// <summary>
    /// Add to the number of usages available for this skill
    /// </summary>
    public void AddSkillUsage(int usage)
    {
        usagesRemaining += usage;
        usagesRemainTxt.text = "X " + usagesRemaining;
    }

    /// <summary>
    /// Update text for cultist cost.
    /// </summary>
    private void UpdateCultistCostText()
    {
        cultistCostTxt.text = "Cost: " + cultistCost;
    }
}
