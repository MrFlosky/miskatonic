using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class SkillAoeFear : Skill
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
                    MazeCellObject cellHit = hit.transform.GetComponent<MazeCellObject>();
                    GetSurroundingCells(cellHit);
                    
                    
                }
            }
        }
    }

    /// <summary>
    /// We might refactor this later so not going based on gameob name, instead a var in mazecell ob would be better
    /// </summary>
    /// <param name="selectedCell"></param>
    void GetSurroundingCells(MazeCellObject selectedCell)
    {
        var match = Regex.Match(selectedCell.gameObject.name, @"\[(\d+),(\d+)\]");
        if (match.Success)
        {
            int x = int.Parse(match.Groups[1].Value);
            int y = int.Parse(match.Groups[2].Value);
            var coordinates = (x, y);
            
            Debug.Log($"Extracted coordinates: ({coordinates.x}, {coordinates.y})");
        }


        GameObject parentRoom = selectedCell.transform.parent.gameObject;
        foreach(MazeCellObject cell in parentRoom.GetComponentsInChildren<MazeCellObject>())
        {


        }

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
