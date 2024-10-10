using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DialogueEditor;
using UnityEngine;
using UnityEngine.InputSystem.HID;

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
                if (hit.collider.CompareTag("AI"))
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            playerCam.enabled = true;
            ConversationManager.Instance.EndConversation();
            activeCamera.gameObject.SetActive(false); // Enable the GameObject if it's disabled
            activeCamera.enabled = false; // Enable the CinemachineVirtualCamera component
            
            activeCamera = playerCam;
        }
    }
    
}
