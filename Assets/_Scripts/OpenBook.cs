using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBook : MonoBehaviour
{

    public List<GameObject> pageTurners = new List<GameObject>();
    public List<GameObject> storyPanels = new List<GameObject>();
    
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



    }

    public void turnRight()
    {


    }
    
    public void turnLeft()
    {
        
    }


}
