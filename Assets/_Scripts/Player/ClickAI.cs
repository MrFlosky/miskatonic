using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DialogueEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;


/// <summary>
/// 
/// Handles interactions when player clicks on AI characters
/// 
/// Responsibilities:
/// - Triggering dialouge with AI
/// - Managing camrea trsnsitions between player camera and AI camera
/// - Handling outline visualization for AI movement paths
/// - Disabling/enabling UI buttons during conversations
/// 
/// </summary>
public class ClickAI : MonoBehaviour
{
    public static ClickAI S; // rRference

    [Header("Cameras")]
    public Camera mainCamera; // Reference to the camera (defaults to main camera if null)
    public CinemachineVirtualCamera playerCam;  // Player's default camera
    public CinemachineVirtualCamera activeCamera;   // Currently active camera (AI or player)

    private bool _inConversation;   // Tracks if a conversation is ongoing
    public bool _clickedAI = false; // Track if the AI has been clicked.


    [Header("Camera Dolly")]
    public CinemachineTrackedDolly track;   // Dolly component used for camera path movement
    public float scrollSpeed = 1f; // Speed factor for scrolling

    [Header("Highlighting")]
    private List<Transform> activeOutlines = new List<Transform>(); // List of current outlined tiles
    private bool rightClicking = false; // Whether the player is right-clicking
    [SerializeField] Material outlineMaterial;  // Material applied when highlighting tiles

    [Header("UI")]
    public Button[] buttons;    // Buttons need to show/hide during dialogue

    [SerializeField] private AIClickOptions aiClickOptions;

    private void Start()
    {
        S = this;

        // Dafault to main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }

        // Set up initial active camera
        activeCamera = playerCam;
        track = playerCam.GetCinemachineComponent<CinemachineTrackedDolly>();
    }


