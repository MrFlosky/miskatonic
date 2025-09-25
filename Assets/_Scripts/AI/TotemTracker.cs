using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotemTracker : MonoBehaviour
{
    public int characterNum;
    [SerializeField]
    float raycastDistance;
    private void Start()
    {


    }

    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position, Vector3.forward * raycastDistance, Color.magenta);
    }



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
                    FTE.TargetSighted();
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
