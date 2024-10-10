using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Skill Settings")]
    public int sk1CD;
    public int sk2CD;

    [Header("Round Settings")]
    public int maxRound;

    private bool sk1Ready;
    private bool sk2Ready;

    private int sk1Count;
    private int sk2Count;

    private Button sk1Button;  
    private Button sk2Button;

    private int roundCount;

    private Text roundText;

    void Start()
    {
        sk1Button = GameObject.Find("Skill1").GetComponent<Button>();
        sk2Button = GameObject.Find("Skill2").GetComponent<Button>();
        roundText = GameObject.Find("RoundUI").GetComponent<Text>();

        // By default, Button.onClick.AddListener() only supports calling functions without parameters
        // This can be done through lambda expressions or anonymous functions
        sk1Button.onClick.AddListener(() => OnButtonClick(ref sk1Ready));
        sk2Button.onClick.AddListener(() => OnButtonClick(ref sk2Ready));

        sk1Ready = true;
        sk2Ready = true;

        sk1Count = sk1CD;
        sk2Count = sk2CD;

        roundCount = maxRound;
    }

    private void Update()
    {
        UpdateRound();

        //simulate round end
        if (Input.GetKeyDown(KeyCode.N))
        {
            RoundEnd();
        }

        if(!sk1Ready)
        {
            sk1Button.interactable = false;
            CDCheck(sk1Button, ref sk1Count, sk1CD, ref sk1Ready);
        }
        if (!sk2Ready)
        {
            sk2Button.interactable = false;
            CDCheck(sk2Button, ref sk2Count, sk2CD, ref sk2Ready); 
        }
    }

    /// <summary>
    /// check how many round left for CD
    /// </summary>
    /// <param name="button">which button will respond</param>
    /// <param name="cd">how many round need to be wait</param>
    /// <param name="cdMax">the original CD of the skill</param>
    void CDCheck(Button button, ref int cd, int cdMax, ref bool isReady)
    {
        button.GetComponentInChildren<Text>().text = cd.ToString();
        if (cd == 0)
        {
            isReady = true;
            button.interactable = true;
            cd = cdMax;
            button.GetComponentInChildren<Text>().text = null;
        }
    }

    /// <summary>
    /// when button is clicked run this function
    /// </summary>
    void OnButtonClick(ref bool isReady)
    {
        isReady = false;
    }

    /// <summary>
    /// JUST FOR TEST, NEED TO REPLACE BY THE REAL ROUND MANAGER SCRIPT(when round ends, run this function)
    /// </summary>
    void RoundEnd()
    {
        if (sk1Count > 0 && !sk1Ready)
            { sk1Count--; }
        if (sk2Count > 0 && !sk2Ready)
            { sk2Count--; }

        roundCount--;
    }

    /// <summary>
    /// Update the number of current round
    /// </summary>
    void UpdateRound()
    {
        roundText.text = roundCount.ToString();
    }

}
