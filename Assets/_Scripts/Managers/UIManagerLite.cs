using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// TODO: REMOVE REDUNDANT DISPATCH OR DEALER GAME STATE. IDK WHY THIS WAS ADDED BUT THEY ARE THE SAME THING

/// <summary>
/// This class is responsible for bandaid UI management.
/// Im calling this lite because not everything relevant to UI management is here yet, but
/// might as well start creating a solution.
/// </summary>
public class UIManagerLite : MonoBehaviour
{
    public static UIManagerLite S;

    // ===== STATE MACHINE =====
    public enum GameState
    {
        BoardBase,      // Central hub state - all states can transition here
        Dialogue,
        SkillInUse,
        Storyboard,
        Dispatch,
        EnemyTurn
    }

    private GameState currentState;
    // Contains key, value where key is state and value is all states that key is allowed to transition to.
    private Dictionary<GameState, List<GameState>> allowedTransitions;
    
    #region UI References
    /// <summary>
    /// Component attached to new dialogue system's line presenter.
    /// For wahtever reason, it does not automatically turn off block raycasts and interactable
    /// when a dialogue is complete. Doing that manually here.
    /// </summary>
    [SerializeField] private CanvasGroup evilCanvasGroup;
    [SerializeField] private CanvasGroup lineAndOptionsCG;
    [SerializeField] private CanvasGroup optionsPresenterCG;

    // Buttons
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button dealerButton; // button for starting mission dealer interaction
    [SerializeField] private Button dealerReturnToBoardButton; // button for returning to board base from dealer
    [SerializeField] private Button rerollButton; // button for rerolling cards
    [SerializeField] private Button storyboardButton;
    [SerializeField] private Button[] potraitButtons;
    [SerializeField] private GameObject portraitOb; // holds all potraits and text
    [SerializeField] PortraitFade[] portraitFades;
    // Responsible for all relevant UI functions for skills
    [SerializeField] private SkillsCollection skillUICollection;
    [SerializeField] private SkillSystem skillPointUICollection;

    // Canvases
    [SerializeField] private Canvas dealerCanvas;

    // Script that manages model highlighting
    [SerializeField] private OutlineHighlight outlineManager;

    #endregion

    void Start()
    {
        S = this;
        SetupTransitions();
        currentState = GameState.BoardBase; // Default starting state
        portraitFades = S.portraitOb.GetComponentsInChildren<PortraitFade>();
    }

    #region State Machine Methods
    
    private void SetupTransitions()
    {
        allowedTransitions = new Dictionary<GameState, List<GameState>>()
        {
            { 
                GameState.BoardBase, 
                new List<GameState>
                {
                    GameState.Dialogue,
                    GameState.SkillInUse,
                    GameState.Storyboard,
                    GameState.Dispatch,
                    GameState.EnemyTurn,
                    GameState.BoardBase,
                } 
            },
            { 
                GameState.Dialogue, 
                new List<GameState> 
                { 
                    GameState.BoardBase
                } 
            },
            { 
                GameState.SkillInUse, 
                new List<GameState>
                {
                    GameState.BoardBase
                } 
            },
            { 
                GameState.Storyboard, 
                new List<GameState> 
                { 
                    GameState.BoardBase,
                    GameState.Dispatch
                } 
            },
            { 
                GameState.Dispatch, 
                new List<GameState> 
                { 
                    GameState.BoardBase
                } 
            },
            { 
                GameState.EnemyTurn, 
                new List<GameState> 
                { 
                    GameState.BoardBase
                } 
            }
        };
    }
    
    private static bool CanTransition(GameState targetState)
    {
        if (!S.allowedTransitions.ContainsKey(S.currentState))
            return false;
            
        return S.allowedTransitions[S.currentState].Contains(targetState);
    }
    
    public static bool TransitionToState(GameState newState)
    {
        // Check if transition is allowed
        if (!CanTransition(newState))
        {
            //Debug.Log($"Called from:\n{System.Environment.StackTrace}");

            Debug.LogWarning($"Cannot transition from {S.currentState} to {newState}");
            return false;
        }
        
        GameState previousState = S.currentState;
        
        // Exit current state (knows where it's going TO)
        //ExitState(S.currentState, newState);
        
        // Change state
        S.currentState = newState;
        Debug.Log($"Transitioned from {previousState} to: {newState}");
        
        // Enter new state (knows where it came FROM)
        EnterState(newState, previousState);
        
        return true;
    }

