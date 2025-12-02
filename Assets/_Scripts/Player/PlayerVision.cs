using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public enum Vision { Up, Down, Left, Right }
public class PlayerVision : MonoBehaviour, ITurnObserver
{
    public Vision facing = Vision.Up;
    public Vector2Int gridPos;
    public int visionRange = 3;

    [SerializeField] Material outlineMaterial;  // Material applied when highlighting tiles

    private List<Vector2Int> currentVisible = new List<Vector2Int>();
    private List<Vector2Int> currentVisibleRadius = new List<Vector2Int>();
    private MazeCellObject currentCell;
    private Dictionary<MeshRenderer, Material[]> originalMaterials
    = new Dictionary<MeshRenderer, Material[]>();


    // Fear Totem skill
    public List<MazeCellObject> totemCell = new List<MazeCellObject>();
    private int totemFear = 0;

    // Psychosis skill
    public bool isPsychosis = false;
    private int endTurn;
    private int psyRadius = 20;
    private List<PlayerVision> nearbyCharacters = new List<PlayerVision>();
    private int psyFear = 0;

    public Light psychosisLight;
    public Color psychosisColor = new Color(0.6f, 0f, 0.8f, 1f); // purple
    public float psychosisIntensity = 6f;

    // Incept Loneliness
    private int lonelyFear = 2;
    public bool isLonely = false;
    public List<PlayerVision> notNearbyCharacters = new List<PlayerVision>();
    private List<PlayerVision> nearbyCharsForDialog = new List<PlayerVision>();
    private int lonelyRadius = 20;

    // Tile distance required for a character to see another character around them
    private int nearbyRadius = 4;

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

    private void Update()
    {
//        Debug.Log("Visible cells: " + string.Join(", ", currentVisibleRadius),this);

        // string nearby = string.Join(", ", nearbyCharacters.Select(c => c.gameObject.GetComponent<DialogueBank>().character));
        // string visible = string.Join(", ", currentVisible.Select(v => v.ToString()));   
        // Debug.Log($"{gameObject.GetComponent<DialogueBank>().character} is next to {nearby}");
        // Debug.Log($"{gameObject.GetComponent<DialogueBank>().character} can see {visible}");
    }

    void Awake()
    {
        GameObject lightObj = new GameObject("PsychosisLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = new Vector3(0, 2f, 0); // slightly above player
        lightObj.SetActive(true);

        psychosisLight = lightObj.AddComponent<Light>();
        psychosisLight.type = LightType.Point;
        psychosisLight.color = psychosisColor;
        psychosisLight.intensity = 0f; // start off
        psychosisLight.range = psyRadius;
        psychosisLight.enabled = false;
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

            MazeCell nextCell = gameManager.GetCentralMaze()[newX, newY];
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
        currentVisibleRadius = GetVisionRadiusBlocked(nearbyRadius);

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


            // if (MazeRenderer.Instance.cellGameObjects.TryGetValue(pos, out GameObject cellObject))
            // {

            //     foreach (Transform child in cellObject.transform)
            //     {
            //         var renderer = child.GetComponent<MeshRenderer>();
            //         //if (renderer != null)
            //         //{
            //         //	var mats = new List<Material>(renderer.materials);
            //         //	if (!mats.Contains(outlineMaterial)) // avoid duplicates
            //         //		mats.Add(outlineMaterial);
            //         //	renderer.materials = mats.ToArray();
            //         //}
            //         if (renderer != null)
            //         {
            //             if (!originalMaterials.ContainsKey(renderer))
            //                 originalMaterials[renderer] = renderer.materials;

            //             var mats = new List<Material>(renderer.materials);
            //             if (!mats.Contains(outlineMaterial))
            //                 mats.Add(outlineMaterial);

            //             renderer.materials = mats.ToArray();
            //         }
            //     }
            // }
        }


        foreach (var player in PlayerVisionCollection.S.GetPlayerVisions())
        {
            if (player == this || player.gridPos == new Vector2(0,0)) continue;
            if (!nearbyCharsForDialog.Contains(player) && player.GetComponent<DialogueBank>().GetDialogueType() == DialogueBank.DialogueType.None && (currentVisible.Contains(player.GetComponent<PlayerVision>().gridPos) || currentVisibleRadius.Contains(player.GetComponent<PlayerVision>().gridPos)))
            {
                CharacterDialogTriggered(player);
                player.CharacterDialogTriggered(this);
                return;
            }
        }


    }

    public void CharacterDialogTriggered(PlayerVision player)
    {
        nearbyCharsForDialog.Add(player);
        gameObject.GetComponent<DialogueBank>().QueueSpecialDialog($"Encounter{player.GetComponent<DialogueBank>().character}", DialogueBank.DialogueType.Encounter);
    }

    public void EnablePsychosisVision(bool enable, int endAtTurn, int radius, int fear)
    {
        isPsychosis = enable;
        endTurn = endAtTurn;
        psyRadius = radius;
        psyFear = fear;

        if (psychosisLight != null)
        {
            psychosisLight.enabled = true;
            psychosisLight.color = psychosisColor;
            psychosisLight.intensity = psychosisIntensity;
            psychosisLight.range = psyRadius;
        }
    }

    public void EnableInceptLoneliness(bool enable, int fear, int radius)
    {
        isLonely = enable;
        lonelyFear = fear;
        lonelyRadius = radius;
    }

    /// <summary>
    /// Returns all cells within a circular (360�) vision range.
    /// Ignores facing direction and wall blocking for Psychosis.
    /// </summary>
    public List<Vector2Int> GetFullVisionCells()
    {
         return GetFullVisionCells(psyRadius);
    }
    
