using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FearMeter : MonoBehaviour
{
    public static FearMeter S { get; private set; }
    int totalFear = 0;
    public bool RnD;
    public Slider slid;
    float timeElapsed = 0;
    public float sliderSpeed = 0.2f;
    public ParticleSystem ps;

    //NOTE: This numnber should be how many percent you want to increase by i.e. if you want 1% just put 1.
    //The number will be divided by 100 to make it a percentage later
    //currently not functional, fear must be float
    //[SerializeField] float RnDGain;

    //temporary until fear is reworked to float
    //gives one extra fear for every 10 cultists by default
    [SerializeField] int RnDIntGain;
    [SerializeField] int RnDDivisor=10;
    void Awake()
    {
        slid = GetComponent<Slider>();
        S = this;
        
        if (LoadManager.S.loadData) totalFear = LoadManager.S.playerDataToLoad.fear;

        slid.maxValue = GameManager.Instance.GetWinFear();
        enabled = false;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        slid.value = Mathf.Lerp(slid.value, totalFear, timeElapsed * sliderSpeed);

        if(slid.value == totalFear)
        {
            enabled = false;
        }
    }

    static public void INCREASE_METER(int increase)
    {
        
        AudioManager.PlaySFX(SFXType.FearIncrease);
        S.timeElapsed = 0; // setting to 0 here makes so slider animates smoothly with multiple increase calls

        if (!S.RnD) S.totalFear += increase;

        else
        {
            print("Yup");
            S.RnDIntGain = Mathf.RoundToInt(CultistPoints.S.cultistPoints / S.RnDDivisor);
        }

        //this will apply the percentage gain from the R&D upgrade, will only be functional if Fear is changed to float
        // else
        // {
        //     S.totalFear += increase*(1+S.RnDGain) 
        // }


        GameManager.Instance.CHECK_FEAR_WIN(S.totalFear);
        S.enabled = true;
    }

    static public void DECREASE_METER(int decrease)
    {
        AudioManager.PlaySFX(SFXType.FearDecrease);
        S.timeElapsed = 0;
        S.totalFear -= decrease;
        S.enabled = true;
    }

    static public int GET_METER_TOTAL()
    {
        return S.totalFear;
    }




}
