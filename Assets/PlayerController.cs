using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; } // Singleton instance

    private PlayerInputActions inputActions;
    private ClickAI clickAI;
    public Skill activeSkill; // Reference to the currently active skill
    public bool clickedAI = false;

    private Bookshelf bookshelf;
    private void Awake()
    {
        // Ensure there is only one instance of PlayerController
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        inputActions = new PlayerInputActions();
        clickAI = GetComponent<ClickAI>(); // Assumes ClickAI is on the same GameObject
        bookshelf = Bookshelf.S;
    }

    private void OnEnable()
    {
        inputActions.Player.Click.started += OnClick;
        inputActions.Player.Scroll.started += OnScroll;
        inputActions.Player.RightClick.started += OnRightClick;
        inputActions.Player.RightClick.performed += OnRightClick;
        inputActions.Player.RightClick.canceled += OnRightClick;
        inputActions.Player.Confirm.started += OnConfirm; // Add the Confirm action
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Click.started -= OnClick;
        inputActions.Player.Scroll.started -= OnScroll;
        inputActions.Player.RightClick.started -= OnRightClick;
        inputActions.Player.RightClick.performed -= OnRightClick;
        inputActions.Player.RightClick.canceled -= OnRightClick;
        
        inputActions.Player.Confirm.started -= OnConfirm; // Remove the Confirm action
        inputActions.Disable();
    }

    public void SetActiveSkill(Skill newSkill)
    {
        // Enable the selected skill
        activeSkill = newSkill;
        if (newSkill) activeSkill.enabled = true;
    }

    private void OnClick(InputAction.CallbackContext context)
    {

        

        // Use the currently active skill
        if (activeSkill != null && activeSkill.enabled)
        {
            switch (activeSkill)
            {
                case SkillFear skillFear:
                    skillFear.OnClick(context);
                    break;
                case SkillAoeFear skillAoeFear:
                    skillAoeFear.OnClick(context);
                    break;
                case SkillSwapRooms skillSwapRooms:
                    skillSwapRooms.OnClick(context);
                    break;
                case SkillFearTotem skillFearTotem:
                    skillFearTotem.OnClick(context);
                    break;
                case SkillBarrier skillBarrier:
                    skillBarrier.OnClick(context);
                    break;
                case SkillPsychosis skillPsychosis:
                    skillPsychosis.OnClick(context);
                    break;
            }



        }

        // Forward the click input to the ClickAI script
        if (clickAI != null && activeSkill == null)
        {
            clickAI.OnClick(context);
        }

        //Lyn's note to self: COME UP WITH SOME VARIABLE TO CHECK SO THIS ISNT RUNNING EVERY TIME!!!
        if (true)
        {
            
            bookshelf.OnClick(context);
        }
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        // Forward the scroll input to the ClickAI script
        if (clickAI != null)
        {
            clickAI.OnScroll(context);
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        // Use the currently active skill
        if (activeSkill != null && activeSkill.enabled && context.started)
        {
            switch (activeSkill)
            {
                case SkillAoeFear skillAoeFear:
                    skillAoeFear.OnRightClick(context);
                    break;
                case SkillSwapRooms skillSwapRooms:
                    skillSwapRooms.OnRightClick(context);
                    break;
                case SkillBarrier skillBarrier:
                    skillBarrier.OnRightClick(context);
                    break;
                case SkillFearTotem fearTotem:
                    fearTotem.OnRightClick(context);
                    break;
                case SkillPsychosis skillPsychosis:
                    skillPsychosis.OnRightClick(context);
                    break;
            }
        }
        
        if (clickAI != null && activeSkill == null)
        {
            clickAI.OnRightClick(context);
        }
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        // Use the currently active skill for confirm action
        if (activeSkill != null && activeSkill.enabled)
        {
            switch (activeSkill)
            {
                case SkillSwapRooms skillSwapRooms:
                    skillSwapRooms.OnConfirm(context);
                    break;
                case SkillBarrier skillBarrier:
                    skillBarrier.OnConfirm(context);
                    break;
            }
        }
    }
}
