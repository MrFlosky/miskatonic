using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;

public class CultistPoints : MonoBehaviour
{
    static private CultistPoints S;

    [SerializeField] private TextMeshProUGUI txt;

    // Number of available points player has
    [SerializeField] public int cultistPoints { get; private set; }

    void Start()
    {
        //for testing
        cultistPoints = 33;

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
    }

    /// <summary>
    /// Updates text to reflect new cultist point balance
    /// </summary>
    private void UpdateText()
    {
        txt.text = "Cultist Points: " + cultistPoints;
    }
}
