using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Books: MonoBehaviour
{

    public GameObject paper;
    
    
    public Bookshelf bookshelf;
 


    private void Start()
    {
        bookshelf = Bookshelf.S;


        if (paper == null)
        {
            Debug.LogError(gameObject.name + " has no paper assigned.");

        }

        bookshelf.Booklist.Add(this);

      
    
    }


    // activats the enabled page and deactivates all other book pages
    public void ChangeBook()
    {
        

        foreach(var BK in bookshelf.Booklist)
        {
            
            BK.paper.SetActive(false);
           
        }
       
        paper.SetActive(true);
       
        

   
        

    }


}
