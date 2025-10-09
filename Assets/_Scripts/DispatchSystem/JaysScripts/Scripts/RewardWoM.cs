using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardWoM : RewardCultist, ITurnObserver
{
    public List <int> thresholds;
    


    private void Update()
    {
        
    }



    void ITurnObserver.PlayerTurnInitiated() { }

   void ITurnObserver.EnemyTurnInitiated()
    {

    }
}
