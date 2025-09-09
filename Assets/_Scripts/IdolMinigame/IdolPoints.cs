using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdolPoints : MonoBehaviour
{
    public int remainingPoints;


    // TODO: i want to handle 0 checking here
    public void DEDUCT_POINT()
    {
        if (remainingPoints - 1 >= 0)
        {
            remainingPoints -= 1;
        }
        else
        {
            Debug.Log("NO MORE IDOL POINTS AVAILABLE");
            return;
        }
    }
}
