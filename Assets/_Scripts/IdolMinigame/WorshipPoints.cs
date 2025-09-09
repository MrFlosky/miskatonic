using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorshipPoints : MonoBehaviour
{
    public static WorshipPoints S;
    public int totalPoints = 0;

    void Start()
    {
        S = this;
    }

    public static void DEDUCT_POINT()
    {
        if (S.totalPoints - 1 >= 0)
        {
            S.totalPoints -= 1;
        }
        else
        {
            Debug.Log("NO MORE IDOL POINTS AVAILABLE");
            return;
        }
    }

    public static void ADD_POINTS(int pts)
    {
        S.totalPoints += pts;
    }

    public static void ADD_POINT()
    {
        S.totalPoints += 1;
    }
}
