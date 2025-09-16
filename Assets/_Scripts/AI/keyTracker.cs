using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyTracker : MonoBehaviour
{
    public GameObject Key;

    //placeholder script to turn key object on when goal room is reached. Should be deleted and replaced when able to
        

    void Start()
    {if (Key == null)
        {
            Debug.LogError(gameObject.name + " has no key assigned. Assign key in inspector to have key appear once goal room is reached");
            return;
        }
        Key.SetActive(false);
        
    }



}
