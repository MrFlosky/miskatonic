using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClickToMove : MonoBehaviour
{
    public Camera mainCamera; // Reference to the camera, if null defaults to main camera
    public MazeRenderer mazeRenderer; // Reference to the GridAgentNavigation script
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

            // Perform a raycast to find the clicked position on the NavMesh
            if (Physics.Raycast(ray, out hit))
            {
                // Get the center of the grid cell closest to the hit point
                Vector3 targetPosition = GetGridCenter(hit.point);

                // Set the destination of the grid agent
                agent.SetDestination(new Vector3(targetPosition.x, 0.5f, targetPosition.z));
            }
        }
    }
    
    

    // Calculate the center of the grid cell
    private Vector3 GetGridCenter(Vector3 position)
    {
        float gridX = Mathf.Round(position.x / CellSize) * CellSize;
        float gridZ = Mathf.Round(position.z / CellSize) * CellSize;

        return new Vector3(gridX, position.y, gridZ);
    }
}