using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Class that implements the swap room skill.
/// </summary>
public class SkillSwapRooms : Skill
{
    ///***DEBUG 
    public TextMeshProUGUI txt;

    [Header("Material of a selected room")]
    public Material selectedMat;

    /// The material of the cell before change
    private Material beforeSelectMat;

    /// Type of first room selected and its object.
    string mRoomType1 = "None";
    GameObject rm1Obj;

    /// Type of second room selected
    string mRoomType2 = "None";
    GameObject rm2Obj;

    /// If 2 rooms have been selected
    bool switchCanOccur = false;

    // Start is called before the first frame update
    void Start()
    {
        st = SkillType.Skill1;
    }

    
    // Checks the rooms clicked.
    void Update()
    {
        // Determines the rooms being selected for a switch.
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = SkillSystem.GET_CAM().ScreenPointToRay(Input.mousePosition); 
            RaycastHit hit;
            // Note this is hitting the floor.
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.transform.GetComponentInParent<MazeCellObject>() != null)
                {
                    MazeCellObject cellHit = hit.transform.GetComponentInParent<MazeCellObject>(); // The single cell
                    GameObject roomHit = cellHit.transform.parent.gameObject; // The entire room holding all the cells
                    string selectedRoomType = cellHit.roomType;

                    Debug.Log(cellHit.roomType  + " HI");
                    SelectionCheck(selectedRoomType, roomHit, cellHit);
                }
            }
        }

        // Revert your last selection
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(mRoomType2 != "None")
            {
                mRoomType2 = "None";
                SetRoomColor(rm2Obj, beforeSelectMat);

            } else if(mRoomType1 != "None")
            {
                mRoomType1 = "None";
                SetRoomColor(rm1Obj, beforeSelectMat);
            }
        }

        // With two rooms selected, initiate the switch upon pressing enter.
        if(switchCanOccur && Input.GetKeyDown(KeyCode.Return))
        {
            SwitchRooms(rm1Obj, rm2Obj, mRoomType1, mRoomType2);
            this.enabled = false; // Disable this skill after it has been used once.
        }

    }

    /// <summary>
    /// If the roomType for room1 has not been set, set it and indicate it as selected.
    /// Then, do the same for room2 therefore indicating a room switch may occur.
    /// </summary>
    /// <param name="selectedType"></param>
    /// <param name="roomHit"></param>
    /// <param name="cellHit"></param>
    private void SelectionCheck(string selectedType, GameObject roomHit, MazeCellObject cellHit)
    {
        if(mRoomType1 == "None")
        {
            beforeSelectMat = cellHit.roomFloorMaterial;
            rm1Obj = roomHit;
            mRoomType1 = selectedType;
            SetRoomColor(rm1Obj, selectedMat);
                        
        } else if(mRoomType2 == "None")
        {
            rm2Obj = roomHit;
            mRoomType2 = selectedType;
            SetRoomColor(rm2Obj, selectedMat);
            switchCanOccur = true;
        }
    }

    /// <summary>
    /// Initiate the room swap, changing their types.
    /// Once the swap is complete their original materials are restored and
    /// values are reset for the next swap.
    /// </summary>
    /// <param name="room1Ob"></param>
    /// <param name="room2Ob"></param>
    /// <param name="rm1Type"></param>
    /// <param name="rm2Type"></param>
    private void SwitchRooms(GameObject room1Ob, GameObject room2Ob, string rm1Type, string rm2Type)
    {
            foreach(MazeCellObject cell in room1Ob.GetComponentsInChildren<MazeCellObject>())
            {
                cell.roomType = rm2Type;
            }

            foreach(MazeCellObject cell in room2Ob.GetComponentsInChildren<MazeCellObject>())
            {
                cell.roomType = rm1Type;
            }

            ///***DEBUG
            txt.text = "SWAP BETWEEN " + "\n" + rm1Type + " AND " + rm2Type + "\n SUCCESSFUL"; 

            // Reset the room types being held for switch
            mRoomType1 = "None";
            mRoomType2 = "None";
            switchCanOccur = false;

            // Set the rooms back to the original material
            SetRoomColor(rm1Obj, beforeSelectMat);
            SetRoomColor(rm2Obj, beforeSelectMat);
    }

    private void SetRoomColor(GameObject room, Material roomMaterial)
    {
        foreach(MazeCellObject cell in room.GetComponentsInChildren<MazeCellObject>())
        {
            cell.roomFloorMaterial = roomMaterial;
            cell.roomWallMaterial = roomMaterial;
            cell.RefreshMaterial();
        }
    }

    public override void ActivateSkill()
    {
        base.ActivateSkill();
        this.enabled = true; // Enable the script allowing this skill to happen once.

    }
}
