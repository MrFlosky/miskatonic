using System.Collections;
using System.Collections.Generic;
using CharacterData;
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

    public Character charactersBook;
 
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
    public void ChangeBook(bool FromUnlock, bool UnlockingClue)
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

        foreach(GameObject page in OB.narrativeClues)
        {
            page.SetActive(false);
        }
        
        if(!FromUnlock)
        {
            Debug.LogError("NOT UNLOCK");
            OB.storyPanels[0].SetActive(true);
            OB.activePage = OB.storyPanels[0];
            OB.firstPage= OB.storyPanels[0];
            OB.lastPage = OB.storyPanels[OB.unlockedPages-1];
            OB.storyboardOpen= true;
        }

        else
        {
            

            if (UnlockingClue)
            {
                
               
                OB.storyboardOpen=false;
            }

            else
            {
                OB.storyboardOpen=true;
                Debug.LogError("GDSAG");
            }
        }
       
        

   
        

    }


}
