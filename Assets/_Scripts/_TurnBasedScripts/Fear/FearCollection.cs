using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Possible fear states character may be in, all triggered via dialogues.
/// Affects characters per tile movement.
/// </summary>
public enum FearState
{
    Neutral,
    Determined,
    Scared,
    Terrified,
    Tweaking
};


/// <summary>
/// Describes the behavior for a given state, namely how many tiles AI may move
/// and how long corresponding state should last before returning to neutral.
/// </summary>
[System.Serializable]
public class FearStateOb
{
    public FearState state;
    public int tilesPerTurn;
    public int turnsLast;

    public Color stateColor;
}

/// <summary>
/// This class is responsible for managing fear states, including:
/// - Holding global game data for each state and its respective behavior
/// - Interface for other classes to get a states behavior
/// </summary>
public class FearCollection : MonoBehaviour
{
    /// <summary>
    /// Container for editor adjustments, converted to dict for code use.
    /// </summary>
    [Header("-1 TILE BC AGENTS CURRENT SLOT COUNTS AS 1")]
    [SerializeField]
    private FearStateOb[] fearStateData;

    /// <summary>
    /// Dict that holds fear state with corresponding state behaviors as value.
    /// </summary>
    private Dictionary<FearState, StateBehavior> fearStateDict = new Dictionary<FearState, StateBehavior>();

    public static FearCollection S;

    /// <summary>
    /// Text that displays given characters fear state, under their slider in dialogue. One shared for all characters.
    /// Parented to AI slider since that. 
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI fearStateText;

    void Awake()
    {
        S = this;
        ConvertToDict(); // convert inspector list to dict
    }

    public int GetStateMovement(FearState state)
    {
        return fearStateDict[state].tilesPerTurn;
    }

    public int GetStateTurnLife(FearState state)
    {
        return fearStateDict[state].turnsLast;
    }

    /// <summary>
    /// Used for dictionary conversion as the value.
    /// </summary>
    private struct StateBehavior // struct for data that acts like a value
    {
        public int tilesPerTurn;
        public int turnsLast;
        public Color color;

        public StateBehavior(int tiles, int turns, Color color)
        {
            tilesPerTurn = tiles;
            turnsLast = turns;
            this.color = color;
        }
    }

    /// <summary>
    /// Convert inspector array into dictionary. 
    /// </summary>
    private void ConvertToDict()
    {
        foreach (FearStateOb fs in fearStateData)
        {
            StateBehavior stateBehavior = new StateBehavior(fs.tilesPerTurn, fs.turnsLast, fs.stateColor);
            fearStateDict.Add(fs.state, stateBehavior);
        }
    }
    
    /// <summary>
    /// Set the fear state text
    /// </summary>
    public void SetFearStateText(FearState state)
    {
        CurrentFearUIColor = fearStateDict[state].color;
        fearStateText.text = "State: " + state.ToString();
    }

    public Color CurrentFearUIColor { get; private set; }

    /// <summary>
    /// Get state color
    /// </summary>
    public Color GetStateColor(FearState state)
    {
        CurrentFearUIColor = fearStateDict[state].color;
        return fearStateDict[state].color;
    }
}
