using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Using ConvoManager to show/hide slider.
/// Using AICamera to set sliders initial startup value upon each AI click.
/// Using Fear to update/animate increase/decrease.
/// </summary>
public class AIFearMeter : MonoBehaviour
{
    private static AIFearMeter S;
    private Slider aiSlider;
    private int fear;
    void Awake()
    {
        S = this;
        aiSlider = GetComponent<Slider>();
        S.enabled = false;
    }

    private float timeElapsed = 0f;
    public float sliderSpeed = 0.2f;

    void Update()
    {
        timeElapsed += Time.deltaTime;
        aiSlider.value = Mathf.Lerp(aiSlider.value, fear, timeElapsed * sliderSpeed);

        if(aiSlider.value == fear)
        {
            S.enabled = false;
        }
    }

    /// <summary>
    /// Upon AI being selected in AICAmera, this function is called
    /// which sets the slider's max and current value.
    /// </summary>
    /// <param name="currentFear"></param>
    /// <param name="maxFear"></param>
    static public void StartUpSlider(int currentFear, int maxFear)
    {
        //S.enabled = false; // not sure if needed here. maybe just on awake have enabled false.
        Heartbeats.StartHeartbeat(currentFear, maxFear);
        S.fear = currentFear;
        S.aiSlider.maxValue = maxFear; 
        S.aiSlider.value = currentFear;
    }

    /// <summary>
    /// This function is called in Fear whenever fear is added/reduced on
    /// an AI.
    /// </summary>
    /// <param name="newFear"></param>
    static public void UpdateSlider(int newFear)
    {
        Heartbeats.UpdateHeartBeat(newFear);
        S.timeElapsed = 0;
        S.fear = newFear;
        S.enabled = true;
    }



}
