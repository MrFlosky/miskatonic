using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
public class SkillRenovation : Skill
{
    /// <summary>
    /// this script handles the activation of the Renovation skill. When active it will check to see if the player clicks on a room
    /// 
    /// if the player clicks on the room it will activate the skill. Effects Added fear is handled by AgentNavigator script
    /// </summary>

    /// currently effects do not stack. creating a new renovator will overwrite the previous one

    [SerializeField] GameObject renObj;
    [SerializeField] protected AgentManager agentManager;
    [SerializeField] Camera mainCamera;
    [SerializeField] int roundsLast;

    [SerializeField] List<GameObject> Renovators;

    protected override void Awake()
    {
        base.Awake();

        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }
        Renovators = new List<GameObject>();
    }

    void Start()
    {
        enabled = false;
    }


    public void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse
            ? Mouse.current.position.ReadValue()
            : VirtualMouseUI.Instance.GetVirtualMousePosition();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
        {
            MazeCellObject cellHit = hit.collider.gameObject.GetComponentInParent<MazeCellObject>();
            if (cellHit != null
            && cellHit.roomType != "Hallway")
            {
                
                string RoomsName = cellHit.roomType;
                RoomsName = RoomsName.Replace(" ", "");
                

                AgentNavigation.RoomName Rn = (AgentNavigation.RoomName)System.Enum.Parse(typeof(AgentNavigation.RoomName), RoomsName);

                
                foreach (AgentNavigation AN in agentManager.agents)
                {
                   AN.BonusFearRooms.Add(Rn);
                    
                    

                }
                RoomCenter rc = cellHit.transform.parent.GetComponentInChildren<RoomCenter>();
                CreateRenovator(rc.transform.position);
                EndSkill();
                
            }
        }

    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
//        Debug.LogError("test");
        ExitInProgress();
        
    }

    public override void EndSkill()
    {
        base.EndSkill();
    }

    public override void ActivateSkill()
    {
        base.ActivateSkill();
    }

    public void CreateRenovator(Vector3 newPos)
    {
        Renovators.Clear();

        GameObject newRen = Instantiate(renObj);
        RenovationEffect renEff = newRen.GetComponent<RenovationEffect>();
        newRen.transform.position=newPos;
        newRen.transform.SetParent(transform);
        Renovators.Add(newRen);
        renEff.maxTurns = roundsLast;
        renEff.skillRenovation = this;
        
        foreach (AgentNavigation AN in agentManager.agents)
            {
                AN.SetBonusFearRoom();  
            }
        

    }


    public void EndRenovation(int index)
    {
       
        foreach (AgentNavigation AN in agentManager.agents)
            {
                AN.BonusFearRooms[index]= AgentNavigation.RoomName.None;
                AN.CheckForClear();
            }
        
        

    }

   

}
