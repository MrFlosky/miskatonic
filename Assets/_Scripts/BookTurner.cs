using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookTurner : MonoBehaviour
{
    OpenBook OB;
    bool turnRight;
    void Start()
    {
        OB = GetComponentInParent<OpenBook>();
    }


    public void TurnThePage()
    {

        if (turnRight)
        {
            OB.turnRight();
        }

        else OB.turnLeft();

    }


}
