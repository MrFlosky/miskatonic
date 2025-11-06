using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Books: MonoBehaviour
{
    /// <summary>
    /// This script handles logic for changing books
    /// 
    /// </summary>

    public GameObject openBook;
    public Bookshelf bookshelf;


 
    private void Start()
    {
        bookshelf = Bookshelf.S;


        if (openBook == null)
        {
            Debug.LogError(gameObject.name + " has no openBook assigned. Assign an openBook in the inspector");

        }

        bookshelf.Booklist.Add(this);

      
    
    }


    // changes which book is open for the player and sets the first page as the current active page
    public void ChangeBook()
    {
        OpenBook OB =openBook.GetComponent<OpenBook>();
        if (OB.gameObject.activeSelf) return;
        foreach(var BK in bookshelf.Booklist)
        {
            
            BK.openBook.SetActive(false);
           
        }

        openBook.SetActive(true);

        foreach(GameObject page in OB.storyPanels)
        {
            page.SetActive(false);
        }
        OB.storyPanels[0].SetActive(true);
        OB.activePage = OB.storyPanels[0];
        
        
       
        

   
        

    }


}
