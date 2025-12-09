using System.Collections;
using System.Collections.Generic;
using CharacterData;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;


public class Fear : MonoBehaviour, ITurnObserver
{

    public int currentFear = 10;
    /// <summary>
    /// 2 important notes:
    /// - Max value on AI slider automatically updated when this is changed.
    /// - HOWEVER, the fear WIN meter max value (win value) needs to be manually adjusted to account for totaled AI thresholds.
    /// </summary>
    public int fearThreshold = 25;
    //string weakness;

    /// <summary>
    /// Characters current fear state as set by dialogues.
    /// </summary>
    private FearState fearState = FearState.Neutral;

    /// <summary>
    /// Character's individual slider wrapper, as displayed with the portrait.
    /// Wrapped used bc need to enable/disable monobehavior and mono cannot enable
    /// within itself when disabled.
    /// </summary>
    private IndividualFearMeter individualSlider;

    /// <summary>
    /// Number of turns until this character returns to neutral fear state.
    /// </summary>
    private int turnsUntilNeutralState = 0;

    /// <summary>
    /// Reference to who this fear belongs to
    /// </summary>
    private Character characterEnum;

    private HashSet<FearState> prevStates = new HashSet<FearState> { FearState.Neutral };

    /// <summary>
    /// Variables for fear add debugging.
    /// </summary>
    #if UNITY_EDITOR
    private static bool showDebug = false;
    private static int debugSlotCounter = 0;
    private int myDebugSlot;
    private List<string> lastChanges = new List<string>();
    private bool expanded = false; 
    #endif



    void Awake()
    {
        // awake since apart of same game obj
        individualSlider = GetComponent<IndividualFearMeter>();
        characterEnum = GetComponent<DialogueBank>().GetCharacter();
        Color c = FearCollection.S.GetStateColor(FearState.Neutral);
        fearState = FearState.Neutral;
        if (sliderFill != null)
            sliderFill.color = c;
    }

    // Start is called before the first frame update
    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
        // Initiate fear meter with contribution from the fear of this AI
        FearMeter.INCREASE_METER(currentFear);
        individualSlider.InitSlider(fearThreshold);
        individualSlider.UpdateCharacterFearSlider(currentFear);
        FearCollection.S.SetFearStateText(fearState); // set initial fear state text
        sliderFill.color =  FearCollection.S.GetStateColor(fearState);

