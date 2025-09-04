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
        skillSystem = GetComponent<SkillSystem>();
        
    }
    
    private void Start()
    {
        enabled = false;
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
        // Get the pointer position from the InputAction
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Determine the rooms being selected for a switch
        Ray ray = skillSystem.GET_CAM().ScreenPointToRay(mousePos);
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