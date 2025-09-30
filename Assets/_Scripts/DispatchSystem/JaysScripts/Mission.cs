using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for filling in the mission UI given a scriptable object
/// and performing a mission selection
/// </summary>
public class Mission : MonoBehaviour
{
    // GameObject this mission script is attached to, obj created in MissionCollection.
    private GameObject missionCardObj;

    // IMPORTANT: GETTING THESE TEXT VALUES VIA THEIR CHILD INDEX UNDER THE PREFAB! 
    // Text values for a mission card.
    private TextMeshProUGUI missionTitle;
    private TextMeshProUGUI missionDescription;
    private TextMeshProUGUI cultistCost;
    private TextMeshProUGUI timeCost;

    // Button to select this mission
    private Button selectMissionButton;

    // Reward associated with this mission
    private Reward missionReward;

    // Mission data associated with this mission
    private MissionData missionData;

    // Reference to cultist point class
    private CultistPoints cultistPointSystem;

    /// <summary>
    /// Initialize this mission with data from scriptable object passed
    /// from mission collection!
    /// </summary>
    public void InitMission(MissionData data, CultistPoints cps)
    {
        missionData = data;
        missionCardObj = transform.gameObject;
        cultistPointSystem = cps;
        InitUI(missionData);
        ProcessReward(missionData.reward);

    }

    /// <summary>
    /// Grab all relevant UI components attached to this mission card.
    /// </summary>
    private void InitUI(MissionData data)
    {
        missionTitle = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        missionDescription = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        cultistCost = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        timeCost = transform.GetChild(4).GetComponent<TextMeshProUGUI>();
        selectMissionButton = transform.GetChild(5).GetComponent<Button>();
        selectMissionButton.onClick.AddListener(MissionSelect);

        missionTitle.text = data.title;
        missionDescription.text = data.description;

        // always assume cultist cost is deduction, even if 0.
        // handle cultist reward as reward obj
        cultistCost.text = "-" + data.cultistCost.ToString() + " Cultists";
        


        timeCost.text = "Time: " + data.turnActivateTime.ToString();
        missionCardObj.name = "Mission: " + missionTitle.text;
    }

    /// <summary>
    /// Later this will check for cultist cost before allowing
    /// the selection.
    /// </summary>
    private void MissionSelect()
    {
        if (cultistPointSystem.AttemptMissionPurchase(missionData.cultistCost))
        {
            missionReward.GrantReward();
            DispatchSystem.DealingOver();
        }
        
    }


    /// <summary>
    /// Set rewards specialized data, for example Skill Reward which
    /// will use MissionData's usages member
    /// </summary>
    private void ProcessReward(Reward reward)
    {
        // This is kinda strange, creating a instance of reward prefab.
        // Mostly done bc dont want to allow assigning gameobjects to MissionData
        GameObject rewardPrefab = reward.gameObject;
        // Instantiate prefab to get a instance we can work with
        GameObject rewardObj = Instantiate(rewardPrefab);
        //rewardObj.transform.SetParent(missionCardObj.transform); TODO: we need these to be outside mission card obj, organize later
        missionReward = rewardObj.GetComponent<Reward>();

        transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "Reward: " + missionReward.rewardName;

        switch (missionReward)
        {
            case SkillReward skillReward:
                skillReward.InitSkill();
                skillReward.SetUsages(missionData.usages);
                break;
            case RewardCultist rewardCultist:
                rewardCultist.Init(cultistPointSystem);
                rewardCultist.SetCultistsRewarded(missionData.cultistsGranted);
                break;
            default:
                // no skill reward given
                break;

        }

        // at end, all mission rewards have ui that needs to be init
        missionReward.InitRewardUI();
    }



}
