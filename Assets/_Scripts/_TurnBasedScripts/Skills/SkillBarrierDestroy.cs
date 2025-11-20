using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
/// This class is responsible for the skill that destroys walls.
/// Once a wall is destroyed, it is removed for the rest of the game and AI recognize the change.
/// </summary>
public class SkillBarrierDestroy : Skill
{
    [Header("Material of a possible barrier wall")]
    public Material selectedMat;

    /// <summary>
    /// Original mat of cell walls.
    /// </summary>
    private Material originalMat;

    /// <summary>
    /// Only need to get original mat once on first cell click!
    /// </summary>
    private bool getOriginalMat = true;

    public int roundsLast = 3;

    private List<Transform> activeWalls = new List<Transform>();
    private int lastWallIndex;
    private bool canCycle = false;
    /// <summary>
    /// Starting at index 1 for barrer destroy, because want to skip the floor as option.
    /// </summary>
    private int cycleCount = 0;
    private Transform cellHit;

    private void Start()
    {
        // I had a bug because this was in awake thus stopping start from being called until this script became enabled again.
        //Debug.Log(skillSystem != null);
        enabled = false;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!canCycle)
        {
            Vector2 mousePos = Vector2.zero;
            mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
            Ray ray = skillSystem.GET_CAM().ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
            {
                if (hit.transform.GetComponentInParent<MazeCellObject>() != null)
                {
                    cellHit = hit.transform.parent;
                    Debug.Log(cellHit.name);
                    AudioManager.PlaySFX(SFXType.MouseButtonHit);
                    GetPossibleBarrierWalls(cellHit);
                    canCycle = true;

                    // grab original mat once!
                    if (getOriginalMat)
                    {
                        getOriginalMat = false;
                        originalMat = cellHit.GetChild(0).GetComponent<Renderer>().material;
                    }
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
            Transform confirmedWall = activeWalls[lastWallIndex];
            MazeCellObject confirmedWallMz = cellHit.GetComponent<MazeCellObject>();
            int x = confirmedWallMz.GetXCoord();
            int y = confirmedWallMz.GetYCoord();
            WallType wT = GetWallType(cellHit, confirmedWall);


            MazeGenerator.Instance.SetWallInternalState(x, y, wT, false);
            confirmedWall.gameObject.SetActive(false);
            DestroyWallWithNeighbor(x, y, wT);

            // DESTROYED WALL DOESNT RETURN FOR NOW< SO DONT NEED TO USE A SYSTEM LIKE THIS TO RESTORE IT!
            //TurnBasedSystem.DestroyAfterRounds destroyInstance = new TurnBasedSystem.DestroyAfterRounds(roundsLast, confirmedWall.gameObject, wT);

            MapChangeEvents.InvokeBarrierChanged();
            ResetValues();
            EndSkill();
        }
    }

    /// <summary>
    /// Called up player right clicking to cancel their current barrier selection and or exiting
    /// this selected skill entirely if they double right click.
    /// </summary>
    public void OnRightClick(InputAction.CallbackContext context)
    {
        // If a cell is selected, cancel the selection
        if (canCycle)
        {
            Debug.Log("Cancelling wall selection");
            foreach (Transform wall in activeWalls)
            {
                wall.gameObject.SetActive(true);
            }
            AudioManager.PlaySFX(SFXType.MouseButtonHit);
            ResetValues(); // Reset values for the current selection
        }
        else
        {
            // If no cell is selected, end the skill and refund points
            Debug.Log("No cell selected, ending skill");
            ExitInProgress(); // samme as base.ExitInProgress if no override in this function
        }

    }
    public override void EndSkill()
    {
        base.EndSkill();
        // If player ends turn without confirming wall
        // (this is now impossible bc end turn button goes inactive)
        if (canCycle) { activeWalls[lastWallIndex].gameObject.SetActive(true); }
        this.enabled = false;
    }

    /// <summary>
    /// Get active barrier walls apart of cell, which are those we may destroy.
    /// </summary>
    private void GetPossibleBarrierWalls(Transform cell)
    {
        activeWalls.Clear();

        int i = 0;
        foreach (Transform child in cell) // interesting cell is just a transform, but we can loop through its transforms
        {
            GameObject wallOb = child.gameObject;
            if (wallOb.activeSelf == true) // grab all active walls
            {
                if (i == 0) { i++; continue; } // index 0 is floor, skip it

                activeWalls.Add(child);
                Debug.Log(child.name);
                i++;
            }
        }
    }



    /// <summary>
    /// Cycle through active walls to choose one to destroy.
    /// </summary>
    private void CycleWall()
    {
        Debug.Log("Cycling through walls");
        // each new cycle input, reset all previously inactive walls in activewalls, back to active
        // with their original materials
        foreach (Transform wall in activeWalls)
        {
            wall.gameObject.SetActive(true);
            Renderer wallMat = wall.GetComponent<Renderer>();
            wallMat.material = originalMat;
        }

        Renderer matOfSelectedWall = activeWalls[cycleCount].GetComponent<Renderer>();
        matOfSelectedWall.material = selectedMat;

        lastWallIndex = cycleCount;
        cycleCount = (cycleCount + 1) % activeWalls.Count; // wrap around at last wall index
    }

    private void ResetValues()
    {
        canCycle = false;
        cycleCount = 0;
        cellHit = null;
        activeWalls.Clear(); // Clear the list to avoid retaining any references
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

    /// <summary>
    /// If destroyed wall is stacked with another neighboring cell's wall,
    /// destroy that wall too.
    /// </summary>
    private void DestroyWallWithNeighbor(int x, int y, WallType wall)
    {
        int neighborX = x;
        int neighborY = y;
        WallType neighborWall = wall;

        // Calculate neighbor position and wall
        switch (wall)
        {
            case WallType.Top:
                neighborY = y + 1;
                neighborWall = WallType.Bottom;
                break;
            case WallType.Bottom:
                neighborY = y - 1;
                neighborWall = WallType.Top;
                break;
            case WallType.Left:
                neighborX = x - 1;
                neighborWall = WallType.Right;
                break;
            case WallType.Right:
                neighborX = x + 1;
                neighborWall = WallType.Left;
                break;
        }

        // check to see if this neighbor cell even exists within bounds of maze
        if (neighborX < 0 || neighborX >= MazeGenerator.Instance.mazeWidth ||
            neighborY < 0 || neighborY >= MazeGenerator.Instance.mazeHeight)
        {
            return;
        }

        // Get the neighbor's wall GameObject
        GameObject neighborWallObj = MazeRenderer.Instance.GetWallObject(neighborX, neighborY, neighborWall);

        // Only destroy if the wall exists and is active (duplicated shared wall)
        if (neighborWallObj != null && neighborWallObj.activeSelf)
        {
            MazeGenerator.Instance.SetWallInternalState(neighborX, neighborY, neighborWall, false);
            neighborWallObj.SetActive(false);
            Debug.Log($"Destroyed shared wall: Cell({neighborX}, {neighborY}) - {neighborWall}");
        }
    }

    public override void ActivateSkill()
    {
        if (usagesRemaining >= usageDeduction)
        {
            base.ActivateSkill();
            this.enabled = true;
        }
    }
}
