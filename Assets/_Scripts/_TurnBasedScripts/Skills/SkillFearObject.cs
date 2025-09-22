using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillFearObject : Skill
{
    public Camera mainCamera;
    public GameObject fearObj;
    Transform fearObjTrans;

    
    private SkillsCollection skillsUI;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }
        fearObjTrans = fearObj.transform;
        skillsUI = GetComponent<SkillsCollection>();
        skillSystem = GetComponent<SkillSystem>();
    }


    private void Start()
    {
        enabled = false;
    }


    private void Update()
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Bound object to being placed in maze
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        // Note this is hitting the floor.
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.GetComponentInParent<MazeCellObject>() != null)
            {
               
                Vector3 newPosition = hit.transform.position;
                
                fearObjTrans.position = newPosition;

            }
        }


    }



    public void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Determine the rooms being selected for a switch
        Ray ray = skillSystem.GET_CAM().ScreenPointToRay(mousePos);
        RaycastHit hit;
       if (Physics.Raycast(ray, out hit))
        {

        }

    }



    public override void EndSkill()
    {

        base.EndSkill();
        this.enabled = false;

        fearObj.SetActive(false);
       

    }


    public override void ActivateSkill()
    {
        if (skillSystem.GET_PTS() >= ptDeduction)
        {
            base.ActivateSkill();
            enabled = true; // Enable the script allowing this skill to happen once.
            fearObj.SetActive(true);
           
        }

    }


    public void OnRightClick(InputAction.CallbackContext context)
    {
        ExitInput();
    }

}
