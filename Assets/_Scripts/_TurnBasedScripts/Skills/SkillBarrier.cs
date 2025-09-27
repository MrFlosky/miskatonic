using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WallType
{
    Top, Bottom, Left, Right
}

public class SkillBarrier : Skill
{
    [Header("Material of a possible barrier wall")]
    public Material selectedMat;

    public int roundsLast = 3;

    private List<Transform> mInactiveWalls = new List<Transform>();
    private int lastWallIndex;
    private bool canCycle = false;
    private int cycleCount = 0;

    private Transform cellHit;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        // For some reason the parent class, Skill, does not pass its reference to skillSystem as it should. Try
        // debugging in various way but this my best fix. Simply calling get component again to get skillSystem.
        inputActions = new PlayerInputActions();
        skillSystem = GetComponent<SkillSystem>();
    }

    private void Start()
    {
        // I had a bug because this was in awake thus stopping start from being called until this script became enabled again.
        //Debug.Log(skillSystem != null);
        enabled = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Click.performed += OnClick;
        inputActions.Player.Confirm.performed += OnConfirm;
        inputActions.Player.RightClick.performed += OnRightClick; // Right-click for canceling
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClick;
        inputActions.Player.Confirm.performed -= OnConfirm;
        inputActions.Player.RightClick.performed -= OnRightClick;
        inputActions.Disable();
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (!canCycle)
        {
            Vector2 mousePos = Vector2.zero;
            mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
            Ray ray = skillSystem.GET_CAM().ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.GetComponentInParent<MazeCellObject>() != null)
                {
                    cellHit = hit.transform.parent;
                    Debug.Log(cellHit.name);
                    AudioManager.PlaySFX(SFXType.MouseButtonHit);
                    GetPossibleBarrierWalls(cellHit);
                    canCycle = true;
                }
            }
        }
        else
        {
            AudioManager.PlaySFX(SFXType.MouseButtonHit);
            CycleWall();
        }
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        if (canCycle)
        {
            PlayerController.Instance.SetActiveSkill(null);
            AudioManager.PlaySFX(SFXType.SkillBarrier);
            Debug.Log("Confirming wall placement");
            this.enabled = false;
            Transform confirmedWall = mInactiveWalls[lastWallIndex];
            MazeCellObject confirmedWallMz = cellHit.GetComponent<MazeCellObject>();
            int x = confirmedWallMz.GetXCoord();
            int y = confirmedWallMz.GetYCoord();
            WallType wT = GetWallType(cellHit, confirmedWall);

            MazeGenerator.Instance.SetWallInternalState(x, y, wT, true);
            TurnBasedSystem.DestroyAfterRounds destroyInstance = new TurnBasedSystem.DestroyAfterRounds(roundsLast, confirmedWall.gameObject, wT);
            ResetValues();
            EndSkill();
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        // Double processing fix
        if (inputActions.Player.RightClick.phase == InputActionPhase.Performed)
        {
            return;
        }
        // If a cell is selected, cancel the selection
        if (canCycle)
        {
            Debug.Log("Cancelling wall selection");
            foreach (Transform wall in mInactiveWalls)
            {
                wall.gameObject.SetActive(false);
            }
            AudioManager.PlaySFX(SFXType.MouseButtonHit);
            ResetValues(); // Reset values for the current selection
            //inputActions.Player.RightClick.Disable(); // Double Processing Fix(For our purposes anyways, as it is still doing the call twice)
        }
        else
        {
            // If no cell is selected, end the skill
            Debug.Log("No cell selected, ending skill");
            base.ExitInput();
            EndSkill();
   
        }
        
    }

    public override void EndSkill()
    {
        base.EndSkill();
        this.enabled = false;

        // If player ends turn without confirming wall.
        if (canCycle)
        {
            mInactiveWalls[lastWallIndex].gameObject.SetActive(false);
        }
        ResetValues();
        ExitInput();
    }

    private void GetPossibleBarrierWalls(Transform cell)
    {
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



    private void CycleWall()
    {
        Debug.Log("Cycling through walls");
        foreach (Transform wall in mInactiveWalls)
        {
            wall.gameObject.SetActive(false);
        }
        mInactiveWalls[cycleCount].gameObject.SetActive(true);
        lastWallIndex = cycleCount;

        cycleCount = (cycleCount + 1) % mInactiveWalls.Count;
    }

    private void ResetValues()
    {
        canCycle = false;
        cycleCount = 0;
        cellHit = null;
        mInactiveWalls.Clear(); // Clear the list to avoid retaining any references
    }

    private WallType GetWallType(Transform cell, Transform wallToIdentify)
    {
        int i = 0;
        foreach (Transform child in cell)
        {
            if (child == wallToIdentify)
            {
                break;
            }
            i++;
        }

        return i switch
        {
            1 => WallType.Bottom,
            2 => WallType.Top,
            3 => WallType.Right,
            4 => WallType.Left,
            _ => WallType.Top // Default case, should not happen
        };
    }

    public override void ActivateSkill()
    {
        if (usagesRemaining >= ptDeduction)
        {
            base.ActivateSkill();
            this.enabled = true;
        }
    }
}
