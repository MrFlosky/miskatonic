using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearObjEffect : MonoBehaviour
{
    bool seen = false;
    public int sightFear;
    public int target;
    bool canTargetSelect;

    private void Start()
    {
        Fear fear = GetComponent<Fear>();
    }



    public void TargetSighted()
    {
        seen = true;
        Fear fear = GetComponent<Fear>();
        fear.AddFear(sightFear);
    }

}
