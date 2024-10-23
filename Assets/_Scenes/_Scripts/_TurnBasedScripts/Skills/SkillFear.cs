using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillFear : Skill
{
    public int fearIncrease = 5;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Click.performed += OnClick; // Subscribe to the Click action
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClick; // Unsubscribe to prevent memory leaks
        inputActions.Disable();
    }

    private void Update()
    {
        base.ExitInput();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        // Perform the raycast to detect the object under the mouse
        Ray ray = SkillSystem.GET_CAM().ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.GetComponentInParent<Fear>() != null)
            {
                Fear thisAIFear = hit.transform.GetComponent<Fear>();
                HandleIncrease(thisAIFear);
            }
        }
    }

    private void HandleIncrease(Fear fear)
    {
        fear.AddFear(fearIncrease);
        this.enabled = false; // Disable after single use
    }

    public override void ActivateSkill()
    {
        base.ActivateSkill();
        this.enabled = true; // Enable the script allowing this skill to happen once
    }
}