using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The ONLY purpose of this class is to create an interface where balance
/// values can all be reached within one place.
/// </summary>
public class ModifyValsInterface : MonoBehaviour
{
    [Header("Modifiable")]
    [Tooltip("Number of points player starts off with each turn.")]
    [SerializeField] private int playerStartingPoints = 3;
    [SerializeField] private int aoeFearAddedPerTurn = 10;

    [Header("Dont touch!")]
    [SerializeField] private PlayerTurn pt;
    [SerializeField] private AOEEffect aoe;

    /// <summary>
    /// Called by BalanceValsEditor to apply new values
    /// </summary>
    public void ApplyValues()
    {
        pt.startingPoints = playerStartingPoints;
        aoe.perTurnFear = aoeFearAddedPerTurn;
        
    }

    public void ResetToDefaults()
    {

    }


}
