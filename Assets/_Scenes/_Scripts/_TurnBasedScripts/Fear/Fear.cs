using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fear : MonoBehaviour
{

    public int currentFear = 10;
    public int fearThreshold = 20;
    string weakness;
    // Start is called before the first frame update
    void Start()
    {
        // Initiate fear meter with contribution from the fear of this AI
        FearMeter.INCREASE_METER(currentFear);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddFear(int add)
    {
        currentFear += add;
        FearMeter.INCREASE_METER(add);
    }

    public void ReduceFear(int subt)
    {
        currentFear -= subt;
        FearMeter.DECREASE_METER(subt);
    }
}
