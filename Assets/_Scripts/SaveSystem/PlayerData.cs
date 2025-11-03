using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Holds all the data gathered during a save operation, all nonserializable data types are converted here as well
/// </summary>
[System.Serializable]
public class PlayerData 
{

    // TurnBasedSystem
    public int totalTurn;
    public int playerTurn;
    public int aiTurn;
    public int eventTurn;
    public int roundsUntil;

    // Maze Generation
    public int mazeSeed;

    // Dispatch System
    public int cultists;

    public float fear;

    // Agent Positions
    public float[] haroldPos;
    public float[] iainPos;
    public float[] mackenziePos;
    public float[] evelynPos;

    // Agent Fear Numbers
    public int haroldFear;
    public int iainFear;
    public int mackenzieFear;
    public int evelynFear;

    // Agent Visited Rooms
    public string[] haroldRooms;
    public string[] iainRooms;
    public string[] mackenzieRooms;
    public string[] evelynRooms;

    // Skill Unlocks
    public bool AOEFearUnlocked;
    public bool BarrierUnlocked;
    public bool TotemUnlocked;
    public bool LureUnlocked;
    public bool RoomSwapUnlocked;

    // Agent Fears Discovered
    public bool[] haroldFears;
    public bool[] iainFears;
    public bool[] mackenzieFears;
    public bool[] evelynFears;

    // Dialogue 
    public int haroldDialogueRemainingCount;
    public int iainDialogueRemainingCount;
    public int mackenzieDialogueRemainingCount;
    public int evelynDialogueRemainingCount;
    public List<int> haroldRemainingDialogues;
    public List<int> iainRemainingDialogues;
    public List<int> mackenzieRemainingDialogues;
    public List<int> evelynRemainingDialogues;





    // Constructor handles conversion of non-serializable data types
    public PlayerData(
        int totalTurnCount, int playerTurnCount, int aiTurnCount, int eventTurnCount, int roundsUntilDefeat,
        int seed,
        int cultistPoints,
        float totalFear,
        int hFear, int iFear, int mFear, int eFear,
        Vector3 hPos, Vector3 iPos, Vector3 mPos, Vector3 ePos,
        List<string> hRooms, List<string> iRooms, List<string> mRooms, List<string> eRooms,
        bool aoeUnlocked, bool bUnlocked, bool tUnlocked, bool lUnlocked, bool rUnlocked,
        bool[] hFears, bool[] iFears, bool[] mFears, bool[] eFears,
        int hDialogueCount, int iDialogueCount, int mDialogueCount, int eDialogueCount,
        List<int> hRemainingDialogues, List<int> iRemainingDialogues, List<int> mRemainingDialogues, List<int> eRemainingDialogues
        )
    {

        totalTurn = totalTurnCount;
        playerTurn = playerTurnCount;
        aiTurn = aiTurnCount;
        eventTurn = eventTurnCount;

        mazeSeed = seed;


        cultists = cultistPoints;
        fear = totalFear;

        iainFear = iFear;
        haroldFear = hFear;
        evelynFear = eFear;
        mackenzieFear = mFear;

        haroldPos = new float[3];
        haroldPos[0] = hPos.x;
        haroldPos[1] = hPos.y;
        haroldPos[2] = hPos.z;

        iainPos = new float[3];
        iainPos[0] = iPos.x;
        iainPos[1] = iPos.y;
        iainPos[2] = iPos.z;

        mackenziePos = new float[3];
        mackenziePos[0] = mPos.x;
        mackenziePos[1] = mPos.y;
        mackenziePos[2] = mPos.z;

        evelynPos = new float[3];
        evelynPos[0] = ePos.x;
        evelynPos[1] = ePos.y;
        evelynPos[2] = ePos.z;

        haroldRooms = hRooms.ToArray();
        iainRooms = iRooms.ToArray();
        mackenzieRooms = mRooms.ToArray();
        evelynRooms = eRooms.ToArray();

        AOEFearUnlocked = aoeUnlocked;
        BarrierUnlocked = bUnlocked;
        TotemUnlocked = tUnlocked;
        LureUnlocked = lUnlocked;
        RoomSwapUnlocked = rUnlocked;

        haroldFears = hFears;
        iainFears = iFears;
        mackenzieFears = mFears;
        evelynFears = eFears;

        haroldDialogueRemainingCount = hDialogueCount;
        iainDialogueRemainingCount = iDialogueCount;
        mackenzieDialogueRemainingCount = mDialogueCount;
        evelynDialogueRemainingCount = eDialogueCount;

        haroldRemainingDialogues = hRemainingDialogues;
        iainRemainingDialogues = iRemainingDialogues;
        mackenzieRemainingDialogues = mRemainingDialogues;
        evelynRemainingDialogues = eRemainingDialogues;

    }
}