    /// <summary>
    /// Enter a state with context of where we came from
    /// </summary>
    private static void EnterState(GameState state, GameState fromState)
    {
        switch (state)
        {
            // ==================================================================================
            // ENTERING: BoardBase (Central Hub State)
            // ==================================================================================
            case GameState.BoardBase:
                // Different behavior based on where we came from
                // TRANSITION: Dialogue → BoardBase
                if (fromState == GameState.Dialogue)
                {
                    Debug.Log("Dialogue ended, back to board");
                    if (!DispatchSystem.dealerDealtWith) // only set dealer button active if not yet dealt with
                    {
                        DisableEndTurnButton();
                        S.dealerButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        EnableEndTurnButton();
                    }

                    S.storyboardButton.gameObject.SetActive(true);
                    S.endTurnButton.gameObject.SetActive(true);
                    S.skillUICollection.ENABLE_BUTTONS();
                    
                    
                    foreach(var P in S.portraitFades)
                    {
                        P.StartCoroutine(P.FadeIn());
                    }

                    S.outlineManager.ResumeOutlining();
                }
                // TRANSITION: SkillInUse → BoardBase
                else if (fromState == GameState.SkillInUse)
                {
                    Debug.Log("Skill effect complete, back to board");

                    if (!DispatchSystem.dealerDealtWith) // only set dealer button active if not yet dealt with
                    {
                        DisableEndTurnButton();
                        S.dealerButton.gameObject.SetActive(true);
                        S.dealerButton.interactable = true;
                    }
                    else
                    {
                        EnableEndTurnButton();
                    }

                    S.skillUICollection.ENABLE_BUTTONS();
                    S.EnablePortraitButtons();

                }
                // TRANSITION: Storyboard → BoardBase
                else if (fromState == GameState.Storyboard)
                {
                    Debug.Log("Storyboard ended, back to game board");
                    if (!DispatchSystem.dealerDealtWith)
                    {
                        S.dealerButton.gameObject.SetActive(true);
                        S.dealerButton.interactable = true;
                    }
                    S.endTurnButton.gameObject.SetActive(true);
                    S.skillPointUICollection.DISPLAY_PTS();
                    S.skillUICollection.DISPLAY_SKILLS();
                    foreach(var P in S.portraitFades)
                    {
                        P.StartCoroutine(P.BoardBaseReset());
                    }
                    
                    S.outlineManager.ResumeOutlining();
                }
                // TRANSITION: Dispatch → BoardBase
                else if (fromState == GameState.Dispatch)
                {
                    Debug.Log("Dealer interaction complete, back to board");
                    if(DispatchSystem.dealerDealtWith)
                    {
                        S.dealerButton.gameObject.SetActive(false);
                        S.endTurnButton.gameObject.SetActive(true);
                        EnableEndTurnButton(); 
                    } else
                    {
                        S.dealerButton.gameObject.SetActive(true);
                        S.endTurnButton.gameObject.SetActive(false);
                    }
                       
                    S.dealerReturnToBoardButton.gameObject.SetActive(false);
                    S.storyboardButton.gameObject.SetActive(true);
                    
                    S.skillUICollection.ENABLE_BUTTONS();
                    foreach(var P in S.portraitFades)
                    {
                        P.StartCoroutine(P.FadeIn());
                    }

                    S.outlineManager.ResumeOutlining();
                }
                // TRANSITION: EnemyTurn → BoardBase
                else if (fromState == GameState.EnemyTurn)
                {
                    Debug.Log("Enemy turn ended, your turn now!");
                    DisableEndTurnButton();
                    S.dealerButton.gameObject.SetActive(true); // only set dealer button as active coming from enemy turn to force interaction
                    S.storyboardButton.gameObject.SetActive(true);
                    S.endTurnButton.gameObject.SetActive(true);
                    S.skillUICollection.DISPLAY_SKILLS();
                    S.skillPointUICollection.DISPLAY_PTS();
                    
                }
                break;

              
            // ==================================================================================
            // ENTERING: Dialogue
            // ==================================================================================
            case GameState.Dialogue:

                // TRANSITION: BoardBase → Dialogue
                if (fromState == GameState.BoardBase)
                {
                    Debug.Log("Dialogue triggered");

                    S.dealerButton.gameObject.SetActive(false);
                    S.storyboardButton.gameObject.SetActive(false);
                    S.endTurnButton.gameObject.SetActive(false);
                    S.skillUICollection.DEACTIVATE_SKILLS_NAKED(); // ends any skill currently in use
                    S.skillUICollection.DISABLE_BUTTONS();
                    foreach(var P in S.portraitFades)
                    {
                        P.StartCoroutine(P.FadeOut());
                    }

                    S.outlineManager.PauseOutlining();
                }
                break;

            // ==================================================================================
            // ENTERING: SkillInUse
            // ==================================================================================
            case GameState.SkillInUse:

                // TRANSITION: BoardBase → SkillInUse
                if (fromState == GameState.BoardBase)
                {
                    Debug.Log("Skill activated");

                    if (!DispatchSystem.dealerDealtWith)
                    {
                        S.dealerButton.gameObject.SetActive(false);
                        S.dealerButton.interactable = false;
                    }
                    DisableEndTurnButton();
                    S.skillUICollection.DISABLE_BUTTONS();
                    S.DisablePortaitButtons();
                }
                break;

            // ==================================================================================
            // ENTERING: Storyboard
            // ==================================================================================
            case GameState.Storyboard:

                // TRANSITION: BoardBase → Storyboard
                if (fromState == GameState.BoardBase)
                {
                    Debug.Log("Coming from board, to storyboard");

                    if (!DispatchSystem.dealerDealtWith)
                    {
                        S.dealerButton.gameObject.SetActive(false);
                    }
                    S.endTurnButton.gameObject.SetActive(false);
                    S.skillPointUICollection.HIDE_PTS();
                    S.skillUICollection.HIDE_SKILLS();
                    foreach(var p in S.portraitFades)
                    {
                        p.StartCoroutine(p.FadeIn());
                      
                    }

                    S.outlineManager.PauseOutlining();
                }
                break;
            // ==================================================================================
            // ENTERING: Dispatch
            // ==================================================================================
            case GameState.Dispatch:

                // TRANSITION: BoardBase → Dispatch
                if (fromState == GameState.BoardBase)
                {
                    Debug.Log("Coming from board, to dispatch");

                    DisableEndTurnButton();
                    Debug.Log("Entering dealer interaction");

                    S.dealerReturnToBoardButton.gameObject.SetActive(true);
                    S.storyboardButton.gameObject.SetActive(false);
                    S.endTurnButton.gameObject.SetActive(false);
                    S.dealerButton.gameObject.SetActive(false);
                    S.skillUICollection.DISABLE_BUTTONS();
                    foreach(var P in S.portraitFades)
                    {
                        P.StartCoroutine(P.FadeOut());
                    }

                    S.outlineManager.PauseOutlining();
                }
                break;

            // ==================================================================================
            // ENTERING: EnemyTurn
            // ==================================================================================
            case GameState.EnemyTurn:

                // TRANSITION: BoardBase → EnemyTurn
                if (fromState == GameState.BoardBase)
                {
                    Debug.Log("Enemy turn starting");
                    S.storyboardButton.gameObject.SetActive(false);
                    S.endTurnButton.gameObject.SetActive(false);
                    S.skillUICollection.DEACTIVATE_SKILLS_NAKED();
                    S.skillUICollection.HIDE_SKILLS();
                    S.skillPointUICollection.HIDE_PTS();
                    
                }
                break;
        }
    }
    
