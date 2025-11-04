using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
public class SkillRenovation : Skill
{
    /// <summary>
    /// this script handles the activation of the Renovation skill. When active it will check to see if the player clicks on a room
    /// 
    /// if the player clicks on the room it will activate the skill. Effects handled by the RenovationEffect script
    /// </summary>


   // private PlayerInputActions inputActions;
    [SerializeField] GameObject Renovator;
    [SerializeField] AgentManager agentManager;
    [SerializeField] Camera mainCamera;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }

        skillSystem = GetComponent<SkillSystem>();

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

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.GetComponentInParent<MazeCellObject>() != null
            && hit.collider.gameObject.GetComponentInParent<MazeCellObject>().roomType != "Hallway")
            {

                string RoomsName = hit.collider.gameObject.GetComponentInParent<MazeCellObject>().roomType;
                RoomsName = RoomsName.Replace(" ", "");
                Debug.LogError(RoomsName);

                AgentNavigation.RoomName Rn = (AgentNavigation.RoomName)System.Enum.Parse(typeof(AgentNavigation.RoomName), RoomsName);
                
                Debug.LogError(Rn);
                foreach (AgentNavigation AN in agentManager.agents)
                {
                    AN.SetBonusFearRoom(Rn);

                }
                
                
            }
        }

    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        print("test");
        ExitInProgress();
        this.enabled = false;
    }

    public override void EndSkill()
    {
        base.EndSkill();
    }

    public override void ActivateSkill()
    {
        base.ActivateSkill();
    }
    
}
