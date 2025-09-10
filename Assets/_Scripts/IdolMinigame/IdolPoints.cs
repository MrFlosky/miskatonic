using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IdolPoints : MonoBehaviour
{
    public static IdolPoints S;
    public int remainingPoints;
    public TextMeshProUGUI txt;


    void Start()
    {
        S = this;
    }

    // TODO: i want to handle 0 checking here
    public static void DEDUCT_POINT()
    {
        if (S.remainingPoints - 1 >= 0)
        {
            S.remainingPoints -= 1;
            S.txt.text = "Idols: " + S.remainingPoints;
        }
        else
        {
            Debug.Log("NO MORE IDOL POINTS AVAILABLE");
            return;
        }
    }


    public static void ADD_POINT()
    {
        S.remainingPoints += 1;
        S.txt.text = "Idols: " + S.remainingPoints;
        if (S.txt.IsActive() == false)
        {
            S.txt.gameObject.SetActive(true);
        }
    }

    public static void HIDE_PTS()
    {
        S.txt.gameObject.SetActive(false);
    }

    public static void DISPLAY_PTS()
    {
        S.txt.text = "Idols: " + S.remainingPoints;
        if (S.txt.IsActive() == false)
        {
            S.txt.gameObject.SetActive(true);
        }
        
    }
}
