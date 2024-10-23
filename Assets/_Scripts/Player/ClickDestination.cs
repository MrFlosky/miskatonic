using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToMove : MonoBehaviour
{
    public Camera mainCamera; // Reference to the camera, if null defaults to main camera
    public MazeRenderer mazeRenderer; // Reference to the MazeRenderer script
    public AgentNavigation agent; // Reference to the AgentNavigation script
    private float CellSize = 1.0f; // Size of the grid cells (e.g., 1 for 1x1, 2 for 2x2)

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }

        if (mazeRenderer == null)
        {
            mazeRenderer = FindObjectOfType<MazeRenderer>();
        }

        if (agent == null)
        {
            agent = FindObjectOfType<AgentNavigation>();
        }

        CellSize = mazeRenderer.CellSize;
    }

    private void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform a raycast to find the clicked position on the ground
            if (Physics.Raycast(ray, out hit))
            {
                // Get the center of the grid cell closest to the hit point
                Vector3 targetPosition = GetGridCenter(hit.point);

                // Convert the target position to grid coordinates
                Vector2Int gridCoordinates = WorldToGridPosition(targetPosition);

                // Stop the agent's movement and set the new destination
                agent.SetDestination(gridCoordinates);
            }
        }
    }

    // Calculate the center of the grid cell
    private Vector3 GetGridCenter(Vector3 position)
    {
        float gridX = Mathf.Round(position.x / CellSize) * CellSize;
        float gridZ = Mathf.Round(position.z / CellSize) * CellSize;

        // Use the agent's mazeHeight to set the y-position
        return new Vector3(gridX, agent.mazeHeight, gridZ);
    }

    // Convert a world position to grid coordinates (Vector2Int)
    private Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int gridX = Mathf.RoundToInt(worldPosition.x / CellSize);
        int gridZ = Mathf.RoundToInt(worldPosition.z / CellSize);

        return new Vector2Int(gridX, gridZ);
    }
}
