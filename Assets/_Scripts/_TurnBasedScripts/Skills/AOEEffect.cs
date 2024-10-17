using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AOEEffect : MonoBehaviour
{   
    [Header("How much fear is adding each turn an AI is in this AOE")]
    public int perTurnFear = 5;
    public BoxCollider boxColl; // gonna try turning on and off coll to reactivate ontrigger each turn. done by turnbasedsys
    void Awake()
    {
        boxColl = transform.GetComponent<BoxCollider>();
        boxColl.enabled = false;
    }

    /// <summary>
    /// Turn system responsible for turning on and off coll to do this
    /// </summary>
    /// <param name="other"></param>
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("COLL");
        if(other.GetComponent<Fear>() != null)
        {
            Fear fearOfSomeAI = other.GetComponent<Fear>();
            fearOfSomeAI.AddFear(perTurnFear);
            Debug.Log(" FEAR ADDED " + perTurnFear);
        }
    
    }
}
