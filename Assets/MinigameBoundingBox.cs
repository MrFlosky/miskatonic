using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameBoundingBox : MonoBehaviour
{
    void OnTriggerEnter(Collider coll)
    {
        Debug.Log(coll.gameObject.name);
        Unconverted u = coll.gameObject.GetComponent<Unconverted>();
        if (u != null)
        {
            Destroy(coll.gameObject);
        }
    }
}
