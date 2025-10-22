using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all the data gathered during a save operation, all nonserializable data types are converted here as well
/// </summary>
[System.Serializable]
public class PlayerData 
{
    public int turn;
    public int cultists;
    public float fear;
    public float[] haroldPos;
    public float[] iainPos;
    public float[] mackenziePos;
    public float[] evelynPos;

    public PlayerData(int currentTurn, int cultistPoints, float currentFear, Vector3 harold, Vector3 iain, Vector3 mack, Vector3 eve)
    {
        turn = currentTurn;
        cultists = cultistPoints;
        fear = currentFear;

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



    }
}
