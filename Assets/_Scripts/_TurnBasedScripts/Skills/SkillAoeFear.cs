using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkillAoeFear : Skill
{
    public int roundsLast = 3;

    public GameObject AOEOb;
    Transform obTrans;
    bool canPlace = true;

    // Start is called before the first frame update
    void Start()
    {
        obTrans = AOEOb.GetComponent<Transform>();
    }

    void Update()
    {
            base.ExitInput();
            // Bound object to being placed in maze
            Ray ray = SkillSystem.GET_CAM().ScreenPointToRay(Input.mousePosition); 
            RaycastHit hit;
            // Note this is hitting the floor.
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.transform.GetComponentInParent<MazeCellObject>() != null)
                {
                    
                    obTrans.position = hit.transform.position;
                    
                }
            }

            if(Input.GetMouseButtonDown(0) && canPlace)
            {
                
                Vector3 newPos = obTrans.position;
                GameObject newAOE = Instantiate(AOEOb);
                newAOE.transform.position = newPos;
                TurnBasedSystem.DestroyAfterRounds destroyInstance = new TurnBasedSystem.DestroyAfterRounds(roundsLast, newAOE);
                AOEOb.SetActive(false);
                canPlace = false;
                this.enabled = false;
                SkillsCollection.ENABLE_BUTTONS();
            }
        
    }

    public override void EndSkill()
    {
 
        base.EndSkill();
        this.enabled = false;

        AOEOb.SetActive(false);
        canPlace = true;

    }

    /// <summary>
    /// We might refactor this later so not going based on gameob name, instead a var in mazecell ob would be better
    /// </summary>
    /// <param name="selectedCell"></param>
    // void GetSurroundingCells(MazeCellObject selectedCell)
    // {
    //     var match = Regex.Match(selectedCell.gameObject.name, @"\[(\d+),(\d+)\]");
    //     if (match.Success)
    //     {
    //         int x = int.Parse(match.Groups[1].Value);
    //         int y = int.Parse(match.Groups[2].Value);
    //         var coordinates = (x, y);
            
    //         Debug.Log($"Extracted coordinates: ({coordinates.x}, {coordinates.y})");
    //     }


    //     GameObject parentRoom = selectedCell.transform.parent.gameObject;
    //     foreach(MazeCellObject cell in parentRoom.GetComponentsInChildren<MazeCellObject>())
    //     {


    //     }

    // }


    public override void ActivateSkill()
    {
        base.ActivateSkill();
        this.enabled = true; // Enable the script allowing this skill to happen once.
        AOEOb.SetActive(true);
        canPlace = true;

    }


}
