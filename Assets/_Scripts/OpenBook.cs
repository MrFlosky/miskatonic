using System;
using System.Collections;
using System.Collections.Generic;
using CharacterData;
using Unity.VisualScripting;
using UnityEngine;

public class OpenBook : MonoBehaviour
{
    /// <summary>
    /// Handles the logic for changing pages
    /// 
    /// The open book object must be active when the scene starts to ensure that the list of pages and page turners is properly populated.
    /// It will deactivate on start.
    /// 
    /// By default only one page will be active and viewable, more can be unlocked via the dispatch system
    /// 
    /// Code has gotten messy, I plan to clean it up and make it more clear if there is time, right now I have focused on functionality first
    /// </summary>

    public List<GameObject> pageTurners = new List<GameObject>();
    public List<GameObject> storyPanels = new List<GameObject>();
    public List<GameObject> narrativeClues = new List<GameObject>();

    public GameObject firstPage, lastPage;
    public GameObject activePage;
    public int unlockedPages;
    public int unlockedClues;
    
    public bool storyboardOpen=true; 
    
    public Character charactersBook;
    void Start()
    {   
        unlockedPages = 1;
        unlockedClues=0;
        pageTurners.Clear();

        foreach (Transform t in transform.GetComponentInChildren<Transform>())
        {
            if (t.GetComponent<BookTurner>())
            {
                pageTurners.Add(t.gameObject);
            }

            else if (t.CompareTag("Clues"))
            {
                foreach(Transform v in t.GetComponentsInChildren<Transform>())
                {
                    if(v!=t)
                    {
                        narrativeClues.Add(v.gameObject);
                        v.gameObject.SetActive(false);
                    }
                }
            }

            else if(t.CompareTag("Storyboard"))
            {
                foreach(Transform v in t.GetComponentsInChildren<Transform>())
                {
                    if(v!=t) storyPanels.Add(v.gameObject);
                }
            }
        }
        firstPage = storyPanels[0];
        lastPage = storyPanels[0];
        gameObject.SetActive(false);
        print(storyPanels.Count);
        UnlockNarrative.BookObjects.Add(this);
    }

    public void NextPage()
    {
        int pageNumber;
        //the last unlocked storyboard pannel is open. Advances to narrative clues if they are unlocked, returns if not
        if (lastPage.activeSelf&&storyboardOpen)
        {
            
            if(unlockedClues==0) return;
            
            storyboardOpen=false;
            activePage.SetActive(false);
            narrativeClues[0].SetActive(true);
            firstPage = narrativeClues[0];
            lastPage = narrativeClues[unlockedClues-1];
            activePage = narrativeClues[0];

            return;
        }
        
        //Storyboards are open, and there are more storyboards to view;
        else if(!lastPage.activeSelf && storyboardOpen)
        {
            pageNumber = storyPanels.IndexOf(activePage);
            activePage.SetActive(false);
            storyPanels[pageNumber + 1].SetActive(true);
            activePage = storyPanels[pageNumber + 1];
        }
        
        //narrative clues are open, and there are more clues to view
        else if(!storyboardOpen&& !lastPage.activeSelf)
        {
            pageNumber = narrativeClues.IndexOf(activePage);
            activePage.SetActive(false);
            narrativeClues[pageNumber+1].SetActive(true);
            activePage= narrativeClues[pageNumber+1];    

        }
        

    }
    
    public void PreviousPage()
    {
        int pageNumber;
        
        //storyboard pannel is open, opens the previous page
        if (storyboardOpen && !firstPage.activeSelf)
        {   
            pageNumber = storyPanels.IndexOf(activePage);
            print(pageNumber);
            activePage.SetActive(false);
            storyPanels[pageNumber-1].SetActive(true);
            activePage = storyPanels[pageNumber-1];
        }

        //th first narrative clue is open, opens the final unlocked story pannel
        else if (firstPage.activeSelf&& ! storyboardOpen)
        {
            activePage.SetActive(false);
            storyPanels[unlockedPages-1].SetActive(true);
            lastPage= storyPanels[unlockedPages-1];
            firstPage= storyPanels[0];
            activePage = storyPanels[unlockedPages-1];
            storyboardOpen = true;
            

        }
        //narrative clues are open, the first page is not open
        else if (!storyboardOpen && !firstPage.activeSelf)
        {
            pageNumber=narrativeClues.IndexOf(activePage);
            activePage.SetActive(false);
            narrativeClues[pageNumber-1].SetActive(true);
            activePage = narrativeClues[pageNumber-1];
        }

        
        
    }

    
    public void UnlockPage()
    {
        if(unlockedPages == storyPanels.Count) return;
        unlockedPages++;
        lastPage = storyPanels[unlockedPages-1];
        activePage=lastPage;
        
    }

    public void UnlockClue()
    {
         if(unlockedClues == narrativeClues.Count ) return;
         storyboardOpen=false;
         unlockedClues++;
         lastPage = narrativeClues[unlockedClues-1];
         firstPage=narrativeClues[0];
         
         activePage= lastPage;
    }

}
