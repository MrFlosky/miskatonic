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
            if(OB.unlockedPages==1 && OB.unlockedClues==0) return;
            OB.NextPage();
        }

        else 
        {
            if(OB.storyPanels[0].activeSelf) return;
            OB.PreviousPage();
            
        }

    }


}
