using System;
using System.Collections;
using System.Collections.Generic;
using CharacterData;
using UnityEngine;

public class OpenBook : MonoBehaviour
{
    /// <summary>
    /// Handles the logic for changing pages
    /// 
    /// The open book object must be active when the scene starts to ensure that the list of pages and page turners is properly populated.
    /// It will deactivate on start.
    /// By default only one page will be active and viewable, more can be unlocked via the dispatch system (WIP)
    /// 
    /// </summary>

    public List<GameObject> pageTurners = new List<GameObject>();
    public List<GameObject> storyPanels = new List<GameObject>();
    public List<GameObject> narrativeClues = new List<GameObject>();

    public GameObject firstPage, lastPage;
    public GameObject activePage;
    public int unlockedPages;
    public int unlockedClues;
    
    [SerializeField]bool storyboardOpen=true; 
    
    public Character charactersBook;
    void Start()
    {
        unlockedPages = 1;
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
        UnlockStoryboard.BookObjects.Add(this);
    }

    public void NextPage()
    {
      
        if (lastPage.activeSelf&&storyboardOpen)
        {
            print("F");
            if(unlockedClues==0) return;
            print("YEs");
            storyboardOpen=false;
            activePage.SetActive(false);
            narrativeClues[0].SetActive(true);

            return;
        }
        
        else if(!lastPage.activeSelf && storyboardOpen)
        {
            int pageNumber = storyPanels.IndexOf(activePage);
            activePage.SetActive(false);
            storyPanels[pageNumber + 1].SetActive(true);
            activePage = storyPanels[pageNumber + 1];
        }
        
        else if(!storyboardOpen&& !lastPage.activeSelf)
        {
            
        }
        

    }
    
    public void PreviousPage()
    {
        print("FDSA");
        if (!storyboardOpen)
        {   
            print("Fe");
            storyPanels[unlockedPages-1].SetActive(true);
        }



        // else if(!firstPage.activeSelf )
        // {
        //     int pageNumber = storyPanels.IndexOf(activePage);
        //     activePage.SetActive(false);
        //     storyPanels[pageNumber - 1].SetActive(true);
        //     activePage = storyPanels[pageNumber - 1];
        // }
        
    }

    public void UnlockPage()
    {
        if(unlockedPages == storyPanels.Count) return;
        lastPage = storyPanels[unlockedPages];
        unlockedPages++;
    }

    public void UnlockClue()
    {
         
    }

}
