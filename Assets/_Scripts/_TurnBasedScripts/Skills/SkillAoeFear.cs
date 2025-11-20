using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillAoeFear : Skill
{
    public int roundsLast = 3;
    public Camera mainCamera; // Reference to the camera, if null defaults to main camera
    public GameObject AOEOb;
    Transform obTrans;
    bool canPlace = true;

    

    protected override void Awake()
    {
        base.Awake();
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }
        obTrans = AOEOb.GetComponent<Transform>();
    }

    void Start()
    {
        enabled = false;
    }

    void Update()
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Bound object to being placed in maze
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        // Note this is hitting the floor.
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
        {
            if (hit.transform.GetComponentInParent<MazeCellObject>() != null)
            {

                Vector3 newPosition = hit.transform.position;
                newPosition.x += 1f;
                newPosition.y = 0.5f;
                newPosition.z += 1f;
                obTrans.position = newPosition;

            }
        }
        else
        {
            obTrans.position = new Vector3(0, -1000, 0);
        }

    }

    public void OnClick(InputAction.CallbackContext context)
    {
                
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Determine the rooms being selected for a switch
        Ray ray = skillSystem.GET_CAM().ScreenPointToRay(mousePos);
        RaycastHit hit;
        // Note this is hitting the floor.
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
        {
            if (canPlace && hit.transform.GetComponentInParent<MazeCellObject>() != null)
            {
                AudioManager.PlaySFX(SFXType.SkillAOE);
                Vector3 newPos = obTrans.position;
                GameObject newAOE = Instantiate(AOEOb);
                newAOE.transform.position = newPos;
                TurnBasedSystem.DestroyAfterRounds destroyInstance = new TurnBasedSystem.DestroyAfterRounds(roundsLast, newAOE);
                AOEOb.SetActive(false);
                canPlace = false;
                this.enabled = false;
                EndSkill();
            }
        }
        
    }

    public override void EndSkill()
    {
 
        base.EndSkill();
        this.enabled = false;
        AOEOb.SetActive(false);
        canPlace = true;

    }


    public override void ActivateSkill()
    {
        if(usagesRemaining >= usageDeduction)
        {
            base.ActivateSkill(); // uses child instance members but parents function
            enabled = true; // Enable the script allowing this skill to happen once.
            AOEOb.SetActive(true);
            canPlace = true;
        }

    }
    
    /// <summary>
    /// Handles the right-click action to exit the skill if conditions are met.
    /// </summary>
    public void OnRightClick(InputAction.CallbackContext context)
    {
        ExitInProgress();
    }


}
