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

    private void Start()
    {
        enabled = false;
    }

    // Input subscription handled by PlayerContoller for all skills, useless here.
    // private void OnEnable()
    // {
    //     inputActions.Player.Click.performed += OnClick; // Subscribe to left-click action
    //     inputActions.Player.RightClick.performed += OnRightClick; // Subscribe to right-click action
    //     inputActions.Player.Confirm.performed += OnConfirm; // Subscribe to Enter key action
    //     inputActions.Enable();
    // }
    // private void OnDisable()
    // {
    //     inputActions.Player.Click.performed -= OnClick;
    //     inputActions.Player.RightClick.performed -= OnRightClick;
    //     inputActions.Player.Confirm.performed -= OnConfirm;
    //     inputActions.Disable();
    // }


    public void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Determine the rooms being selected for a switch
        Ray ray = skillSystem.GET_CAM().ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
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
                AudioManager.PlaySFX(SFXType.MouseButtonHit);
            }
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        // I thought we needed one of these if statements but apparently not!

        // Ensure the action is only processed once when the right-click is performed
        // if (context.phase != InputActionPhase.Performed)
        //     return;
        // if (inputActions.Player.RightClick.phase == InputActionPhase.Performed)
        // {
        //     return;
        // }

        // Execute only one condition per right-click
        if (mRoomType2 != "None")
        {
            Debug.Log("removed room 2");
            mRoomType2 = "None";
            SetRoomColor(rm2Obj, beforeSelectMatFloor, beforeSelectMatWall);
            rm2Obj = null;
            switchCanOccur = false;
            AudioManager.PlaySFX(SFXType.MouseButtonHit);
            return;
        }
        if (mRoomType1 != "None")
        {
            Debug.Log("removed room 1");
            mRoomType1 = "None";
            SetRoomColor(rm1Obj, beforeSelectMatFloor, beforeSelectMatWall);
            rm1Obj = null;
            AudioManager.PlaySFX(SFXType.MouseButtonHit);
            return;
        }

        base.ExitInProgress();
    }




    public void OnConfirm(InputAction.CallbackContext context)
    {
        // Initiate the room switch when Enter is pressed and two rooms are selected
        if (switchCanOccur)
        {
            PlayerController.Instance.SetActiveSkill(null);
            AudioManager.PlaySFX(SFXType.SkillRoomswap);
            SwitchRooms(rm1Obj, rm2Obj, mRoomType1, mRoomType2);
            this.enabled = false; // Disable this skill after use
            SkillsCollection.S.ENABLE_BUTTONS();
            EndSkill();
        }
    }

    public override void EndSkill()
    {
        base.EndSkill();
        this.enabled = false;

        // Reset the room types being held for switch
        mRoomType2 = "None";
        if (rm2Obj != null) { SetRoomColor(rm2Obj, beforeSelectMatFloor, beforeSelectMatWall); }

        mRoomType1 = "None";
        if (rm1Obj != null) { SetRoomColor(rm1Obj, beforeSelectMatFloor, beforeSelectMatWall); }


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
        SwapRooms(room1Ob, rm1Type, room2Ob, rm2Type);        

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
        
        // If it works it works :)
        // Should prob cache the agents but will maybeee do later 
        AgentNavigation[] agents = FindObjectsOfType<AgentNavigation>();
        foreach (var agent in agents)
        {
            agent.OnRoomSwapped();
        }
        
        MapChangeEvents.InvokeRoomsSwapped();
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
        if (usagesRemaining >= usageDeduction)
        {
            base.ActivateSkill();
            this.enabled = true; // Enable the script allowing this skill to happen once
        }
    }




    /// <summary>
    /// Performs room swap skill between two rooms.
    /// </summary>
    public void SwapRooms(GameObject room1Ob, string rm1Type, GameObject room2Ob, string rm2Type)
    {
        //Find Original Swap Position
        Vector3 orginal10bposition = room1Ob.transform.Find("RoomCam(Clone)").position;
        Vector3 orginal20bposition = room2Ob.transform.Find("RoomCam(Clone)").position;

        // Swap the room positions in the scene
        Vector3 room1Position = room1Ob.transform.position;
        Vector3 room2Position = room2Ob.transform.position;

        //Swap Room Cameras
        room1Ob.transform.Find("RoomCam(Clone)").position = orginal20bposition;
        room2Ob.transform.Find("RoomCam(Clone)").position = orginal10bposition;

        // Swap the positions of the room GameObjects
        room1Ob.transform.position = room2Position;
        room2Ob.transform.position = room1Position;

        // Swap room types for the cells in the centralMaze
        foreach (MazeCellObject cell in room1Ob.GetComponentsInChildren<MazeCellObject>())
        {
            Vector2Int cellPosition = new Vector2Int(cell.GetXCoord(), cell.GetYCoord());
            GameManager.Instance.centralMaze[cellPosition.x, cellPosition.y].roomType = rm2Type;
        }

        foreach (MazeCellObject cell in room2Ob.GetComponentsInChildren<MazeCellObject>())
        {
            Vector2Int cellPosition = new Vector2Int(cell.GetXCoord(), cell.GetYCoord());
            GameManager.Instance.centralMaze[cellPosition.x, cellPosition.y].roomType = rm1Type;
        }

        // Swap tiles between rooms while maintaining corresponding positions
        SwapRoomTiles(room1Ob, room2Ob);
    }

    /// <summary>
    /// For every cell apart of 2 given rooms, swap their types.
    /// </summary>
    private void SwapRoomTiles(GameObject room1Ob, GameObject room2Ob)
    {
        var room1Type = room1Ob.GetComponentsInChildren<MazeCellObject>()[0].roomType;
        var room2Type = room2Ob.GetComponentsInChildren<MazeCellObject>()[0].roomType;

        // Organize the tiles into a 5x5 grid based on their local positions
        foreach (MazeCellObject cell in room1Ob.GetComponentsInChildren<MazeCellObject>())
        {
            cell.roomType = room2Type;
        }

        foreach (MazeCellObject cell in room2Ob.GetComponentsInChildren<MazeCellObject>())
        {
            cell.roomType = room1Type;
        }

        SwapRoomProps(room1Ob, room2Ob);
    }

    private void SwapRoomProps(GameObject room1Ob, GameObject room2Ob)
    {
        // Find the props in the rooms based on the "Prop" tag
        GameObject room1Prop = FindPropWithTag(room1Ob, "Prop");
        GameObject room2Prop = FindPropWithTag(room2Ob, "Prop");

        // Ensure the props and parents are valid
        if (room1Prop != null && room2Prop != null)
        {
            Transform room1Parent = room1Ob.transform;
            Transform room2Parent = room2Ob.transform;

            // Store the local positions before swapping
            Vector3 room1LocalPosition = room1Prop.transform.localPosition;
            Vector3 room2LocalPosition = room2Prop.transform.localPosition;

            // Swap the props between rooms
            room1Prop.transform.SetParent(room2Parent);
            room1Prop.transform.localPosition = room2LocalPosition;

            room2Prop.transform.SetParent(room1Parent);
            room2Prop.transform.localPosition = room1LocalPosition;
        }
    }
    
    private GameObject FindPropWithTag(GameObject parentObject, string tag)
    {
        foreach (Transform child in parentObject.transform)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        return null;
    }

}
