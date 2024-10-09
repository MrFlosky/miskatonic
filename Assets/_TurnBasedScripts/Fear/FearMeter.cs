using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FearMeter : MonoBehaviour
{
    private static FearMeter S;
    int totalFear = 0;
    int maxFear = 100;

    Slider slid;
    float timeElapsed = 0;
    public float sliderSpeed = 0.2f;

    void Awake()
    {
        slid = GetComponent<Slider>();
        S = this;
        this.enabled = false;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        slid.value = Mathf.Lerp(slid.value, totalFear, timeElapsed * sliderSpeed);

        if(slid.value >= totalFear)
        {
            this.enabled = false;
        }
    }

    static public void INCREASE_METER(int increase)
    {

        S.timeElapsed = 0; // setting to 0 here makes so slider animates smoothly with multiple increase calls
        S.totalFear += increase;
        S.enabled = true;
    }

    static public void DECREASE_METER(int decrease)
    {
        S.timeElapsed = 0;
        S.totalFear -= decrease;
        S.enabled = true;
    }

    static public int GET_METER_TOTAL()
    {
        return S.totalFear;
    }


}
