using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;


public class Fear : MonoBehaviour, ITurnObserver
{

    public int currentFear = 10;
    /// <summary>
    /// 2 important notes:
    /// - Max value on AI slider automatically updated when this is changed.
    /// - HOWEVER, the fear WIN meter max value (win value) needs to be manually adjusted to account for totaled AI thresholds.
    /// </summary>
    public int fearThreshold = 25;
    string weakness;

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

    void Awake()
    {
        // awake since apart of same game obj
        individualSlider = GetComponent<IndividualFearMeter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
        // Initiate fear meter with contribution from the fear of this AI
        FearMeter.INCREASE_METER(currentFear);
        individualSlider.InitSlider(fearThreshold);
        individualSlider.UpdateCharacterFearSlider(currentFear);
        FearCollection.S.SetFearStateText(fearState.ToString()); // set initial fear state text
    }


    public void AddFear(int add)
    {
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

    /// <summary>
    /// Called by yarn commands to set character's fear state in dialogues.
    /// </summary>
    public void SetFearState(FearState state)
    {
        FearState prevState = fearState; // previous fear state before setting new, used to compare against to determine lifetime
        fearState = state;
        SetStateLifetime(FearCollection.S.GetStateTurnLife(fearState), prevState);
        FearCollection.S.SetFearStateText(fearState.ToString());
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
        }
    }
}
