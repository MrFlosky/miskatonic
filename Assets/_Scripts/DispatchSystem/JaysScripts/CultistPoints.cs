using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;

public class CultistPoints : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txt;

    // Number of available points player has
    [SerializeField] private int cultistPoints = 0;

    void Start()
    {
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
    /// Updates text to reflect new cultist point balance
    /// </summary>
    private void UpdateText()
    {
        txt.text = "Cultist Points: " + cultistPoints;
    }
}
