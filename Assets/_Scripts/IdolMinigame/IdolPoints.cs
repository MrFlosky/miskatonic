using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdolPoints : MonoBehaviour
{
    public int remainingPoints;
    // Start is called before the first frame update
    void Start()
    {
        remainingPoints = 3;
    }

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
