using System;
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
    [SerializeField] private int numberOfTOTALRoundsUntilDefeat = 30;
    [SerializeField] private int totalFearToWin = 400;
    [SerializeField] private int aoeFearAddedPerTurn = 10;
    [SerializeField] private int dialogueFearAdd = 10;
    [SerializeField] private int dialogueFearReduce = 10;

    
    [Header("0:Evelyn _ 1:Harold _ 2:Iain _ 3:Mackenzie")]
    [SerializeField] private int[] aiStartingFear = new int[4];
    [SerializeField] private int[] aiFearThresholds = new int[4];

    [Header("DO NOT TOUCH!")]
    [SerializeField] private PlayerTurn pt;
    [SerializeField] private AOEEffect aoe;
    [SerializeField] private SkillSystem ss;
    [SerializeField] private GameManager gm;
    [SerializeField] private FearMeter fm;
    [SerializeField] private Fear[] fearArray = new Fear[4];

    /// <summary>
    /// Resets all values to their original (last build before mi497)
    /// </summary>
    public void ResetToDefaults()
    {
        playerStartingPoints = 3;
        aoeFearAddedPerTurn = 10;
        numberOfTOTALRoundsUntilDefeat = 30;
        totalFearToWin = 400;

        // idk if these are the originals, i forget
        dialogueFearAdd = 10;
        dialogueFearReduce = 10;

        foreach (Fear aiFear in fearArray)
        {
            aiFear.fearThreshold = 100;
            aiFear.currentFear = 10;
        }
    }

    /// <summary>
    /// Sets all balance variables on frame script is enabled, start of game
    /// </summary>
    void Start()
    {
        pt.startingPoints = playerStartingPoints;
        ss.availablePts = playerStartingPoints;
        aoe.perTurnFear = aoeFearAddedPerTurn;
        gm.roundsUntilDefeat = numberOfTOTALRoundsUntilDefeat;
        gm.winFear = totalFearToWin;
        DialogueBank.fearAdd = dialogueFearAdd;
        DialogueBank.fearReduce = dialogueFearReduce;
        // have to do this here bc maxval set once in awake for fmet
        // same does NOT have to be done for ai fmet bc its updated upon clicking ai
        fm.slid.maxValue = totalFearToWin;

        for (int i = 0; i < fearArray.Length; i++)
        {
            fearArray[i].fearThreshold = aiFearThresholds[i];
            fearArray[i].currentFear = aiStartingFear[i];
        }
    }



}
