using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookTurner : MonoBehaviour
{
    OpenBook OB;
    [SerializeField]
    bool nextPage;
    void Start()
    {
        OB = GetComponentInParent<OpenBook>();
    }


    public void TurnThePage()
    {

        if (nextPage)
        {
            OB.NextPage();
        }

        else OB.PreviousPage();

    }


}
