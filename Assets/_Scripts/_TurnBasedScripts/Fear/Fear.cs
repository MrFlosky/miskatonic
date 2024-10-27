using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fear : MonoBehaviour
{

    public int currentFear = 10;
    [Header("If changed AI Slider max val will need to be adjusted to account for this atm.")]
    public int fearThreshold = 25;
    private Slider aiFearSlid;
    string weakness;
    // Start is called before the first frame update
    void Start()
    {
     
        // Initiate fear meter with contribution from the fear of this AI
        FearMeter.INCREASE_METER(currentFear);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddFear(int add)
    {
        // Check if would go over threshold
        // Add only what brings currentFear to threshold.
        if(currentFear + add > fearThreshold)
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
