using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillAoeFear : Skill
{
    public int roundsLast = 3;
    public Camera mainCamera; // Reference to the camera, if null defaults to main camera
    public GameObject AOEOb;
    Transform obTrans;
    bool canPlace = true;
    

    // Start is called before the first frame update
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }
        obTrans = AOEOb.GetComponent<Transform>();
    }

    void Update()
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Bound object to being placed in maze
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        // Note this is hitting the floor.
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.GetComponentInParent<MazeCellObject>() != null)
            {

                Vector3 newPosition = hit.transform.position;
                newPosition.x += 1f;
                newPosition.y = 0.5f;
                newPosition.z += 1f;
                obTrans.position = newPosition;

            }
        }
        else
        {
            obTrans.position = new Vector3(0, -1000, 0);
        }

    }

    public void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Determine the rooms being selected for a switch
        Ray ray = SkillSystem.GET_CAM().ScreenPointToRay(mousePos);
        RaycastHit hit;
        // Note this is hitting the floor.
        if (Physics.Raycast(ray, out hit))
        {
            if (canPlace && hit.transform.GetComponentInParent<MazeCellObject>() != null)
            {
                AudioManager.PlaySFX(SFXType.SkillAOE);
                Vector3 newPos = obTrans.position;
                GameObject newAOE = Instantiate(AOEOb);
                newAOE.transform.position = newPos;
                TurnBasedSystem.DestroyAfterRounds destroyInstance = new TurnBasedSystem.DestroyAfterRounds(roundsLast, newAOE);
                AOEOb.SetActive(false);
                canPlace = false;
                this.enabled = false;
                SkillsCollection.ENABLE_BUTTONS();
                EndSkill();
            }
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
        if(ResourceManager.Instance.worshipPoints >= ptDeduction)
        {
        base.ActivateSkill();
        enabled = true; // Enable the script allowing this skill to happen once.
        AOEOb.SetActive(true);
        canPlace = true;
        }

    }
    
    /// <summary>
    /// Handles the right-click action to exit the skill if conditions are met.
    /// </summary>
    public void OnRightClick(InputAction.CallbackContext context)
    {
        ExitInput();
    }


}
