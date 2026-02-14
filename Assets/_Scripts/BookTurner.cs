using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookTurner : MonoBehaviour
{
    OpenBook OB;

    public bool nextPage;
    public SpriteRenderer pageTurner;

    void Awake()
    {
         pageTurner= GetComponentInChildren<SpriteRenderer>();
    }

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