    #endregion


    public static GameState GetCurrentState()
    {
        return S.currentState;
    }

    public static bool IsInState(GameState state)
    {
        return S.currentState == state;
    }

    // ===== EXISTING UI METHODS =====
    
    /// <summary>
    /// Block raycasts and turn ON interaction for dialogue PRESENTER.
    /// </summary>
    public static void EnableDialogueSettings()
    {
        S.evilCanvasGroup.interactable = true;
        S.evilCanvasGroup.blocksRaycasts = true;

        S.lineAndOptionsCG.interactable = true;
        S.lineAndOptionsCG.blocksRaycasts = true;

        S.optionsPresenterCG.interactable = true;
        S.optionsPresenterCG.blocksRaycasts = true;
        S.optionsPresenterCG.alpha = 1;
    }

    /// <summary>
    /// Stop blocking raycasts and turn off interaction for dialogue PRESENTER.
    /// </summary>
    public static void DisableDialogueSettings()
    {
        S.evilCanvasGroup.interactable = false;
        S.evilCanvasGroup.blocksRaycasts = false;

        S.lineAndOptionsCG.interactable = false;
        S.lineAndOptionsCG.blocksRaycasts = false;

        S.optionsPresenterCG.interactable = false;
        S.optionsPresenterCG.blocksRaycasts = false;
        S.optionsPresenterCG.alpha = 0;
    }


    
        void DisablePortaitButtons()
    {
        foreach (var P in S.portraitFades)
        {
            P.PortraitButton.enabled = false;
        }
    }

    /// <summary>
    /// the following were changed to account for portrait fade rework. It now only enables those portraits that haven't been clicked this turn
    /// </summary>

    void EnablePortraitButtons()
    {
        foreach(var P in S.portraitFades)
        {
            if (!P.clicked) P.PortraitButton.enabled = true;
        }
    }



    /// <summary>
    /// these were originally in wrappers to be called by other classes
    /// </summary>
    public static void DisableEndTurnButton()
    {
        S.endTurnButton.interactable = false;
    }

    public static void EnableEndTurnButton()
    {
        S.endTurnButton.interactable = true;
    }
}