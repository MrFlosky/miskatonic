using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Vision { Up, Down, Left, Right }
public class PlayerVision : MonoBehaviour
{
    public Vision facing = Vision.Up;
    public Vector2Int gridPos;
    public int visionRange = 3;

    [SerializeField] Material outlineMaterial;  // Material applied when highlighting tiles

    private List<Vector2Int> currentVisible = new List<Vector2Int>();
    private MazeCellObject currentCell;
    private Dictionary<MeshRenderer, Material[]> originalMaterials
    = new Dictionary<MeshRenderer, Material[]>();

    private bool totemSeenInTurn = false;
    private int totemFear = 100;

    /// <summary>
    /// Set up varibles for current position of AI
    /// </summary>
    void Start()
    {
        // Try to get the MazeCellObject this player is standing on
        currentCell = GetComponent<MazeCellObject>();

        if (currentCell != null)
        {
            gridPos = new Vector2Int(currentCell.GetXCoord(), currentCell.GetYCoord());
        }
        else
        {
            Debug.LogWarning("PlayerVision: No MazeCellObject attached. " +
                             "Make sure you assign currentCell when spawning!");
        }
    }

    /// <summary>
    /// Check if it is a wall block vision direction
    /// </summary>
    /// <param name="nextCell"></param>
    /// <returns>
    ///     true if there is
    ///     otherwise, false
    /// </returns>
    public bool IsBlocked(MazeCell nextCell)
    {
        bool result = true;
        switch (facing)
        {
            case Vision.Up:
                if (nextCell.topWall) return true;
                else return false;
            case Vision.Down:
                if (nextCell.bottomWall) return true;
                else return false;
            case Vision.Left:
                if (nextCell.leftWall) return true;
                else return false;
            case Vision.Right:
                if (nextCell.rightWall) return true;
                else return false;
        }
        return result;
    }

    /// <summary>
    /// Get current vision of AI
    /// Check if the sight is blocked by the wall
    /// </summary>
    /// <returns>
    ///     list of current visible cell position
    /// </returns>
    public List<Vector2Int> GetVisibleCells()
    {
        List<Vector2Int> visible = new List<Vector2Int>();
        var agentNavigation = GetComponent<AgentNavigation>();
        var gameManager = agentNavigation.gameManager;

        for (int i = 0; i <= visionRange; i++)
        {
            MazeCell nextCell = new MazeCell(gridPos.x, gridPos.y);
            switch (facing)
            {
                case Vision.Up:
                    visible.Add(new Vector2Int(gridPos.x, gridPos.y + i));
                    nextCell = gameManager.centralMaze[gridPos.x, gridPos.y + i];
                    break;
                case Vision.Down:
                    visible.Add(new Vector2Int(gridPos.x, gridPos.y - i));
                    nextCell = gameManager.centralMaze[gridPos.x, gridPos.y - i];
                    break;
                case Vision.Left:
                    visible.Add(new Vector2Int(gridPos.x - i, gridPos.y));
                    nextCell = gameManager.centralMaze[gridPos.x - i, gridPos.y];
                    break;
                case Vision.Right:
                    visible.Add(new Vector2Int(gridPos.x + i, gridPos.y));
                    nextCell = gameManager.centralMaze[gridPos.x + i, gridPos.y];
                    break;
            }

            if (IsBlocked(nextCell))
                break; // stop vision at the wall
        }

        return visible;
    }

    /// <summary>
    /// Find the nearest cell object of the position
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns>
    ///     Position of the cell oject
    /// </returns>
    private Vector2Int FindNearestCellGrid(Vector3 worldPos)
    {
        float best = float.MaxValue;
        MazeCellObject nearest = null;

        foreach (var kv in MazeRenderer.Instance.cellGameObjects)
        {
            Vector3 cellWorld = kv.Value.transform.position;
            float d = (cellWorld - worldPos).sqrMagnitude;
            if (d < best)
            {
                best = d;
                nearest = kv.Value.GetComponent<MazeCellObject>();
            }
        }

        if (nearest != null)
            return new Vector2Int(nearest.GetXCoord(), nearest.GetYCoord());

        return Vector2Int.zero;
    }

    /// <summary>
    /// Remove old vision's highlight 
    /// Highlight the visible sight of AI on floor 
    /// (so when AI come in a room, if the floor is not cell object, we cannot see the vision)
    /// </summary>
    public void UpdateVision(bool turn)
    {
        gridPos = FindNearestCellGrid(transform.position);
        var agentNavigation = GetComponent<AgentNavigation>();
        var gameManager = agentNavigation.gameManager;
        var fear = GetComponent<Fear>();

        // 1. Clear old highlights
        foreach (var pos in currentVisible)
        {
            
            if (MazeRenderer.Instance.cellGameObjects.TryGetValue(pos, out GameObject cellObject))
            {
                foreach (Transform child in cellObject.transform)
                {
                    // Reset to original materials
                    var renderer = child.GetComponent<MeshRenderer>();
                    //if (renderer != null)
                    //{
                    //var mats = new List<Material>(renderer.materials);
                    //mats.Remove(outlineMaterial);
                    //renderer.materials = mats.ToArray();
                    //}
                    if (renderer != null && originalMaterials.TryGetValue(renderer, out var mats))
                    {
                        renderer.materials = mats; // restore baseline
                    }
                }
            }
        }

        // 2. Compute new vision
        currentVisible = GetVisibleCells();

        // 3. Apply new highlights
        foreach (var pos in currentVisible)
        {
            MazeCellObject cell = null ;

            if (MazeRenderer.Instance.cellGameObjects.TryGetValue(gridPos, out GameObject cellGO))
            {
                cell = cellGO.GetComponent<MazeCellObject>();
            }

            if (cell && cell.hasTotem && turn)
            {
               
                Debug.LogWarning("There is totem in player vision");
                totemSeenInTurn = true;
                totemFear = cell.Totem;
            }

            if (!turn)
            {
                if (totemSeenInTurn) fear.AddFear(totemFear);
                totemSeenInTurn = false;
            }


            if (MazeRenderer.Instance.cellGameObjects.TryGetValue(pos, out GameObject cellObject))
            {

                foreach (Transform child in cellObject.transform)
                {
                    var renderer = child.GetComponent<MeshRenderer>();
                    //if (renderer != null)
                    //{
                    //	var mats = new List<Material>(renderer.materials);
                    //	if (!mats.Contains(outlineMaterial)) // avoid duplicates
                    //		mats.Add(outlineMaterial);
                    //	renderer.materials = mats.ToArray();
                    //}
                    if (renderer != null)
                    {
                        if (!originalMaterials.ContainsKey(renderer))
                            originalMaterials[renderer] = renderer.materials;

                        var mats = new List<Material>(renderer.materials);
                        if (!mats.Contains(outlineMaterial))
                            mats.Add(outlineMaterial);

                        renderer.materials = mats.ToArray();
                    }
                }
            }
        }
    }


}