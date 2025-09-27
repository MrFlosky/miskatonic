using System.Collections;
using System.Collections.Generic;
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
/// </summary>
public abstract class Skill : MonoBehaviour
{

    [Header("The point deduction associated with this skill")]
    public int ptDeduction = 1;

    [Header("The selectable image associated with this skill")]
    public Button skillButton;

    [Header("Event associated with this skill")]
    public UnityEvent skillEvent;

    [Header("Number of usages player has remaining for this skill")]
    public int usagesRemaining;

    // end turn button, just hangin out as a loose variable
    public Button endTurnButton;

    // Text displaying usages remaining
    public TextMeshProUGUI usagesRemainTxt;


    // For some reason the parent class, Skill, does not pass its reference to skillSystem as it should. Try
    // debugging in various way but this my best fix. Simply calling get component again to get skillSystem.
    protected SkillSystem skillSystem;

    private void Awake()
    {
        skillSystem = GetComponent<SkillSystem>();
    }

    /// <summary>
    /// Every skill can only be activated if the player has enough points.
    /// Upon activation, deduct its cost and invoke any event associated with
    /// this class. i.e. calls that should happen with all skills.
    /// </summary>
    public virtual void ActivateSkill()
    {
        if (usagesRemaining >= ptDeduction) // Check if the player has enough points
        {
            AudioManager.PlaySFX(SFXType.ButtonSelection);
            PlayerController.Instance.SetActiveSkill(this); // THIS IS CHILD INSTANCE ON base.ActivateSkill() call in children classes
            skillSystem.DEDUCT_PT(ptDeduction, this);
            SkillsCollection.S.DISABLE_BUTTONS();
            endTurnButton.interactable = false;
            skillEvent.Invoke(); // Not using this for now
            // Stuff that happens when skill is activated!
        }
    }

    // Each skill must perform different functions to end itself.
    //  Called on fully exiting skill end turn!
    public virtual void EndSkill()
    {
        // this means dealer was already DEALT with ahaha so we can
        // turn on the end turn button on skill cancel. 
        // dont gotta worry about this when dealer is already locking end turn.
        if (UIManagerLite.DealerButtonIsActive() == false)
        {
            endTurnButton.interactable = true;
            Debug.LogWarning("EFJEIWJ");
        }

        PlayerController.Instance.SetActiveSkill(null);
        SkillsCollection.S.ENABLE_BUTTONS();
    }

    /// <summary>
    /// Exit any skill on right click only if they are in middle of a skill.
    /// i.e., the buttons are not interactable. Give them their spent points back.
    /// </summary>
    protected virtual void ExitInput()
    {
        if (!skillButton.interactable) // Check if the button is not interactable
        {
            PlayerController.Instance.SetActiveSkill(null);
            AudioManager.PlaySFX(SFXType.ButtonSelection);
            skillSystem.ADD_PT(ptDeduction, this);
            EndSkill();
        }
    }
}
