using System.Collections;
using System.Collections.Generic;
using CharacterData;
using UnityEngine;

public class UnlockNarrative : NarrativeReward
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
                    bk.UnlockClue();
                    
                    
                }
                else
                { 
                 
                    bk.UnlockPage();
                    
                }

                foreach(var OB in Bookshelf.S.Booklist)
                {
                    if(OB.charactersBook == bk.charactersBook)
                    {
                        OB.ChangeBook(true,Clue);
                        break;
                    }
                }  

            }


        }

    }

    public override void InitRewardUI()
    {
       // throw new System.NotImplementedException();
    }
    
    
}
