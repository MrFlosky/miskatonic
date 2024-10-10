using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBarrier : Skill
{
    [Header("Material of a possible barrier wall")]
    public Material selectedMat;

    public int roundsLast = 3;

    // Dont mind these...prob need refactoring
    List<Transform> mInactiveWalls = new List<Transform>();
    Transform lastWall; // last wall set
    bool canCycle = false;
    int cycleCount = 0;

    void Update()
    {
        // Determines the rooms being selected for a switch.
        if(Input.GetMouseButtonDown(0) && !canCycle)
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
                    canCycle = true;
                }
            }
        }

        //Cycle between barriers that can be placed
        if(Input.GetMouseButtonDown(0) && canCycle)
        {
            Debug.Log("hi");
            foreach(Transform wall in mInactiveWalls)
            {
                wall.gameObject.SetActive(false);
            }
            mInactiveWalls[cycleCount].gameObject.SetActive(true);

            if(cycleCount < mInactiveWalls.Count - 1)
            {
                cycleCount ++;
            } else
            {
                cycleCount = 0;
            }
        }

        // Confirm selection ending this skill.Reset vals
        if(Input.GetKey(KeyCode.Return) && canCycle)
        {
            this.enabled = false;
            //                                          Testing this next line for destroy after lifetime. should be last wall set. Might be easier to set new var above after mactivate[]/.gam
            if(cycleCount == 0)
            {
                TurnBasedSystem.DestroyAfterRounds destroyInstance = new TurnBasedSystem.DestroyAfterRounds(roundsLast, mInactiveWalls[cycleCount].gameObject );
            } else 
            {
                TurnBasedSystem.DestroyAfterRounds destroyInstance = new TurnBasedSystem.DestroyAfterRounds(roundsLast, mInactiveWalls[cycleCount - 1].gameObject );
            }

            cycleCount = 0;
            canCycle = false;
        }

        // Change cell selection. Set all walls back to false and allow reselect.
        if(Input.GetKeyDown(KeyCode.Escape) && canCycle)
        {
            foreach(Transform wall in mInactiveWalls)
            {
                wall.gameObject.SetActive(false);
            }

            canCycle = false;
            cycleCount = 0;
        }
    }
    

    /// <summary>
    /// Get the inactive walls apart of the selected cell.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    void GetPossibleBarrierWalls(Transform cell)
    {
        // This is important for subsequent calls so walls dont stay in the array
        mInactiveWalls.Clear();

        int wallCount = 4;
        int i = 0;
        foreach(Transform child in cell)
        {
            if(child.gameObject.activeSelf == false) // regardeless of parent state
            {
                Renderer childMat = child.GetComponent<Renderer>();
                childMat.material = selectedMat;

                mInactiveWalls.Add(child);
                Debug.Log(child.name);
                i++;
            }
        }
    }

    public override void ActivateSkill()
    {
        base.ActivateSkill();
        this.enabled = true; // Enable the script allowing this skill to happen once.
        
    }


}
