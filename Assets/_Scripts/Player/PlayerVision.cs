using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Vision { Up, Down, Left, Right }
public class PlayerVision : MonoBehaviour, ITurnObserver
{
    public Vision facing = Vision.Up;
    public Vector2Int gridPos;
    public int visionRange = 3;

    [SerializeField] Material outlineMaterial;  // Material applied when highlighting tiles

    private List<Vector2Int> currentVisible = new List<Vector2Int>();
    private MazeCellObject currentCell;
    private Dictionary<MeshRenderer, Material[]> originalMaterials
    = new Dictionary<MeshRenderer, Material[]>();

    // Fear Totem skill
    public List<MazeCellObject> totemCell = new List<MazeCellObject>();
    private int totemFear = 0;

    // Psychosis skill
    public bool isPsychosis = false;
    private int endTurn;
    private int psyRadius;
    private List<PlayerVision> nearbyCharacters = new List<PlayerVision>();
    private int psyFear = 0;


    /// <summary>
    /// Set up varibles for current position of AI
    /// </summary>
    void Start()
    {
        // Try to get the MazeCellObject this player is standing on
        currentCell = GetComponent<MazeCellObject>();
        TurnBasedSystem.SubscribeAsObserver(this);

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
            
            int newX = gridPos.x;
            int newY = gridPos.y;

            switch (facing)
            {
                case Vision.Up:
                    newY = gridPos.y + i;
                    break;
                case Vision.Down:
                    newY = gridPos.y - i;
                    break;
                case Vision.Left:
                    newX = gridPos.x - i;
                    break;
                case Vision.Right:
                    newX = gridPos.x + i;
                    break;
            }

            if (newX < 0 || newX >= gameManager.centralMaze.GetLength(0) ||
                newY < 0 || newY >= gameManager.centralMaze.GetLength(1))
            {
                break; // Out of maze bounds, stop vision
            }

            MazeCell nextCell = new MazeCell(newX, newY);
            visible.Add(new Vector2Int(newX, newY));

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

            if (MazeRenderer.Instance.cellGameObjects.TryGetValue(pos, out GameObject cellGO))
            {
                cell = cellGO.GetComponent<MazeCellObject>();
            }

            if (cell && cell.hasTotem)
            {
               
                Debug.LogWarning("There is totem in player vision");
                if (!totemCell.Contains(cell)) {
                    fear.AddFear(cell.Totem);
                    totemCell.Add(cell);
                }
            }

            if (!turn)
            {
                totemCell.Clear();
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

    public void EnablePsychosisVision(bool enable, int endAtTurn, int radius, int fear)
    {
        isPsychosis = enable;
        endTurn = endAtTurn;
        psyRadius = radius;
        psyFear = fear;
    }

    /// <summary>
    /// Returns all cells within a circular (360�) vision range.
    /// Ignores facing direction and wall blocking for Psychosis.
    /// </summary>
    public List<Vector2Int> GetFullVisionCells()
    {
        List<Vector2Int> visible = new List<Vector2Int>();
        var agentNavigation = GetComponent<AgentNavigation>();
        var gameManager = agentNavigation.gameManager;

        for (int dx = -psyRadius; dx <= psyRadius; dx++)
        {
            for (int dy = -psyRadius; dy <= psyRadius; dy++)
            {
                int newX = gridPos.x + dx;
                int newY = gridPos.y + dy;

                Vector2Int pos = new Vector2Int(gridPos.x + dx, gridPos.y + dy);

                // Skip out of range
                if (newX < 0 || newX >= gameManager.centralMaze.GetLength(0) ||
                newY < 0 || newY >= gameManager.centralMaze.GetLength(1))
                {
                    break; // Out of maze bounds, stop vision
                }

                visible.Add(pos);
            }
        }

        return visible;
    }


    public void ApplyPsychosis(bool turn)
    {
        if (isPsychosis && turn)
        {

            var agentNavigation = GetComponent<AgentNavigation>();
            var gameManager = agentNavigation.gameManager;
            var fear = GetComponent<Fear>();
            var visibleCells = GetFullVisionCells();

            // int nearbyCharacters = 0;
            

            foreach (var player in FindObjectsOfType<PlayerVision>())
            {
                if (player == this) continue;

                Vector2Int otherPos = player.GetComponent<PlayerVision>().gridPos;
                Vector2Int nearestCell = FindNearestCellGrid(player.transform.position);
                int distance = Mathf.Abs(nearestCell.x - gridPos.x) + Mathf.Abs(nearestCell.y - gridPos.y);

                if (distance <= psyRadius && !nearbyCharacters.Contains(player))
                {
                    // nearbyCharacters++;
                    nearbyCharacters.Add(player);
                    fear.AddFear(psyFear);
                    Debug.LogWarning($"Psychosis raised: {psyFear}");
                }
                    
            }

        }

        if (!turn)
            nearbyCharacters.Clear();

        if (TurnBasedSystem.GET_TURN_COUNT() == endTurn)
        {
            isPsychosis = false;
            nearbyCharacters.Clear();
        }

    }
    /// <summary>
    /// Every player turn, if the psychosis was applied, check the vision and fear added if any 
    /// player is around.
    /// </summary>
    void ITurnObserver.PlayerTurnInitiated()
    {

        if (TurnBasedSystem.GET_TURN_COUNT() == endTurn)
        {
            isPsychosis = false;
        }

    }
    void ITurnObserver.EnemyTurnInitiated()
    {
        
    }
}