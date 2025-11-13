using System.Collections;
using System.Collections.Generic;
using CharacterData;
using UnityEngine;

public class UnlockStoryboard : NarrativeReward
{

    public Character targetsStoryboard;
    public static List <OpenBook> BookObjects = new List<OpenBook>();

    [Tooltip("Check this box if the reward is a narrative clue. Leave unchecked for storyboard rewards")]
    [SerializeField] bool Clue;


    public override void DisplayRewardMessage()
    {
        throw new System.NotImplementedException();
    }

    public override void GrantReward()
    {
        
        foreach(var bk in BookObjects)
        {
            if(targetsStoryboard != bk.charactersBook) continue;
           
            else
            {
                if (Clue)
                {
                    
                }
                else
                { 
                 
                    bk.UnlockPage();
                    
                }
            }
        }

    }

    public override void InitRewardUI()
    {
       // throw new System.NotImplementedException();
    }
    
    
}
