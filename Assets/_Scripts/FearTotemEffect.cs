using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FearTotemEffect : MonoBehaviour
{
    /// <summary>
    /// this will raise the fear if it is seen by a specific character while having no effect on other characters
    /// 
    /// currently there is no way to pick a target, this will be done at a later date
    /// </summary>

    //ensures the effect only triggers once
    public bool seen = false;
    
    public int sightFear;
    
    //should corrispoind with an int on the target
    public int targetInt;


   public void TotemSeen()
    {
        if(seen) return;

        Fear fear = GetComponent<Fear>();
        fear.AddFear(sightFear);


    }
   

}
