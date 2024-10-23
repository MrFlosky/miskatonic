using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DialogueEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickAI : MonoBehaviour
{
    public Camera mainCamera; // Reference to the camera, if null defaults to main camera
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera activeCamera;
    private bool _inConversation;

    public CinemachineTrackedDolly track;
    public float scrollSpeed = 1f; // Speed factor for scrolling

    public bool _clickedAI = false;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }

        activeCamera = playerCam;
        track = playerCam.GetCinemachineComponent<CinemachineTrackedDolly>();

        ConversationManager.OnConversationStarted += StartConversation;
        ConversationManager.OnConversationEnded += EndConversation;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (_clickedAI || !TurnBasedSystem.IsPlayerTurn() || _inConversation) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("AI"))
            {
                _clickedAI = true;
                PlayerController.Instance.clickedAI = _clickedAI;
                var DialogueManager = hit.collider.GetComponentInChildren<DialogueManager>();
                var virtualCam = DialogueManager.GetCurrentDialogue().GetComponentInChildren<CinemachineVirtualCamera>(true);
                if (virtualCam)
                {
                    virtualCam.gameObject.SetActive(true);
                    virtualCam.enabled = true;
                }
                activeCamera = virtualCam;
                playerCam.enabled = false;
                DialogueManager.Increment();
            }
        }
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        if (_clickedAI) return;

        // Get the scroll wheel input value as a Vector2
        Vector2 scrollInput = context.ReadValue<Vector2>().normalized;

        // Use the Y component for vertical scrolling
        float scrollAmount = scrollInput.y;

        // Update the tracked dolly's path position based on the scroll amount and speed factor
        track.m_PathPosition += scrollAmount/120 * scrollSpeed;
    }


    private void StartConversation()
    {
        _clickedAI = false;
        _inConversation = true;
    }

    private void EndConversation()
    {
        PlayerController.Instance.clickedAI = false;
        _inConversation = false;
        playerCam.enabled = true;
        activeCamera.gameObject.SetActive(false);
        activeCamera.enabled = false;
        activeCamera = playerCam;
    }
}
