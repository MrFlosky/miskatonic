using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DialogueEditor;
using Unity.VisualScripting;
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
    
    private List<Transform> activeOutlines = new List<Transform>();

    private bool rightClicking = false;
    
    [SerializeField] Material outlineMaterial;

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
        
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        // Determine the rooms being selected for a switch
        Ray ray = SkillSystem.GET_CAM().ScreenPointToRay(mousePos);
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
                DialogueManager.usedDialogue = true;
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
    
    public void OnRightClick(InputAction.CallbackContext context)
    {
        rightClicking = true;
        if (_clickedAI || !TurnBasedSystem.IsPlayerTurn() || _inConversation)
        {
            ClearOutlines();
            return;
        }
        
        var mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();

        Ray ray = SkillSystem.GET_CAM().ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("AI"))
            {
                if (context.started)
                {
                    var agentNavigation = hit.collider.GetComponent<AgentNavigation>();
                
                    // Highlight the agent's planned path for the next turn
                    if (agentNavigation != null && agentNavigation.storedPath.Count > 0)
                    {
                        // Clear any previously active outlines
                        ClearOutlines();

                        List<Transform> childWithTag = new List<Transform>();
                        for (int i = 0; i < Mathf.Min(agentNavigation.tileSpeed, agentNavigation.storedPath.Count); i++)
                        {
                            MazeCell cell = agentNavigation.storedPath[i];
                            if (MazeRenderer.Instance.cellGameObjects.TryGetValue(cell.position, out GameObject cellObject))
                            {
                                for (int j = 0 ; j < cellObject.transform.childCount; j++)
                                {
                                    if (cellObject.transform.GetChild(j).gameObject.layer == LayerMask.NameToLayer("Ground"))
                                        childWithTag.Add(cellObject.transform.GetChild(j));
                                }
                                
                            }
                        }
                        
                        foreach (var child in childWithTag)
                        {
                            var materials = child.GetComponent<MeshRenderer>().materials;
                                    
                            var newMaterials = new Material[materials.Length + 1];

                            for (int i = 0; i < materials.Length; i++)
                            {
                                newMaterials[i] = materials[i];
                            }

                            newMaterials[^1] = outlineMaterial;

                            child.GetComponent<MeshRenderer>().materials = newMaterials;
                                    
                            activeOutlines.Add(child);
                        }
                    }
                    else if (agentNavigation != null && agentNavigation.path.Count > 0)
                    {
                        // Clear any previously active outlines
                        ClearOutlines();

                        List<Transform> childWithTag = new List<Transform>();
                        for (int i = 0; i < Mathf.Min(agentNavigation.tileSpeed, agentNavigation.path.Count); i++)
                        {
                            MazeCell cell = agentNavigation.path[i];
                            if (MazeRenderer.Instance.cellGameObjects.TryGetValue(cell.position, out GameObject cellObject))
                            {
                                for (int j = 0 ; j < cellObject.transform.childCount; j++)
                                {
                                    if (cellObject.transform.GetChild(j).gameObject.layer == LayerMask.NameToLayer("Ground"))
                                        childWithTag.Add(cellObject.transform.GetChild(j));
                                }
                                
                            }
                        }
                        
                        foreach (var child in childWithTag)
                        {
                            var materials = child.GetComponent<MeshRenderer>().materials;
                                    
                            var newMaterials = new Material[materials.Length + 1];

                            for (int i = 0; i < materials.Length; i++)
                            {
                                newMaterials[i] = materials[i];
                            }

                            newMaterials[^1] = outlineMaterial;

                            child.GetComponent<MeshRenderer>().materials = newMaterials;
                                    
                            activeOutlines.Add(child);
                        }
                    }
                }
                else
                {
                    ClearOutlines();
                }

            }
            else
            {
                ClearOutlines();
            }
        }
        else if (context.performed || context.canceled)
        {
            rightClicking = false;
            ClearOutlines();
        }
    }

    private void ClearOutlines()
    {
        foreach (var outline in activeOutlines)
        {
            var newMaterials = new Material[1];
            newMaterials[0] = outline.GetComponent<MeshRenderer>().materials[0];
            outline.GetComponent<MeshRenderer>().materials = newMaterials;
        }
        activeOutlines.Clear(); // Clear the list after disabling
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

    private void Update()
    {
        if (rightClicking && TurnBasedSystem.IsPlayerTurn()) return;
        ClearOutlines();
    }
}
