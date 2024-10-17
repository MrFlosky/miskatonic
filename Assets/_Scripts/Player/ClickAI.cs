using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DialogueEditor;
using UnityEngine;

public class ClickAI : MonoBehaviour
{
    public Camera mainCamera; // Reference to the camera, if null defaults to main camera
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera activeCamera;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }

        activeCamera = playerCam;
        
        ConversationManager.OnConversationEnded += EndConversation;
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
                if (hit.collider.CompareTag("AI") && TurnBasedSystem.IsPlayerTurn())
                {
                    var virtualCam = hit.collider.GetComponentInChildren<CinemachineVirtualCamera>(true); // Pass 'true' to include inactive objects
                    if (virtualCam)
                    {
                        virtualCam.gameObject.SetActive(true); // Enable the GameObject if it's disabled
                        virtualCam.enabled = true; // Enable the CinemachineVirtualCamera component
                    }
                    activeCamera = virtualCam;
                    playerCam.enabled = false;
                }
            }
        }
    }

    private void EndConversation()
    {
        playerCam.enabled = true;
        activeCamera.gameObject.SetActive(false); // Enable the GameObject if it's disabled
        activeCamera.enabled = false; // Enable the CinemachineVirtualCamera component
        activeCamera = playerCam;
    }
    
}
