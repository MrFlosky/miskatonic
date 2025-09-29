using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotemTracker : MonoBehaviour
{

    /// <summary>
    /// this script will send a raycast out to look for Fear Totems when the character is in motion
    /// </summary>



    //used to check to see if the totem is the correctly targeted one
    public int characterNum;
    
    [SerializeField]
    float raycastDistance;
   
    
    private void Start()
    {


    }

    private void FixedUpdate()
    {
        //for testing, draws the raycast at all times
        //Debug.DrawRay(transform.position, Vector3.forward * raycastDistance, Color.magenta);
    }


    //searches for totem, will only increase fear if the totem is correctly targeting this character
    public void LookForTotem()
    {
            

        //for testing purposes, draws the raycast tnat searches for a totem
        Debug.DrawRay(transform.position, Vector3.forward * raycastDistance, Color.magenta);


        RaycastHit hit;
        FearTotemEffect FTE;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward),out hit, raycastDistance))
        {
            if (hit.transform.GetComponent<FearTotemEffect>())
            {
                FTE = hit.transform.GetComponent<FearTotemEffect>();
                Debug.Log(gameObject.name + " saw a fear totem");

                if (FTE.seen)
                {
                    return;
                }


                if (characterNum == FTE.targetInt)
                {
                    FTE.seen = true;
                    Debug.Log("The fear totem was targeted at this character, fear has increased!");
                    Fear fear = GetComponent<Fear>();
                    fear.AddFear(FTE.sightFear);

                }

                else
                {
                    Debug.Log("The fear totem was not targeted at " + gameObject.name + ". Fear is not increased.");
                }
            }
            
           

            
            
        }


    }

}
