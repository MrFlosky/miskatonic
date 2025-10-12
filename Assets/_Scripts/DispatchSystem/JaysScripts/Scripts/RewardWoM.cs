using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RewardWoM : RewardCultist, ITurnObserver
{
    public static RewardWoM S { get; private set; }
    public bool activated;
    public int maxGain;
    
    
    



    private void Start() 
    {
        
        if (S != null && S != this) Destroy(this);

        if(S== null) S = this;

        
        
        
    }

    private void Update()
    {
        
    }

    void ITurnObserver.PlayerTurnInitiated()
    {

        cultistsToReward = Mathf.FloorToInt(cultistPointSystem.cultistPoints);
        print(cultistsToReward);
        GrantReward();
    }


    void ITurnObserver.EnemyTurnInitiated() { }
}