        #if UNITY_EDITOR
        myDebugSlot = debugSlotCounter++;
        #endif
    }

    void Update()
    {
        /// Debug toggle
        #if UNITY_EDITOR
        if (myDebugSlot == 0 & Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            showDebug = !showDebug;
        }
        #endif
    }
    
    /// <summary>
    /// Debug GUI for tracking fear add events for each character.
    /// </summary>
    #if UNITY_EDITOR
    void OnGUI()
    {
        if (!showDebug) return;
        
        int x = 200;
        int y = myDebugSlot * 200;
        
        // Create larger font style
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 18;
        
        // Adjust height based on expanded state
        int lineHeight = 24;
        int height = expanded ? 500 : 145;
        int maxChanges = expanded ? 20 : 3;
        
        Rect boxRect = new Rect(x, 10 + y, 450, height); 
        
        // Draw fully opaque solid background if expanded
        if (expanded)
        {
            Texture2D bgTexture = new Texture2D(1, 1);
            bgTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1f));
            bgTexture.Apply();
            GUI.DrawTexture(boxRect, bgTexture);
            Destroy(bgTexture);
            
            GUI.Box(boxRect, "");
        }
        else
        {
            GUI.Box(boxRect, "");
        }
        
        // Check for click on box
        if (Event.current.type == EventType.MouseDown && boxRect.Contains(Event.current.mousePosition))
        {
            expanded = !expanded;
            Event.current.Use();
        }
        
        GUI.Label(new Rect(x + 10, 20 + y, 430, 25), $"{characterEnum} - {currentFear}/{fearThreshold}", labelStyle);
        GUI.Label(new Rect(x + 10, 45 + y, 430, 25), $"State: {fearState} ({turnsUntilNeutralState}t)", labelStyle);
        
        string changesLabel = expanded ? "Last 20 changes (click to collapse):" : "Last 3 changes (click to expand):";
        GUI.Label(new Rect(x + 10, 75 + y, 430, 25), changesLabel, labelStyle);
        
        for (int i = 0; i < Mathf.Min(lastChanges.Count, maxChanges); i++)
        {
            GUI.Label(new Rect(x + 15, 100 + y + (i * lineHeight), 420, 25), lastChanges[i], labelStyle); // Increased width
        }
    }
    #endif

    public void AddFear(int add)
    {
        #if UNITY_EDITOR
        var stackFrame = new System.Diagnostics.StackFrame(1);
        var method = stackFrame.GetMethod();
        string className = method?.DeclaringType?.Name ?? "Unknown";
        string methodName = method?.Name ?? "Unknown";
        string change = $"+{add} from {className}.{methodName}";
        lastChanges.Insert(0, change);
        if (lastChanges.Count > 20) lastChanges.RemoveAt(20); // Keep last 20 instead of 3
        Debug.Log($"[{characterEnum}] {change}");
        #endif

        // Check if would go over threshold
        // Add only what brings currentFear to threshold.
        if (currentFear + add > fearThreshold)
        {
            FearMeter.INCREASE_METER(fearThreshold - currentFear);
            currentFear = fearThreshold;
        }
        else
        {
            currentFear += add;
            FearMeter.INCREASE_METER(add);
        }

        AIFearMeter.UpdateSlider(currentFear);
        individualSlider.UpdateCharacterFearSlider(currentFear);
    }

    public void ReduceFear(int subt)
    {
        // Check if would be negative.
        // Deduct only what brings currentfear to 0.
        if (currentFear - subt < 0)
        {
            FearMeter.DECREASE_METER(currentFear);
            currentFear = 0;
        }
        else
        {
            currentFear -= subt;
            FearMeter.DECREASE_METER(subt);
        }

        AIFearMeter.UpdateSlider(currentFear);
        individualSlider.UpdateCharacterFearSlider(currentFear);
    }


    // used in ai cam to get this ai's fear and set it for the meter.
    public int GetFear()
    {
        return currentFear;
    }
    public int GetFearThreshold()
    {
        return fearThreshold;
    }

    /// <summary>
    /// Return fear state of given AI.
    /// </summary>
    public FearState GetFearState()
    {
        return fearState;
    }

    [Header("UI Elements To Color")]
    public Image sliderFill;
    public Image sliderBackground;
    public TextMeshProUGUI stateText;

    /// <summary>
    /// Called by yarn commands to set character's fear state in dialogues.
    /// </summary>
    public void SetFearState(FearState state)
    {
        FearState prevState = fearState; // previous fear state before setting new, used to compare against to determine lifetime
        fearState = state;
        SetStateLifetime(FearCollection.S.GetStateTurnLife(fearState), prevState);
        FearCollection.S.SetFearStateText(fearState);

        Debug.Log(fearState.ToString());

        Color c = FearCollection.S.GetStateColor(state);

        if (sliderFill != null)
            sliderFill.color = c;

        if (sliderBackground != null)
            sliderBackground.color = c * 0.5f; // darker background

        if (stateText != null)
            stateText.color = c;

        if (!prevStates.Contains(state))
        {
            prevStates.Add(state);
            gameObject.GetComponent<DialogueBank>().QueueSpecialDialog(state.ToString(), DialogueBank.DialogueType.State);
        }
    }

    /// <summary>
    /// Set lifetime for how long a state should last until going to default.
    /// </summary>
    public void SetStateLifetime(int lifetime, FearState prevState)
    {
        // if netural there is no lifetime
        if (fearState == FearState.Neutral)
        {
            turnsUntilNeutralState = 0;
        }
        // if state is same as it is was, add to lifetime of this state
        else if (fearState == prevState)
        {
            turnsUntilNeutralState += lifetime;
        }
        // this is a new state with a lifetime
        else
        {
            turnsUntilNeutralState = lifetime;
        }

        Debug.Log(transform.parent.name + " turns until neutral" + turnsUntilNeutralState);
        
    }

    public void EnemyTurnInitiated()
    {
        return;
    }

    public void PlayerTurnInitiated()
    {
        if(turnsUntilNeutralState > 0)
        {
            turnsUntilNeutralState -= 1;
        }
        else // if completed lifetime on a state, return to netural
        {
            fearState = FearState.Neutral;
            Color c = FearCollection.S.GetStateColor(FearState.Neutral);

            if (sliderFill != null)
                sliderFill.color = c;

            CharacterGameDialogue.SetActivePortrait(characterEnum, fearState); // update portrait to neutral immediately
        }
    }
}
