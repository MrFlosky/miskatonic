using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBarrier : Skill
{
    public int fearIncrease = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

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
                    Transform cellHit = hit.transform.parent; // The single cell
                    Debug.Log(cellHit.name);
                    GetPossibleBarrierWalls(cellHit);
                }
            }
        }
    }

    GameObject[] GetPossibleBarrierWalls(Transform room)
    {
        int wallCount = 4;
        GameObject [] inactiveWalls = new GameObject[wallCount]; // better to allocate an array than use vec 
        int i = 0;
        foreach(Transform child in room)
        {
            if(child.gameObject.activeSelf == false) // regardeless of parent state
            {
                inactiveWalls[i] = child.gameObject;
                //Debug.Log(child.name);
            }
            i++;
        }

        return inactiveWalls;
        
    }

    void HandleIncrease(Fear fear)
    {
        fear.AddFear(fearIncrease);
        this.enabled = false; // Disable after single use
    }

    public override void ActivateSkill()
    {
        base.ActivateSkill();
        this.enabled = true; // Enable the script allowing this skill to happen once.

    }


}
