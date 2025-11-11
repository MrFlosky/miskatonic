using System;
using System.Collections;
using System.Collections.Generic;
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

    private GameObject firstPage, lastPage;
    public GameObject activePage;
    public int unlockedPages;
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

            else
            {
                storyPanels.Add(t.gameObject);
            }
        }
        firstPage = storyPanels[0];
        int listCount = storyPanels.Count-1;
        lastPage = storyPanels[0];
        gameObject.SetActive(false);
    }

    public void NextPage()
    {
      
        if (lastPage.activeSelf) return;
        int pageNumber = storyPanels.IndexOf(activePage);
        activePage.SetActive(false);
        storyPanels[pageNumber + 1].SetActive(true);
        activePage = storyPanels[pageNumber + 1];
        
        

    }
    
    public void PreviousPage()
    {
        if (firstPage.activeSelf) return;
        int pageNumber = storyPanels.IndexOf(activePage);
        activePage.SetActive(false);
        storyPanels[pageNumber - 1].SetActive(true);
        activePage = storyPanels[pageNumber - 1];
        
    }


}
