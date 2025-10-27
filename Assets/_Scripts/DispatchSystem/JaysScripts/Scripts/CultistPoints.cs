using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CultistPoints : MonoBehaviour
{
    static private CultistPoints S;

    /// <summary>
    /// Text displayed in DealerCanvas
    /// </summary>
    [SerializeField] private TextMeshProUGUI txt;

    /// <summary>
    /// Mini displayerd on TurnCanvas
    /// </summary>
    [SerializeField] private TextMeshProUGUI babyTxt;

    // Number of available points player has
    [SerializeField] public int cultistPoints;


    void Start()
    {
        S = this;
        UpdateText();
    }

    /// <summary>
    /// Called by a mission. Returns true if player has enough cultist points
    /// to execute this mission, false otherwise.
    /// </summary>
    public bool AttemptMissionPurchase(int cost)
    {
        if (cultistPoints >= cost)
        {
            cultistPoints -= cost;
            UpdateText();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Called by DispatchSystem. Returns true if player has enough cps to 
    /// buy another reroll (only called if this ability was unlocked).
    /// </summary>
    static public bool AttemptRerollPurchase(int cost)
    {
        if (S.cultistPoints >= cost)
        {
            S.cultistPoints -= cost;
            S.UpdateText();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Grant player given number of cultist points, called by cultist reward
    /// </summary>
    public void AddPoints(int points)
    {
        cultistPoints += points;
        UpdateText();
    }

    /// <summary>
    /// For classes that dont have direct ref to this class,
    /// specifically Fear on each character
    /// </summary>
    static public void AddPoint()
    {
        S.cultistPoints += 1;
        S.UpdateText();
    }

    static public void AddPointsStatic(int points)
    {
        S.cultistPoints += points;
        S.UpdateText();
    }

    /// <summary>
    /// Get number of cultist points available.
    /// </summary>
    static public int GetPoints()
    {
        return S.cultistPoints;
    }

    /// <summary>
    /// Deduct given number of usable cultist points.
    /// </summary>
    static public void DeductPonts(int points)
    {
        S.cultistPoints -= points;
        S.UpdateText();
    }

    /// <summary>
    /// Updates text to reflect new cultist point balance
    /// </summary>
    private void UpdateText()
    {
        txt.text = "Cultist Points: " + cultistPoints;
        babyTxt.text = "Cultists: " + cultistPoints;
    }
}
