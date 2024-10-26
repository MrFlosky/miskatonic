using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class that implements the swap room skill.
/// </summary>
public class SkillSwapRooms : Skill
{
    public TextMeshProUGUI txt;

    [Header("Material of a selected room")]
    public Material selectedMat;

    private Material beforeSelectMatFloor, beforeSelectMatWall;
    private string mRoomType1 = "None";
    private GameObject rm1Obj;
    private string mRoomType2 = "None";
    private GameObject rm2Obj;
    private bool switchCanOccur = false;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Click.performed += OnClick; // Subscribe to left-click action
        inputActions.Player.RightClick.performed += OnRightClick; // Subscribe to right-click action
        inputActions.Player.Confirm.performed += OnConfirm; // Subscribe to Enter key action
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClick;
        inputActions.Player.RightClick.performed -= OnRightClick;
        inputActions.Player.Confirm.performed -= OnConfirm;
        inputActions.Disable();
    }
    

    public void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Determine the rooms being selected for a switch
        Ray ray = SkillSystem.GET_CAM().ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            MazeCellObject cellHit = hit.transform.GetComponentInParent<MazeCellObject>();
            if (cellHit != null && cellHit.roomType != "Hallway")
            {
                GameObject roomHit = cellHit.transform.parent.gameObject;
                string selectedRoomType = cellHit.roomType;

                // Check if the selected room is already room1 or room2 to avoid reselecting the same room
                if (roomHit == rm1Obj || roomHit == rm2Obj)
                {
                    return;
                }

                // Select rooms
                SelectionCheck(selectedRoomType, roomHit, cellHit);
            }
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        // Ensure the action is only processed once when the right-click is performed
        if (context.phase != InputActionPhase.Performed)
            return;

        // Execute only one condition per right-click
        if (mRoomType2 != "None")
        {
            Debug.Log("removed room 2");
            mRoomType2 = "None";
            SetRoomColor(rm2Obj, beforeSelectMatFloor, beforeSelectMatWall);
            rm2Obj = null;
            switchCanOccur = false;
            return;
        }
        if (mRoomType1 != "None")
        {
            Debug.Log("removed room 1");
            mRoomType1 = "None";
            SetRoomColor(rm1Obj, beforeSelectMatFloor, beforeSelectMatWall);
            rm1Obj = null;
            return;
        }
        base.ExitInput();
        EndSkill();
        
    }




    public void OnConfirm(InputAction.CallbackContext context)
    {
        // Initiate the room switch when Enter is pressed and two rooms are selected
        if (switchCanOccur)
        {
            SwitchRooms(rm1Obj, rm2Obj, mRoomType1, mRoomType2);
            this.enabled = false; // Disable this skill after use
            SkillsCollection.ENABLE_BUTTONS();
        }
    }

    public override void EndSkill()
    {
        base.EndSkill();
        this.enabled = false;

        // Reset the room types being held for switch
        mRoomType2 = "None";
        if(rm2Obj != null){SetRoomColor(rm2Obj, beforeSelectMatFloor, beforeSelectMatWall);}

        mRoomType1 = "None";
        if(rm1Obj != null){SetRoomColor(rm1Obj, beforeSelectMatFloor, beforeSelectMatWall);}


        switchCanOccur = false;

    }

    private void SelectionCheck(string selectedType, GameObject roomHit, MazeCellObject cellHit)
    {
        if (mRoomType1 == "None")
        {
            beforeSelectMatFloor = cellHit.roomFloorMaterial;
            beforeSelectMatWall = cellHit.roomWallMaterial;
            rm1Obj = roomHit;
            mRoomType1 = selectedType;
            SetRoomColor(rm1Obj, selectedMat, selectedMat);
        }
        else if (mRoomType2 == "None")
        {
            rm2Obj = roomHit;
            mRoomType2 = selectedType;
            SetRoomColor(rm2Obj, selectedMat, selectedMat);
            switchCanOccur = true;
        }
    }

    private void SwitchRooms(GameObject room1Ob, GameObject room2Ob, string rm1Type, string rm2Type)
    {
        // Get the GameManager instance
        GameManager gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("GameManager instance is null.");
            return;
        }

        // Perform the room swap using GameManager
        gameManager.SwapRooms(room1Ob, rm1Type, room2Ob, rm2Type);

        // Update the UI to show the swap success
        txt.text = "SWAP BETWEEN " + "\n" + rm1Type + " AND " + rm2Type + "\n SUCCESSFUL";

        // Reset the room types and objects
        mRoomType1 = "None";
        mRoomType2 = "None";
        rm1Obj = null;
        rm2Obj = null;
        switchCanOccur = false;

        // Set the original materials back for the rooms
        SetRoomColor(room1Ob, beforeSelectMatFloor, beforeSelectMatWall);
        SetRoomColor(room2Ob, beforeSelectMatFloor, beforeSelectMatWall);
    }


    private void SetRoomColor(GameObject room, Material floorMaterial, Material wallMaterial)
    {
        foreach (MazeCellObject cell in room.GetComponentsInChildren<MazeCellObject>())
        {
            cell.roomFloorMaterial = floorMaterial;
            cell.roomWallMaterial = wallMaterial;
            cell.RefreshMaterial();
        }
    }

    public override void ActivateSkill()
    {
        if (SkillSystem.GET_PTS() >= ptDeduction)
        {
            base.ActivateSkill();
            this.enabled = true; // Enable the script allowing this skill to happen once
        }
    }
}
