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
    }

    private void OnEnable()
    {
        inputActions.Player.Click.performed += OnClick;
        inputActions.Player.Scroll.performed += OnScroll;
        inputActions.Player.RightClick.performed += OnRightClick;
        inputActions.Player.Confirm.performed += OnConfirm; // Add the Confirm action
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClick;
        inputActions.Player.Scroll.performed -= OnScroll;
        inputActions.Player.RightClick.performed -= OnRightClick;
        inputActions.Player.Confirm.performed -= OnConfirm; // Remove the Confirm action
        inputActions.Disable();
    }

    public void SetActiveSkill(Skill newSkill)
    {
        // Enable the selected skill
        activeSkill = newSkill;
        activeSkill.enabled = true;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        // Use the currently active skill
        if (activeSkill != null && activeSkill.enabled)
        {
            if (activeSkill is SkillFear skillFear)
            {
                skillFear.OnClick(context);
            }
            else if (activeSkill is SkillAoeFear skillAoeFear)
            {
                skillAoeFear.OnClick(context);
            }
            else if (activeSkill is SkillSwapRooms skillSwapRooms)
            {
                skillSwapRooms.OnClick(context);
            }
        }

        // Forward the click input to the ClickAI script
        if (clickAI != null && activeSkill == null)
        {
            clickAI.OnClick(context);
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
        if (activeSkill != null && activeSkill.enabled)
        {
            if (activeSkill is SkillAoeFear skillAoeFear)
            {
                skillAoeFear.OnRightClick(context);
            }
            else if (activeSkill is SkillSwapRooms skillSwapRooms)
            {
                skillSwapRooms.OnRightClick(context);
            }
            // Add more checks if there are other skill types
        }
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        // Use the currently active skill for confirm action
        if (activeSkill != null && activeSkill.enabled)
        {
            if (activeSkill is SkillSwapRooms skillSwapRooms)
            {
                skillSwapRooms.OnConfirm(context);
                activeSkill = null;
            }
            // Add more checks if there are other skill types
        }
    }
}
