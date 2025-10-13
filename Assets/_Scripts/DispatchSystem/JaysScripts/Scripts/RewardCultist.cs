using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardCultist : Reward
{
    protected int cultistsToReward;
    protected CultistPoints cultistPointSystem;
    [SerializeField]
    private bool WOM;
    [SerializeField]
    GameObject WOMObj; 

    public void Init(CultistPoints cps)
    {
        
        cultistPointSystem = cps;
        
    }

    public void SetCultistsRewarded(int reward)
    {

        cultistsToReward = reward;
    }


    public override void GrantReward()
    {
        //checks to see if the card being activated is the word of mouth and runs a different process for that. 
        //if we plan on having more persistent upgrades or things that don't just immediately add a flat amount of cultists we should look into creating a new system for that
        if (WOM)
        {
           
            DisplayRewardMessage();

            GameObject GO = Instantiate(WOMObj);
            GO.GetComponent<RewardWoM>().activated = true;
            
            return;

        }
        
        cultistPointSystem.AddPoints(cultistsToReward);
        Debug.LogError(cultistPointSystem.cultistPoints);
        DisplayRewardMessage();
        
    }

    public override void InitRewardUI()
    {
        this.enabled = false;
        GameObject rewardPopup = Instantiate(rewardPopupUI); // instantiate prefab and set ref
        rewardPopupUI = rewardPopup;
        rewardPopupUI.SetActive(false);

        rewardText = rewardPopupUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        rewardText.text = cultistsToReward.ToString() + " " + rewardName;
    }
        

    /// <summary>
    /// The display reward message will need altering to accomodate the word of mouth. 
    /// 
    /// Again if we plan to add more cards that are not just a immediate flat gain of cultists I'd reccomend us building a new function
    /// </summary>
    public override void DisplayRewardMessage()
    {
        rewardPopupUI.SetActive(true);
        timeElapsed = 0;
        this.enabled = true;
    }

    // only on when enabled
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= timeToDissapear)
        {
            rewardPopupUI.SetActive(false);
            this.enabled = false;
        }
    }

   
}
