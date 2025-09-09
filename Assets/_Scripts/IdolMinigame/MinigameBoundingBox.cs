using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameBoundingBox : MonoBehaviour
{
    void OnTriggerEnter(Collider coll)
    {
        GamePiece u = coll.gameObject.GetComponent<GamePiece>();
        if (u != null)
        {
            Destroy(coll.gameObject);
        }
    }
}
