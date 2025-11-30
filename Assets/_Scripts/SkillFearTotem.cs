using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillFearTotem : Skill
{
    public Camera mainCamera;
    public GameObject fearTotem;
    Transform fearTotemTrans;
    bool canPlace=true;
    public int roundsLast;
  
    //private SkillsCollection skillsUI;
    
   
    protected override void Awake()
    {
        base.Awake();
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }
        //skillsUI = GetComponent<SkillsCollection>();
        fearTotemTrans = fearTotem.GetComponent<Transform>();
    }


    private void Start()
    {
        enabled = false;
        
    }

    private void Update()
    {
        

        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Bound object to being placed in maze
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        // Note this is hitting the floor.
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
        {
            if (hit.transform.GetComponentInParent<MazeCellObject>() != null)
            {
               
                Vector3 newPosition = hit.transform.position;
                
                fearTotemTrans.position = newPosition;

            }
        }


    }



    public void OnClick(InputAction.CallbackContext context)
    {
        
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        
        Ray ray = skillSystem.GET_CAM().ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
        {
            
            if(canPlace && hit.transform.GetComponentInParent<MazeCellObject>() != null) 
            {
               
                Vector3 newPos = fearTotemTrans.position;
                GameObject newTotem = Instantiate(fearTotem);

                newTotem.transform.position = newPos;
                FearTotemEffect fearTotemEff = newTotem.GetComponent<FearTotemEffect>();
                
                //placeholder randomization. Fix with way of changing number manually later
                fearTotemEff.targetInt = Random.Range(0, 3);
                
                //for testing, delete later
                //fearTotemEff.sightFear = 200;

                MazeCellObject cell = hit.transform.GetComponentInParent<MazeCellObject>();
                if (cell != null)
                {
                    cell.hasTotem = true;
                    cell.Totem = fearTotemEff.sightFear;
                }

                TurnBasedSystem.DestroyAfterRounds destroyInstance = new TurnBasedSystem.DestroyAfterRounds(roundsLast, newTotem, cell);
                fearTotem.SetActive(false);
                canPlace = false;
                this.enabled = false;
                //skillsUI.ENABLE_BUTTONS();
                EndSkill();

                

            }

        }

    }



    public override void EndSkill()
    {

        base.EndSkill();
        this.enabled = false;
        canPlace = true;
        fearTotem.SetActive(false);
       

    }


    public override void ActivateSkill()
    {
        if (usagesRemaining>= usageDeduction)
        {
            print("Activated");
            base.ActivateSkill();
            enabled = true; // Enable the script allowing this skill to happen once.
            fearTotem.SetActive(true);
            canPlace = true;

           
        }

    }


    public void OnRightClick(InputAction.CallbackContext context)
    {
        ExitInProgress();
    }

}
