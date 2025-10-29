using Cinemachine;
using DialogueEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using Yarn.Unity;


/// <summary>
/// 
/// Handles interactions when player clicks on AI characters
/// 
/// Responsibilities:
/// - Triggering dialouge with AI
/// - Room viewing
/// - Managing camrea trsnsitions between player camera and AI camera
/// - Handling outline visualization for AI movement paths
/// 
/// </summary>
public class ClickAI : MonoBehaviour
{
    public static ClickAI S; // rRference

    [Header("Cameras")]
    public Camera mainCamera; // Reference to the camera (defaults to main camera if null)
    public CinemachineVirtualCamera playerCam;  // Player's default camera
    public CinemachineVirtualCamera activeCamera;   // Currently active camera (AI or player)

    public bool _inConversation;   // Tracks if a conversation is ongoing
    public bool _clickedAI = false; // Track if the AI has been clicked. Flips to false once convo started.


    [Header("Camera Dolly")]
    public CinemachineTrackedDolly track;   // Dolly component used for camera path movement
    public float scrollSpeed = 1f; // Speed factor for scrolling

    [Header("Highlighting")]
    private List<Transform> activeOutlines = new List<Transform>(); // List of current outlined tiles
    private bool rightClicking = false; // Whether the player is right-clicking
    [SerializeField] Material outlineMaterial;  // Material applied when highlighting tiles

    [Header("UI")]
    public Button[] buttons;    // Buttons need to show/hide during dialogue

    [Header("Room")]
    public GameObject Maze; //grabbing the Maze itself

    [Header("Room Click Conditions")]
    private bool isSnooping = false; //check if the player is currently looking in a room
    private bool isUsingSkill = false;
    public CinemachineVirtualCamera roomCamera; //Current Room Camera




