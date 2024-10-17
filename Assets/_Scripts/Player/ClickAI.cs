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
    private bool _inConversation;
    
    public CinemachineTrackedDolly track;
    public float scrollSpeed = 1f; // Speed factor for scrolling

    private bool _clickedAI = false;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }

        activeCamera = playerCam;
        
        ConversationManager.OnConversationStarted += StartConversation;
        ConversationManager.OnConversationEnded += EndConversation;
        
        track = playerCam.GetCinemachineComponent<CinemachineTrackedDolly>();
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
                if (hit.collider.CompareTag("AI") && TurnBasedSystem.IsPlayerTurn() && !_inConversation)
                {
                    _clickedAI = true;
                    var DialogueManager = hit.collider.GetComponentInChildren<DialogueManager>();
                    var virtualCam = DialogueManager.GetCurrentDialogue().GetComponentInChildren<CinemachineVirtualCamera>(true); // Pass 'true' to include inactive objects
                    if (virtualCam)
                    {
                        virtualCam.gameObject.SetActive(true); // Enable the GameObject if it's disabled
                        virtualCam.enabled = true; // Enable the CinemachineVirtualCamera component
                    }
                    activeCamera = virtualCam;
                    playerCam.enabled = false;
                    DialogueManager.Increment();
                }
            }
        }

        if (_clickedAI) return;
        
        // Get the scroll wheel input (Mouse ScrollWheel returns values based on scrolling direction)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // Update the tracked dolly's path position based on the scroll input and speed factor
        track.m_PathPosition += scrollInput * scrollSpeed;
    }

    private void StartConversation()
    {
        _clickedAI = false;
        _inConversation = true;
    }
    
    private void EndConversation()
    {
        _inConversation = false;
        
        playerCam.enabled = true;
        activeCamera.gameObject.SetActive(false); // Enable the GameObject if it's disabled
        activeCamera.enabled = false; // Enable the CinemachineVirtualCamera component
        activeCamera = playerCam;
        
        //ConversationManager.Instance.EndConversation();
    }
    
}
