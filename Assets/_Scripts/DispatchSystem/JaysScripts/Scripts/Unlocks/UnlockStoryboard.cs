using System.Collections;
using System.Collections.Generic;
using CharacterData;
using UnityEngine;

public class UnlockStoryboard : NarrativeReward
{

    public Character targetsStoryboard;
    public static List <OpenBook> BookObjects = new List<OpenBook>();




    public override void DisplayRewardMessage()
    {
        throw new System.NotImplementedException();
    }

    public override void GrantReward()
    {
        Debug.LogError(targetsStoryboard);
        foreach(var bk in BookObjects)
        {
            if(targetsStoryboard != bk.charactersBook) continue;
           
            else
            {
                bk.UnlockPage();
            }
        }

    }

    public override void InitRewardUI()
    {
       // throw new System.NotImplementedException();
    }
    
}