    public List<Vector2Int> GetFullVisionCells(int radius)
    {
        List<Vector2Int> visible = new List<Vector2Int>();
        var agentNavigation = GetComponent<AgentNavigation>();
        var gameManager = agentNavigation.gameManager;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
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
    public List<Vector2Int> GetVisionRadiusBlocked(int radius)
    {
        List<Vector2Int> visible = new List<Vector2Int>();
        var agentNavigation = GetComponent<AgentNavigation>();
        var gameManager = agentNavigation.gameManager;

        int minX = Mathf.Max(0, gridPos.x - radius);
        int maxX = Mathf.Min(gameManager.centralMaze.GetLength(0) - 1, gridPos.x + radius);
        int minY = Mathf.Max(0, gridPos.y - radius);
        int maxY = Mathf.Min(gameManager.centralMaze.GetLength(1) - 1, gridPos.y + radius);

        int radiusSq = radius * radius;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int dx = x - gridPos.x;
                int dy = y - gridPos.y;
                if (dx * dx + dy * dy > radiusSq) continue;

                Vector2Int target = new Vector2Int(x, y);

                if (HasLineOfSight(gridPos, target))
                {
                    visible.Add(target);
                }
            }
        }

        return visible;
    }


    /// <summary>
    /// Grid-based line-of-sight.
    /// Returns true if there is an unobstructed line from center of 'from' cell to center of 'to' cell.
    /// </summary>
    private bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        if (from == to) return true;

        var agentNavigation = GetComponent<AgentNavigation>();
        var gameManager = agentNavigation.gameManager;

        float startX = from.x + 0.5f;
        float startY = from.y + 0.5f;
        float endX = to.x + 0.5f;
        float endY = to.y + 0.5f;

        float dx = endX - startX;
        float dy = endY - startY;

        int stepX = (dx > 0) ? 1 : (dx < 0) ? -1 : 0;
        int stepY = (dy > 0) ? 1 : (dy < 0) ? -1 : 0;

        float tMaxX, tMaxY;
        float tDeltaX, tDeltaY;

        int currentX = from.x;
        int currentY = from.y;

        if (stepX != 0)
        {
            float nextGridX = (stepX > 0) ? (currentX + 1.0f) : (currentX * 1.0f);
            tMaxX = (nextGridX - startX) / dx; 
            tDeltaX = 1.0f / Mathf.Abs(dx);
        }
        else
        {
            tMaxX = float.PositiveInfinity;
            tDeltaX = float.PositiveInfinity;
        }

        if (stepY != 0)
        {
            float nextGridY = (stepY > 0) ? (currentY + 1.0f) : (currentY * 1.0f);
            tMaxY = (nextGridY - startY) / dy;
            tDeltaY = 1.0f / Mathf.Abs(dy);
        }
        else
        {
            tMaxY = float.PositiveInfinity;
            tDeltaY = float.PositiveInfinity;
        }

        int targetX = to.x;
        int targetY = to.y;

        while (true)
        {
            if (currentX == targetX && currentY == targetY)
                return true;

            if (tMaxX < tMaxY)
            {
                MazeCell currentCell = gameManager.centralMaze[currentX, currentY];


                if (stepX > 0)
                {
                    if (currentCell.rightWall) return false;
                }
                else
                {
                    if (currentCell.leftWall) return false;
                }

                currentX += stepX;
                tMaxX += tDeltaX;
            }
            else
            {
                MazeCell currentCell = gameManager.centralMaze[currentX, currentY];


                if (stepY > 0)
                {
                    if (currentCell.topWall) return false;
                }
                else
                {
                    if (currentCell.bottomWall) return false;
                }

                currentY += stepY;
                tMaxY += tDeltaY;
            }

            if (currentX < 0 || currentY < 0 ||
                currentX >= gameManager.centralMaze.GetLength(0) ||
                currentY >= gameManager.centralMaze.GetLength(1))
                return false;
        }
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
            

            foreach (var player in PlayerVisionCollection.S.GetPlayerVisions())
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

        if (isLonely && turn)
        {
            var agentNavigation = GetComponent<AgentNavigation>();
            var gameManager = agentNavigation.gameManager;
            
            var visibleCells = GetFullVisionCells();

            // int nearbyCharacters = 0;
            

            foreach (var player in PlayerVisionCollection.S.GetPlayerVisions())
            {
                if (player == this) continue;

                Vector2Int otherPos = player.GetComponent<PlayerVision>().gridPos;
                Vector2Int nearestCell = FindNearestCellGrid(player.transform.position);
                int distance = Mathf.Abs(nearestCell.x - gridPos.x) + Mathf.Abs(nearestCell.y - gridPos.y);

                if (distance <= lonelyRadius && !notNearbyCharacters.Contains(player))
                {
                    notNearbyCharacters.Add(player);
                }
                    
            }

        }

        if (!turn)
        {
            nearbyCharacters.Clear();
            if (notNearbyCharacters.Count == 0 && isLonely)
            {
                var fear = GetComponent<Fear>();
                fear.AddFear(lonelyFear);
            }
                
            notNearbyCharacters.Clear();
        }
            

        if (TurnBasedSystem.GET_TURN_COUNT() == endTurn)
        {
            isPsychosis = false;
            nearbyCharacters.Clear();
            if (psychosisLight != null)
            {
                psychosisLight.intensity = 0f;
                psychosisLight.enabled = false;
            }
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