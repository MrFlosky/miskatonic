using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallType
{
    Top, Bottom, Left, Right
}
public class SkillBarrier : Skill
{
    [Header("Material of a possible barrier wall")]
    public Material selectedMat;

    public int roundsLast = 3;

    /// List holding the inactive walls i.e. those that can be cycled through
    List<Transform> mInactiveWalls = new List<Transform>();
    /// Index of last wall cycled to by player.
    int lastWallIndex; 
    /// Whether or not player is able to cycle through wall options.
    bool canCycle = false;
    /// Number of wall cycles player has gone through. 
    int cycleCount = 0;

    bool cellWasPressed = false; //fix bug that can confirm not existent wall

    /// Cell hit by raycast
    Transform cellHit;

    void Update()
    {
        base.ExitInput();
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
                    cellHit = hit.transform.parent; // The single cell
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
            lastWallIndex = cycleCount;

            if(cycleCount < mInactiveWalls.Count - 1)
            {
                cycleCount ++;
            } else
            {
                cycleCount = 0;
            }
        }

        // Confirm selection ending this skill. Reset vals
        if(Input.GetKey(KeyCode.Return) && canCycle)
        {
            Debug.Log("YOOO");
            this.enabled = false;
            // Set the walls internal state within the maze array and have it destroy after given number of rounds.
            Transform confirmedWall = mInactiveWalls[lastWallIndex];
            MazeCellObject confirmedWallMz = cellHit.GetComponent<MazeCellObject>();
            int x = confirmedWallMz.GetXCoord();
            int y = confirmedWallMz.GetYCoord();
            WallType wT = GetWallType(cellHit, confirmedWall);

            MazeGenerator.Instance.SetWallInternalState(x, y, wT, true);


            // Create new instance of obj that will destroy this wall for us.
            TurnBasedSystem.DestroyAfterRounds destroyInstance = new TurnBasedSystem.DestroyAfterRounds(roundsLast, confirmedWall.gameObject, wT); // will need to make it so the wall is later disabled with funct in mazegen
            cycleCount = 0;
            canCycle = false;
            cellHit = null;
            SkillsCollection.ENABLE_BUTTONS();
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

        // how this skill ends itself on end turn
    public override void EndSkill()
    {
        //SkillsCollection.ENABLE_BUTTONS();

        base.EndSkill();
        this.enabled = false;

        // meaning player never confirmed their selection
        if(canCycle)
        {
        mInactiveWalls[lastWallIndex].gameObject.SetActive(false);
        }
        cycleCount = 0;
        canCycle = false;
        cellHit = null;
    }
    
  
    /// <summary>
    /// Get the inactive walls apart of the selected cell.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    /// 
    void GetPossibleBarrierWalls(Transform cell)
    {
        // This is important for subsequent calls so walls dont stay in the array
        mInactiveWalls.Clear();

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
        if(SkillSystem.GET_PTS() >= ptDeduction)
        {
        base.ActivateSkill();
        this.enabled = true; // Enable the script allowing this skill to happen once.
        }
    }

    
    /// <summary>
    /// Go through all walls/ceil/floor apart of the cell, and identify where a given
    /// wall is located within it. Dependent on consistent hierarchy index placement within cell.
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="WallToIdentify"></param>
    WallType GetWallType(Transform cell, Transform WallToIdentify)
    {
        int i = 0;
        foreach(Transform child in cell)
        {
            if (child == WallToIdentify)
            {
                break;
            }
            i++;
        }

        switch(i)
        {
            case 1:
            return WallType.Bottom;

            case 2:
            return WallType.Top;

            case 3:
            return WallType.Right;

            case 4:
            return WallType.Left;

            default:
            Debug.Log("that wasnt supposed to happen");
            return 0; // is this best to return if shouldnt occur?
        }
    }



}
