using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Class that implements the swap room skill.
/// </summary>
public class SkillSwapRooms : Skill
{
    [Header("Material of a selected room")]
    public Material selectedMat;

    /// Type of first room selected and its object.
    string mRoomType1 = "None";
    GameObject rm1Obj;

    /// Type of second room selected
    string mRoomType2 = "None";
    GameObject rm2Obj;

    // Start is called before the first frame update
    void Start()
    {
        st = SkillType.Skill1;
    }

    
    // Checks the rooms clicked.
    void Update()
    {
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
                    //Debug.Log(roomType);
                    if(mRoomType1 == "None")
                    {
                        rm1Obj = roomHit;
                        mRoomType1 = selectedRoomType;
                        SetRoomColor(rm1Obj, selectedMat);
                        
                    } else if(mRoomType2 == "None")
                    {
                        rm2Obj = roomHit;
                        mRoomType2 = selectedRoomType;
                        SetRoomColor(rm2Obj, selectedMat);
                        SwitchRooms(rm1Obj, rm2Obj, mRoomType1, mRoomType2);
                    }

                }
            }
        }
        
    }

    private void SwitchRooms(GameObject room1Ob, GameObject room2Ob, string rm1Type, string rm2Type)
    {
        if(rm1Type != "None" && rm2Type != "None")
        {
            foreach(MazeCellObject cell in room1Ob.GetComponentsInChildren<MazeCellObject>())
            {
                cell.roomType = rm2Type;
            }

            foreach(MazeCellObject cell in room2Ob.GetComponentsInChildren<MazeCellObject>())
            {
                cell.roomType = rm1Type;
            }

            mRoomType1 = "None";
            mRoomType2 = "None";
        }

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

    }
}