    /// <summary>
    /// 
    /// Handles left-clicking on AI to start conversation
    /// Perform raycast to detect AI, camera switch, trigger dialouge, and disable gameplay buttons
    /// 
    /// </summary>
    public void OnClick(InputAction.CallbackContext context)
    {
        if (_clickedAI || !TurnBasedSystem.IsPlayerTurn() || _inConversation) return;

        // Get moude position based on current input
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse 
            ? Mouse.current.position.ReadValue() 
            : VirtualMouseUI.Instance.GetVirtualMousePosition();
         
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            if (hit.collider.CompareTag("AI"))
            {
                GameObject aiGameObj = hit.collider.gameObject;
                DialogueBank dialogueBank = aiGameObj.GetComponent<DialogueBank>();
                // No dialouge needed
                if (dialogueBank.alreadyTalked)
                {
                    dialogueBank.AlreadyTalked();
                    return;
                }

                // Prevent interaction if AI is out of dialouge
                if (dialogueBank.GetRemainingDialogueCount() == 0)
                {
                    dialogueBank.OutOfDialogues();
                    return;
                }

                // Mark AI as talked and fade potrait
                dialogueBank.alreadyTalked = true;
                PortraitFade portraitFade = aiGameObj.GetComponent<PortraitFade>();
                portraitFade.StartCoroutine(portraitFade.FadeOut());



                // Play SFX and swap camera
                AudioManager.PlaySFX(SFXType.CharacterSelection);
                _clickedAI = true;
                PlayerController.Instance.clickedAI = _clickedAI;
                var virtualCam = dialogueBank.gameObject.GetComponentInChildren<CinemachineVirtualCamera>(true);

                if (virtualCam)
                {
                    virtualCam.gameObject.SetActive(true);
                    virtualCam.enabled = true;
                }
                activeCamera = virtualCam;
                playerCam.enabled = false;

                // Disable interaction buttons during dialouge
                foreach (var button in buttons)
                {
                    Debug.Log(button.gameObject.name);
                    button.interactable = false;
                }

                UIManagerLite.DisableEndTurnButton(); // ugly

                // Show option panel instead of starting dialogue immediately
                aiClickOptions.ShowOptions(dialogueBank);


            }
        }
    }


    /// <summary>
    /// 
    /// Handle mouse scroll input to move the dolly camera along its path
    /// Disable if AI was clicked
    /// 
    /// </summary>
    public void OnScroll(InputAction.CallbackContext context)
    {
        if (_clickedAI) return;

        // Get the scroll wheel input value as a Vector2
        Vector2 scrollInput = context.ReadValue<Vector2>().normalized;

        // Use the Y component for vertical scrolling
        float scrollAmount = scrollInput.y;

        // Update the tracked dolly's path position based on the scroll amount and speed factor
        track.m_PathPosition += scrollAmount / 120 * scrollSpeed;
    }


    /// <summary>
    /// 
    /// Handle right-click input to preview AI movement paths by outlining titles
    /// Clear outlines when releasing right-click or clicking elsewhere
    /// 
    /// </summary>
    public void OnRightClick(InputAction.CallbackContext context)
    {
        rightClicking = true;

        if (_clickedAI || !TurnBasedSystem.IsPlayerTurn() || _inConversation)
        {
            ClearOutlines();
            return;
        }

        var mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse 
            ? Mouse.current.position.ReadValue() 
            : VirtualMouseUI.Instance.GetVirtualMousePosition();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

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

                        for (int i = 0; i < Mathf.Min(agentNavigation.tilesPerTurn, agentNavigation.storedPath.Count); i++)
                        {
                            MazeCell cell = agentNavigation.storedPath[i];
                            if (MazeRenderer.Instance.cellGameObjects.TryGetValue(cell.position, out GameObject cellObject))
                            {
                                for (int j = 0; j < cellObject.transform.childCount; j++)
                                {
                                    if (cellObject.transform.GetChild(j).gameObject.layer == LayerMask.NameToLayer("Ground"))
                                        childWithTag.Add(cellObject.transform.GetChild(j));
                                }

                            }
                        }

                        // Apply outline material
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
                        for (int i = 0; i < Mathf.Min(agentNavigation.tilesPerTurn, agentNavigation.path.Count); i++)
                        {
                            MazeCell cell = agentNavigation.path[i];
                            if (MazeRenderer.Instance.cellGameObjects.TryGetValue(cell.position, out GameObject cellObject))
                            {
                                for (int j = 0; j < cellObject.transform.childCount; j++)
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


    /// <summary>
    /// 
    /// Remove outlines from all currently highlighted tiles
    /// 
    /// </summary>
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


    /// <summary>
    /// Apparently this is important to call at start of conversation, not sure why!
    /// Seems to disable camera controls at end of convo if this is not called?
    /// </summary>
    public void StartConversation()
    {
        _clickedAI = false;
        _inConversation = true;
    }


    /// <summary>
    /// 
    /// Ends the current conversation:
    /// - Resets camera back to player
    /// - Enable buttons
    /// - Reset AI click state (clickedAI)
    /// 
    /// </summary>
    public void EndConversation()
    {
        PlayerController.Instance.clickedAI = false;
        _inConversation = false;

        if (playerCam)
            playerCam.enabled = true;
        else
        {
            playerCam = GameManager.Instance.playerCam;
            playerCam.enabled = true;
        }

        activeCamera.gameObject.SetActive(false);
        activeCamera.enabled = false;
        activeCamera = playerCam;

        foreach (var button in buttons)
        {
            button.interactable = true;
        }

        // im sorry this is ugly
        if (!UIManagerLite.DealerButtonIsActive())
        {
            UIManagerLite.EnableEndTurnButton();
        }
    }

    private void Update()
    {
        // Clear outlines unless hodling right-click in player's turn
        if (rightClicking && TurnBasedSystem.IsPlayerTurn()) return;
        ClearOutlines();
    }



}


// This does not seem to be in use currently.
public class AIHighlighter : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    private List<Transform> activeOutlines = new List<Transform>();

    public void HighlightPath(List<MazeCell> path, int tileSpeed)
    {

    }

    public void ClearOutlines()
    {
        foreach (var outline in activeOutlines)
        {
            var newMaterials = new Material[1];
            newMaterials[0] = outline.GetComponent<MeshRenderer>().materials[0];
            outline.GetComponent<MeshRenderer>().materials = newMaterials;
        }
        activeOutlines.Clear(); // Clear the list after disabling
    }


}
