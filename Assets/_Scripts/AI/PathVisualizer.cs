using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class is responsible for displaying the nav paths of AI.
/// </summary>
public class PathVisualizer : MonoBehaviour, ITurnObserver
{
    public static PathVisualizer Instance;

    [SerializeField] private GameObject pathMarkerPrefab;
    [SerializeField] private float markerHeight = 0.6f;

    [SerializeField]
    private Color[] agentColors =
    {
        new Color(1f, 0f, 0f, 0.5f),     // Red
        new Color(0f, 1f, 0f, 0.5f),     // Green
        new Color(0f, 0f, 1f, 0.5f),     // Blue
        new Color(1f, 1f, 0f, 0.5f)      // Yellow
    };

    private Dictionary<AgentNavigation, List<GameObject>> visualizedPaths = new();
    private bool pathsVisible;

    /// <summary>
    /// Number of turns remaining with a preview.
    /// </summary>
    private int previewTurnsRemaining = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Turn observer used to trigger path preview if player has been granted
    /// some number of them. 
    /// </summary>
    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
    }

    private void Update()
    {
        // debug for toggling path previews with key
        // if (Keyboard.current.zKey.wasPressedThisFrame)
        // {
        //     TogglePaths();
        // }
    }

    private void OnEnable()
    {
        MapChangeEvents.OnMapChanged += HandleMapChanged;
    }

    private void OnDisable()
    {
        MapChangeEvents.OnMapChanged -= HandleMapChanged;
    }

    private void OnDestroy()
    {
        MapChangeEvents.OnMapChanged -= HandleMapChanged;
    }

    private void HandleMapChanged()
    {
        RefreshPaths();
    }

    public bool ArePathsVisible()
    {
        return pathsVisible;
    }

    public void ShowPaths()
    {
        if (PathCoordinator.Instance == null)
        {
            Debug.LogWarning("PathCoordinator.Instance is null");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance is null");
            return;
        }

        if (AgentManager.Instance == null)
        {
            Debug.LogWarning("AgentManager.Instance is null");
            return;
        }

        HidePaths();

        if (TurnBasedSystem.IsPlayerTurn())
        {
            PathCoordinator.Instance.CoordinateAgentPaths(AgentManager.Instance.agents);
        }

        var allPaths = PathCoordinator.Instance.GetAllPlannedPaths();

        if (allPaths.Count == 0)
        {
            return;
        }

        int colorIndex = 0;

        foreach (var kvp in allPaths)
        {
            AgentNavigation agent = kvp.Key;
            List<(Vector2Int position, int timeStep)> path = kvp.Value;

            if (path.Count == 0)
            {
                continue;
            }

            int tilesToShow;
            if (agent.tilesPerTurn > 0)
            {
                tilesToShow = Mathf.Min(agent.tilesPerTurn, path.Count);
            }
            else
            {
                tilesToShow = path.Count;
            }

            Color agentColor = agentColors[colorIndex % agentColors.Length];
            colorIndex++;

            List<GameObject> markers = new List<GameObject>();

            for (int i = 0; i < tilesToShow; i++)
            {
                var (position, timeStep) = path[i];
                Vector3 worldPos = GetWorldPosition(position);
                GameObject marker = Instantiate(pathMarkerPrefab, worldPos, Quaternion.identity, transform);

                Renderer renderer = marker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(renderer.material);
                    mat.color = agentColor;
                    renderer.material = mat;
                }

                float scale = Mathf.Lerp(0.6f, 0.3f, (float)i / Mathf.Max(1, tilesToShow - 1));
                marker.transform.localScale = Vector3.one * scale;

                markers.Add(marker);
            }

            visualizedPaths[agent] = markers;
        }

        pathsVisible = true;
    }

    public void HidePaths()
    {
        foreach (var markers in visualizedPaths.Values)
        {
            foreach (var marker in markers)
            {
                if (marker != null)
                {
                    Destroy(marker);
                }
            }
        }

        visualizedPaths.Clear();
        pathsVisible = false;

        if (TurnBasedSystem.IsPlayerTurn() && PathCoordinator.Instance != null)
        {
            PathCoordinator.Instance.ClearAllReservations();
        }
    }

    /// <summary>
    /// If paths visible turn them off, otherwise turn em on.
    /// </summary>
    public void TogglePaths()
    {
        if (pathsVisible)
        {
            HidePaths();
        }
        else
        {
            ShowPaths();
        }
    }

    public void RefreshPaths()
    {
        if (!pathsVisible)
        {
            return;
        }

        Debug.Log("Refreshing path visualization after map change");
        HidePaths();
        ShowPaths();
    }

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * GameManager.Instance.CellSize,
            markerHeight,
            gridPosition.y * GameManager.Instance.CellSize
        );
    }

    /// <summary>
    /// Grant player with some given number of previews, deducted each turn.
    /// </summary>
    public void GrantPreviews(int n)
    {
        previewTurnsRemaining += n;
        ShowPaths();
        previewTurnsRemaining -= 1; // count this immediate preview as apart of previews granted
    }


    /// <summary>
    /// Dont show paths during enemy turns. 
    /// </summary>
    public void EnemyTurnInitiated()
    {
        HidePaths();
    }

    /// <summary>
    /// Upon player turn path previews on if any number were granted
    /// as a mission reward.
    /// </summary>
    public void PlayerTurnInitiated()
    {
        if(previewTurnsRemaining > 0)
        {
            previewTurnsRemaining -= 1;
            ShowPaths();
        }
    }
}