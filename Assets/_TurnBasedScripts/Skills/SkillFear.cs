using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillFear : Skill
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
                if(hit.transform.GetComponentInParent<Fear>() != null)
                {
                    Fear thisAIFear = hit.transform.GetComponent<Fear>();
                    HandleIncrease(thisAIFear);
                }
            }
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
