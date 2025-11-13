using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// This class is responsible for updating the individual fear meter slider for an AI, 
/// based on values in their respective Fear script.
/// </summary>
public class IndividualFearMeter : MonoBehaviour
{
    /// <summary>
    /// Character's individual slider, as displayed with the portrait.
    /// </summary>
    [SerializeField] private Slider individualSlider;

    /// <summary>
    /// Updated character fear for every add/reduce
    /// </summary>
    private int newFearValue = 0;

    /// <summary>
    /// Variables for adjusting slider value over time.
    /// </summary>
    private float timeElapsed = 0f;
    [SerializeField] private float sliderSpeed = 0.2f;

    /// <summary>
    /// Update slider value over time upon fear value adjustment to this character.
    /// </summary>
    void Update()
    {
        timeElapsed += Time.deltaTime;
        individualSlider.value = Mathf.Lerp(individualSlider.value, newFearValue, timeElapsed * sliderSpeed);

        // disable script once reached the fear value for given ai
        if (individualSlider.value == newFearValue)
        {
            enabled = false;
        }
    }

    /// <summary>
    /// Update character's fear slider over time by enabling this monobehavior,
    /// disables when update complete.
    /// </summary>
    /// <param name="fear">
    /// Character's new TOTAL fear value
    /// </param>
    public void UpdateCharacterFearSlider(int fear)
    {
        newFearValue = fear;
        enabled = true;
    }

    /// <summary>
    /// Init this slider with its max value
    /// </summary>
    public void InitSlider(int fearThreshold)
    {
        individualSlider.maxValue = fearThreshold;
    }
    

}
