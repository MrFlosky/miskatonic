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

    // Agent Fears
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






    // Constructor handles conversion of non-serializable data types
    public PlayerData(
        int totalTurnCount, int playerTurnCount, int aiTurnCount, int eventTurnCount, int roundsUntilDefeat,
        int seed,
        int cultistPoints,
        float totalFear,
        int hFear, int iFear, int mFear, int eFear,
        Vector3 harold, Vector3 iain, Vector3 mack, Vector3 eve,
        List<string> hRooms, List<string> iRooms, List<string> mRooms, List<string> eRooms,
        bool aoeUnlocked, bool bUnlocked, bool tUnlocked, bool lUnlocked, bool rUnlocked 
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
        haroldPos[0] = harold.x;
        haroldPos[1] = harold.y;
        haroldPos[2] = harold.z;

        iainPos = new float[3];
        iainPos[0] = iain.x;
        iainPos[1] = iain.y;
        iainPos[2] = iain.z;

        mackenziePos = new float[3];
        mackenziePos[0] = mack.x;
        mackenziePos[1] = mack.y;
        mackenziePos[2] = mack.z;

        evelynPos = new float[3];
        evelynPos[0] = eve.x;
        evelynPos[1] = eve.y;
        evelynPos[2] = eve.z;

        haroldRooms = hRooms.ToArray();
        iainRooms = iRooms.ToArray();
        mackenzieRooms = mRooms.ToArray();
        evelynRooms = eRooms.ToArray();

        AOEFearUnlocked = aoeUnlocked;
        BarrierUnlocked = bUnlocked;
        TotemUnlocked = tUnlocked;
        LureUnlocked = lUnlocked;
        RoomSwapUnlocked = rUnlocked;



    }
}
