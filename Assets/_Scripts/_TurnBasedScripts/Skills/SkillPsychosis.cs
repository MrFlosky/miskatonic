using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SkillPsychosis : Skill
{
    public Camera mainCamera;
    private SkillsCollection skillsUI;
    public int roundsLast = 3;
    public int fearPerNearby = 5;     // how much fear gained per nearby character
    public int visionRadius = 20;
    private bool isSelecting = false;

    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }
        
        skillSystem = GetComponent<SkillSystem>();
        skillsUI = GetComponent<SkillsCollection>();
    }



    public void OnClick(InputAction.CallbackContext context)
    {
        Debug.Log("Psychosis onclicked");
        if (!isSelecting)
        {   
            Vector2 mousePos = Vector2.zero;
            mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
            // Determine the character being selected for psychosis

            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            RaycastHit hit;

            // We don’t need layer filtering — just check if the clicked object has PlayerVision
            if (Physics.Raycast(ray, out hit, 100f))
            {
                PlayerVision vision = hit.collider.GetComponentInParent<PlayerVision>();
                if (vision != null && !vision.isPsychosis)
                {
                    ApplyPsychosis(vision);
                    isSelecting = true;
                    skillsUI.ENABLE_BUTTONS();
                    EndSkill();
                }
            }
        }

    }


    public void ApplyPsychosis(PlayerVision vision)
    {
        Debug.Log("Psychosis applied");
        vision.EnablePsychosisVision(true, TurnBasedSystem.GET_TURN_COUNT() + roundsLast, visionRadius);
    }

    public void StartTargetSelection()
    {
        isSelecting = true;
        Debug.Log("Select a player to apply Psychosis...");
    }

    public override void ActivateSkill()
    {
        Debug.Log("Psychosis activated");
        if (usagesRemaining >= usageDeduction)
        {
            print("Activated");
            base.ActivateSkill();
            enabled = true; // Enable the script allowing this skill to happen once.

        }

    }

    public override void EndSkill()
    {

        base.EndSkill();
        this.enabled = false;
        isSelecting = false;

    }

    /// <summary>
    /// Handles the right-click action to exit the skill if conditions are met.
    /// </summary>
    public void OnRightClick(InputAction.CallbackContext context)
    {
        Debug.Log("right click");
        isSelecting = false;
        base.ExitInProgress();
    }


}
