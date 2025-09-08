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
/// NEEDS REFACTOR BADLY!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
/// This class is responsible for what happens upon clicking AI. Including:
/// - Presenting dialogue
/// - Camera transition
/// </summary>
public class ClickAI : MonoBehaviour
{
    public static ClickAI S;

    public Camera mainCamera; // Reference to the camera, if null defaults to main camera
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera activeCamera;
    /// <summary>
    /// Needed in order to manually set interactable and block raycasts on dialogue presenter.
    /// for some reason option presenter handles this auto but this one doesnt.
    /// </summary>
    [SerializeField] private CanvasGroup evilCanvasGroup;
    private bool _inConversation;

    public CinemachineTrackedDolly track;
    public float scrollSpeed = 1f; // Speed factor for scrolling

    // Track if the AI has been clicked.
    public bool _clickedAI = false;

    private List<Transform> activeOutlines = new List<Transform>();

    private bool rightClicking = false;

    [SerializeField] Material outlineMaterial;

    // buttons need to show/hide during dialogue
    public Button[] buttons;


    private void Start()
    {
        S = this;
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Use main camera if none specified
        }

        activeCamera = playerCam;
        track = playerCam.GetCinemachineComponent<CinemachineTrackedDolly>();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (_clickedAI || !TurnBasedSystem.IsPlayerTurn() || _inConversation) return;

        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            if (hit.collider.CompareTag("AI"))
            {
                GameObject aiGameObj = hit.collider.gameObject;
                DialogueBank dialogueBank = aiGameObj.GetComponent<DialogueBank>();
                // If AI was already clicked, play notify dialogue
                if (dialogueBank.alreadyTalked)
                {
                    dialogueBank.AlreadyTalked();
                    return;
                }
                dialogueBank.alreadyTalked = true;



                // camera and sound effect
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

                // disable buttons 
                foreach (var button in buttons)
                {
                    Debug.Log(button.gameObject.name);
                    button.interactable = false;
                }



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
        track.m_PathPosition += scrollAmount / 120 * scrollSpeed;
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
                        for (int i = 0; i < Mathf.Min(agentNavigation.tileSpeed, agentNavigation.storedPath.Count); i++)
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
        evilCanvasGroup.interactable = true;
        evilCanvasGroup.blocksRaycasts = true;

        _clickedAI = false;
        _inConversation = true;
    }


    /// <summary>
    /// Returns the camera back to its main state and some other stuff?
    /// </summary>
    public void EndConversation()
    {
        evilCanvasGroup.interactable = false;
        evilCanvasGroup.blocksRaycasts = false;

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
    }

    private void Update()
    {
        if (rightClicking && TurnBasedSystem.IsPlayerTurn()) return;
        ClearOutlines();
    }



}
