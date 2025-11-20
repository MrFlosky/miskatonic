using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SkillPsychosis : Skill
{
    public Camera mainCamera;
    public int roundsLast = 3;
    public int fearPerNearby = 5;     // how much fear gained per nearby character
    public int visionRadius = 20;
    private bool isSelecting = false;

    protected override void Awake()
    {
        base.Awake();
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }
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

            // We don�t need layer filtering � just check if the clicked object has PlayerVision
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
            {
                PlayerVision vision = hit.collider.GetComponentInParent<PlayerVision>();
                if (vision != null && !vision.isPsychosis)
                {
                    ApplyPsychosis(vision);
                    isSelecting = true;
                    this.enabled = false;
                    StartCoroutine(EndSkillWaitFrame());
                }
            }
        }

    }
    
    /// <summary>
    /// SPECIFICALLY FOR PYSCHOSIS SINCE CLICKING ON AI,
    /// wait until end of frame to end the skill. This prevents transitioning to
    /// board base state too early allowing dialogue click event to happen in same frame.
    /// </summary>
    IEnumerator EndSkillWaitFrame()
    {
        yield return new WaitForEndOfFrame();
        EndSkill();
    }


    public void ApplyPsychosis(PlayerVision vision)
    {
        Debug.Log("Psychosis applied");
        vision.EnablePsychosisVision(true, TurnBasedSystem.GET_TURN_COUNT() + roundsLast, visionRadius, fearPerNearby);
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
        this.enabled = false;
        isSelecting = false;
        base.EndSkill();

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