    // AI Camera currently active.
    // Used for bust rotation function calls.
    private AICamera activeAICam;

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
        switchCamButton.onClick.AddListener(SwitchToCustomCamera);
    }


    /// <summary>
    /// 
    /// Handles left-clicking on AI to start conversation
    /// Perform raycast to detect AI, camera switch, trigger dialouge, and disable gameplay buttons
    /// 
    /// </summary>
    public void OnClick(InputAction.CallbackContext context)
    {
        // do NOT allow any click events here if clicked on an ai, in convo, or its the enemy turn
        //if (_clickedAI || TurnBasedSystem.IsAITurn() || _inConversation) return; //this was the long working version.
        UIManagerLite.GameState gameState = UIManagerLite.GetCurrentState();
        if (gameState != UIManagerLite.GameState.BoardBase) return; // not allowed to perform any click actions on board/ai if not in boardbase

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

                // fade potrait
                PortraitFade portraitFade = aiGameObj.GetComponent<PortraitFade>();
                portraitFade.StartCoroutine(portraitFade.FadeOut());

                // Rotate bust
                activeAICam = aiGameObj.transform.GetChild(0).GetComponent<AICamera>();
                activeAICam.RotBustToCam();



                // Play SFX and swap camera
                AudioManager.PlaySFX(SFXType.CharacterSelection);
                _clickedAI = true;
                PlayerController.Instance.clickedAI = _clickedAI;
                var virtualCam = dialogueBank.gameObject.GetComponentInChildren<CinemachineVirtualCamera>(true);

                if (virtualCam)
                {
                    //bug fix for incosistent camera misalignment bug
                    if (roomCamera != null)
                    {
                        roomCamera.enabled = false;
                    }

                    virtualCam.gameObject.SetActive(true);
                    virtualCam.enabled = true;
                }
                activeCamera = virtualCam;
                playerCam.enabled = false;

                // Disable interaction buttons during dialouge
                // foreach (var button in buttons)
                // {
                //     Debug.Log(button.gameObject.name);
                //     button.interactable = false;
                // }

                // UIManagerLite.DisableEndTurnButton(); // ugly

                UIManagerLite.TransitionToState(UIManagerLite.GameState.Dialogue);

            }

            /* else if (hit.collider.CompareTag("Book"))
             {

                 hit.collider.GetComponent<Books>().ChangeBook();
             }*/

            //Check is tile is a room
            else if (hit.collider.gameObject.GetComponentInParent<MazeCellObject>() != null && hit.collider.gameObject.GetComponentInParent<MazeCellObject>().roomType != "Hallway")
            {
                if (Keyboard.current.leftShiftKey.isPressed)
                {
                    cameraRoomManagement(hit);
                }
            }
        }
    }

    private void cameraRoomManagement(RaycastHit Room)
    {
        if (roomCamera)
        {
            roomCamera.enabled = false;
        }
        if (Room.collider.gameObject.GetComponentInParent<MazeCellObject>().roomType != null)
        {
            isSnooping = true;
            //Set the room camera to the room type being clicked
            switch (Room.collider.gameObject.GetComponentInParent<MazeCellObject>().roomType)
            {
                //camera blend on room click
                case "Bedroom":
                    roomCamera = Maze.gameObject.GetComponent<MazeRenderer>().bedroomCam;
                    break;
                case "Library":
                    roomCamera = Maze.gameObject.GetComponent<MazeRenderer>().libraryCam;
                    break;
                case "Dining Room":
                    roomCamera = Maze.gameObject.GetComponent<MazeRenderer>().diningCam;
                    break;
                case "Dungeon":
                    roomCamera = Maze.gameObject.GetComponent<MazeRenderer>().dungeonCam;
                    break;
                case "Ritual Room":
                    roomCamera = Maze.gameObject.GetComponent<MazeRenderer>().ritualCam;
                    break;
                case "Cemetery":
                    roomCamera = Maze.gameObject.GetComponent<MazeRenderer>().cemeteryCam;
                    break;
            }

            //Check if a skill is in use
            if (this.gameObject.GetComponent<PlayerController>().activeSkill != null)
            {
                isUsingSkill = true;
            }
            if (this.gameObject.GetComponent<PlayerController>().activeSkill == null)
            {
                isUsingSkill = false;
            }

            //Blend Camera if skill not in use
            if (roomCamera && !isUsingSkill)
            {
                if (roomCamera.enabled == false)
                {
                    roomCamera.gameObject.SetActive(true);
                    roomCamera.enabled = true;
                }
            }
            activeCamera = roomCamera;
            playerCam.enabled = false;
        
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

        //Leave Room On Right Click
        if (isSnooping && !_inConversation)
        {
            activeCamera.enabled = false;
            isSnooping = false;
            playerCam.enabled = true;
            if (activeCamera.enabled)
            {
                activeCamera = playerCam;
                activeCamera.enabled = true;
            }
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

        activeCamera = playerCam;

        // foreach (var button in buttons)
        // {
        //     button.interactable = true;
        // }

        // // im sorry this is ugly
        // if (!UIManagerLite.DealerButtonIsActive())
        // {
        //     UIManagerLite.EnableEndTurnButton();
        // }

        UIManagerLite.TransitionToState(UIManagerLite.GameState.BoardBase);

        // upon ending conversation, reset bust rotation
        // also handle setting gameobject back to inactive after done within funct
        activeAICam.ResetBustRotation();


    }

    private void Update()
    {
        // Clear outlines unless hodling right-click in player's turn
        if (rightClicking && TurnBasedSystem.IsPlayerTurn()) return;
        ClearOutlines();
        
    }

    [SerializeField] CinemachineVirtualCamera storyboardCam;
    [SerializeField] Button switchCamButton;


    /// <summary>
    /// Switch to camera for story board
    /// </summary>
    public void SwitchToCustomCamera()
    {
        // Disable whatever camera is currently active
        if (activeCamera != null)
        {
            activeCamera.enabled = false;
            activeCamera.gameObject.SetActive(false);
        }

        // Enable your new camera
        storyboardCam.gameObject.SetActive(true);
        storyboardCam.enabled = true;

        // Track it as the active camera
        activeCamera = storyboardCam;

        switchCamButton.onClick.RemoveAllListeners(); // switch what function button calls
        switchCamButton.onClick.AddListener(SwitchToPlayerCamera); // switch what function button calls

        UIManagerLite.TransitionToState(UIManagerLite.GameState.Storyboard);
    }




    /// <summary>
    /// Switch back to player camera from story board
    /// </summary>
    public void SwitchToPlayerCamera()
    {

        // Disable whatever camera is currently active
        if (activeCamera != null)
        {
            activeCamera.enabled = false;
            activeCamera.gameObject.SetActive(false);
        }

        // Enable the player camera
        playerCam.gameObject.SetActive(true);
        playerCam.enabled = true;

        // Track it as the active camera
        activeCamera = playerCam;

        switchCamButton.onClick.RemoveAllListeners(); // switch what function button calls
        switchCamButton.onClick.AddListener(SwitchToCustomCamera); // switch what function button calls

        UIManagerLite.TransitionToState(UIManagerLite.GameState.BoardBase);
    }

    /// <summary>
    /// bit ugly
    /// </summary>
    public void SetAICam(AICamera aicam)
    {
        activeAICam = aicam;
    }


}


// This does not seem to be in use currently.
// public class AIHighlighter : MonoBehaviour
// {
//     [SerializeField] private Material outlineMaterial;
//     private List<Transform> activeOutlines = new List<Transform>();

//     public void HighlightPath(List<MazeCell> path, int tileSpeed)
//     {

//     }

//     public void ClearOutlines()
//     {
//         foreach (var outline in activeOutlines)
//         {
//             var newMaterials = new Material[1];
//             newMaterials[0] = outline.GetComponent<MeshRenderer>().materials[0];
//             outline.GetComponent<MeshRenderer>().materials = newMaterials;
//         }
//         activeOutlines.Clear(); // Clear the list after disabling
//     }



// }
