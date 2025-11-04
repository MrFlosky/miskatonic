using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

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
    public TextMeshProUGUI missionTitle { get; private set; }
    public TextMeshProUGUI missionDescription { get; private set; }
    public TextMeshProUGUI cultistCost { get; private set; }
    public TextMeshProUGUI timeCost { get; private set; }
    public TextMeshProUGUI rewardName { get; private set; }

    // Button to select this mission
    private Button selectMissionButton;

    // Reward associated with this mission
    public Reward missionReward { get; private set; }

    // Mission data associated with this mission
    private MissionData missionData;

    // Reference to cultist point class
    private CultistPoints cultistPointSystem;

    /// <summary>
    /// Whether this card stays in card play pool after being selected.
    /// </summary>
    private bool oneTimeUse;

    /// <summary>
    /// Initialize this mission with data from scriptable object passed
    /// from mission collection!
    /// </summary>
    public void InitMission(MissionData data, CultistPoints cps)
    {
        missionData = data;
        missionCardObj = transform.gameObject;
        cultistPointSystem = cps;
        oneTimeUse = data.oneTimeUse;

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
    /// Upon clicking this mission, activate it if user has enough cultist points.
    /// </summary>
    public void MissionSelect()
    {
        if (cultistPointSystem.AttemptMissionPurchase(missionData.cultistCost))
        {
            missionReward.GrantReward();
            DispatchSystem.DealingOver();

            // if card is a one time use only, remove it from pool
            if(oneTimeUse)
            {
                Debug.LogWarning("CARD REMOVED FROM POOL");
                MissionCollection.S.RemoveCardFromPool(this);
                //Destroy(this.gameObject);
            }
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

        rewardName = transform.GetChild(6).GetComponent<TextMeshProUGUI>();
        rewardName.text = "Reward: " + missionReward.rewardName;

        switch (missionReward)
        {
            case SkillReward skillReward:
                skillReward.InitSkill();
                skillReward.SetUsages(missionData.usages);
                break;
            case RewardAdvertising rewardAdvertising:
                rewardAdvertising.Init(cultistPointSystem);
                rewardAdvertising.SetCultistsRewarded(missionData.cultistsGranted);
                break;
            case RewardCultistMasses rewardCultistMasses:
                rewardCultistMasses.Init(cultistPointSystem);
                rewardCultistMasses.SetCultists();
                break;
            case RewardCultist rewardCultist:
                rewardCultist.Init(cultistPointSystem);
                rewardCultist.SetCultistsRewarded(missionData.cultistsGranted);
                break;
            case NarrativeReward narrativeReward:
                narrativeReward.InitNarrativeClue(missionData.character);
                break;
            case UnlockReward unlockReward:
                unlockReward.InitSkill();
                break;
            case RewardNavPreview navPreviewReward:
                navPreviewReward.SetPreviewsRewarded(missionData.previewsGranted);
                break;
            default:
                // no skill reward given
                break;

        }

        // at end, all mission rewards have ui that needs to be init
        missionReward.InitRewardUI();
    }

    /// <summary>
    /// Return if this mission is only a one time use or not
    /// </summary>
    public bool IsOneTimeUse()
    {
        return oneTimeUse;
    }

    public MissionData GetMissionData()
    {
        return missionData;
    }



}
