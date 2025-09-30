using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fear : MonoBehaviour
{

    public int currentFear = 10;
    /// <summary>
    /// 2 important notes:
    /// - Max value on AI slider automatically updated when this is changed.
    /// - HOWEVER, the fear WIN meter max value (win value) needs to be manually adjusted to account for totaled AI thresholds.
    /// </summary>
    public int fearThreshold = 25;
    string weakness;
    // Start is called before the first frame update
    void Start()
    {
     
        // Initiate fear meter with contribution from the fear of this AI
        FearMeter.INCREASE_METER(currentFear);
    }

    public void AddFear(int add)
    {
        // Check if would go over threshold
        // Add only what brings currentFear to threshold.
        if (currentFear + add > fearThreshold)
        {
            FearMeter.INCREASE_METER(fearThreshold - currentFear);
            currentFear = fearThreshold;
            AIFearMeter.UpdateSlider(currentFear);
        }
        else
        {
            currentFear += add;
            FearMeter.INCREASE_METER(add);
            AIFearMeter.UpdateSlider(currentFear);
        }
        
    }

    public void ReduceFear(int subt)
    {
        // Check if would be negative.
        // Deduct only what brings currentfear to 0.
        if(currentFear - subt < 0)
        {
            FearMeter.DECREASE_METER(currentFear); 
            currentFear = 0;
            AIFearMeter.UpdateSlider(currentFear);
        } else
        {
        currentFear -= subt;
        FearMeter.DECREASE_METER(subt);
        AIFearMeter.UpdateSlider(currentFear);
        }
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
}
