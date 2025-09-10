using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorshipPoints : MonoBehaviour
{
    public TextMeshProUGUI txt;
    public static WorshipPoints S;
    public int totalPoints = 0;

    void Start()
    {
        S = this;
    }


    public static void DEDUCT_POINTS(int pts)
    {
        if (S.totalPoints - pts >= 0)
        {
            S.totalPoints -= pts;
        }
        else
        {
            Debug.Log("NOT ENOUGH WORSHIP POINTS");
            return;
        }
    }

    public static void ADD_POINTS(int pts)
    {
        S.totalPoints += pts;
        DISPLAY_PTS();
    }

    public static void SET_POINTS(int pts)
    {
        S.totalPoints = pts;
        DISPLAY_PTS();
    } 

    public static void DISPLAY_PTS()
    {
        S.txt.text = "Worship Points: " + S.totalPoints;
        if (S.txt.IsActive() == false)
        {
            S.txt.gameObject.SetActive(true);
        }
    }

    public static void HIDE_PTS()
    {
        S.txt.gameObject.SetActive(false);
    }
}
