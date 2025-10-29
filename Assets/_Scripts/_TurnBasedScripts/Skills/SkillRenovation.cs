using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class SkillRenovation : Skill
{
    /// <summary>
    /// this script handles the activation of the Renovation skill. When active it will check to see if the player clicks on a room
    /// 
    /// if the player clicks on the room it will activate the skill. Effects handled by the RenovationEffect script
    /// </summary>

    private PlayerInputActions inputActions;
    private void Awake()
    {
        
        skillSystem = GetComponent<SkillSystem>();

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
                print("Heyo");
            }
        }

    }
}
