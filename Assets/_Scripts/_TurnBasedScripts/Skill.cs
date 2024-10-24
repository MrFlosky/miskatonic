using System.Collections;
using System.Collections.Generic;
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
public class Skill : MonoBehaviour
{
    [Header("The point deduction associated with this skill")]
    public int ptDeduction = 1;

    [Header("The selectable image associated with this skill")]
    public Button skillButton;

    [Header("Event associated with this skill")]
    public UnityEvent skillEvent;

    /// SkillType associated with this skill. Default to NeverSet for debugging.
    protected SkillType st = SkillType.NeverSet; 

    /// Holding all skillButtons
    [Header("Dont assign here")]
    static public Button[] buttonList = new Button[4];

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }
    

    void Update()
    {
        // You can put other update logic here if needed
    }

    /// <summary>
    /// Every skill can only be activated if the player has enough points.
    /// Upon activation, deduct its cost and invoke any event associated with
    /// this class. i.e. calls that should happen with all skills.
    /// </summary>
    public virtual void ActivateSkill()
    {
        if (SkillSystem.GET_PTS() >= ptDeduction) // Check if the player has enough points
        {
            PlayerController.Instance.SetActiveSkill(this);
            SkillSystem.DEDUCT_PT(ptDeduction);
            SkillsCollection.DISABLE_BUTTONS();
            skillEvent.Invoke(); // Not using this for now
            // Stuff that happens when skill is activated!
        }
    }

    // Each skill must perform different functions to end itself. Called on end turn!
    public virtual void EndSkill()
    {
        PlayerController.Instance.SetActiveSkill(null);
        SkillsCollection.ENABLE_BUTTONS();
    }

    /// <summary>
    /// Exit any skill on right click only if they are in middle of a skill.
    /// i.e., the buttons are not interactable. Give them their spent points back.
    /// </summary>
    protected virtual void ExitInput()
    {
        PlayerController.Instance.SetActiveSkill(null);
        if (!skillButton.interactable) // Check if the button is not interactable
        {
            SkillSystem.ADD_PT(ptDeduction);
            EndSkill();
        }
    }
}
