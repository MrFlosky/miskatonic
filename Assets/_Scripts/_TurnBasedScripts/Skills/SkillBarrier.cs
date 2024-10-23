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
        inputActions = new PlayerInputActions();
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
            Ray ray = SkillSystem.GET_CAM().ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.GetComponentInParent<MazeCellObject>() != null)
                {
                    cellHit = hit.transform.parent;
                    Debug.Log(cellHit.name);
                    GetPossibleBarrierWalls(cellHit);
                    canCycle = true;
                }
            }
        }
        else
        {
            CycleWall();
        }
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        if (canCycle)
        {
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
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        // If a cell is selected, cancel the selection
        if (canCycle)
        {
            Debug.Log("Cancelling wall selection");
            foreach (Transform wall in mInactiveWalls)
            {
                wall.gameObject.SetActive(false);
            }
            ResetValues(); // Reset values for the current selection
        }
        else
        {
            // If no cell is selected, end the skill
            Debug.Log("No cell selected, ending skill");
            EndSkill();
        }
    }

    public override void EndSkill()
    {
        base.EndSkill();
        this.enabled = false;

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
        if (SkillSystem.GET_PTS() >= ptDeduction)
        {
            base.ActivateSkill();
            this.enabled = true;
        }
    }
}
