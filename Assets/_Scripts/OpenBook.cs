using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBook : MonoBehaviour
{

    public List<GameObject> pageTurners = new List<GameObject>();
    public List<GameObject> storyPanels = new List<GameObject>();

    public GameObject firstPage, lastPage;
    public GameObject activePage;
    void Start()
    {

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
        lastPage = storyPanels[listCount];
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
