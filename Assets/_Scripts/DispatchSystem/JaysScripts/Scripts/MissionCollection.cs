using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

/// <summary>
/// All possible types of cards.
/// </summary>
public enum CardType
{
    Economy,
    Skill,
    Narrative,
    Debug
}

[System.Serializable]
public class MissionCardEntry
{
    public MissionData missionData;
    public int cardCount;
}

/// <summary>
/// This class is responsible for managing the collection of missions, which includes:
/// - Displaying/hiding missions
/// - Shuffling missions
/// - Processing/initializing mission data 
/// </summary>
public class MissionCollection : MonoBehaviour
{
    static public MissionCollection S;

    [Header("All missions presented to the player and how many of each to add to pool")]
    [SerializeField] private MissionCardEntry[] missionCards;

    /// <summary>
    /// Available msissions to draw from, uncategorized.
    /// </summary>
    private List<Mission> availableMissions = new List<Mission>();
    private List<Mission> discardedMissions = new List<Mission>();
    private List<Mission> completedMissions = new List<Mission>();


    /// <summary>
    /// Available msissions to draw from, categorized.
    /// </summary>
    private const int typesOfMissions = 3;
    private List<Mission> availableEcon = new List<Mission>();
    private List<Mission> availableSkill = new List<Mission>();
    private List<Mission> availableNarrative = new List<Mission>();

    // Flags to track if COMPLETELY out of skills or narrative since they are single use.
    private bool ranOutOfSkillsPermanently = false;
    private bool ranOutOfNarrativePermanently = false;

    [SerializeField] private Canvas dealerCanvas;
    [SerializeField] private GameObject blankMissionCard;

    [SerializeField] private GameObject[] worldCards;

    // Needed for mission rewards!
    // Im having missions access through static variables
    // here in order to keep the class structure contained.
    [SerializeField] private GameObject skillObject;
    static public GameObject skillObjectStatic;
    static public NarrativeRewardTracker trackerStatic;

    // Needed for initializing all missions
    private CultistPoints cultistPointSystem;

    void Awake()
    {
        if(S != null && S != this)
        {
            Destroy(gameObject);
            return;
        }
        S = this;
    }

    // clear static ref on scene unload
    void OnDestroy()
    {
        if(S == this)
        {
            S = null;
        }
    }

    void Start()
    {
        skillObjectStatic = skillObject;
        trackerStatic = GetComponent<NarrativeRewardTracker>();
        cultistPointSystem = GetComponent<CultistPoints>();
        ProcessMissionData();
    }


    private float[] offsets = { -380f, 0f, 380f };
    /// <summary>
    /// Display 3 random available missions to the player.
    /// </summary>
    public void DISPLAY_AVAILABLE_MISSIONS()
    {
        // override this call with categorized version if using category draw
        if(categoryDraw)
        {
            DISPLAY_AVAILABLE_MISSIONS_CATEGORIZED();
            return;
        }

        ShuffleCheck();
        // Generate random index, grab that mission from available mission list,
        // then remove it from there. Add to discard.

        Debug.LogWarning(availableMissions.Count + "Cards remaining in the pool");

        for (int i = 0; i < 3; i++)
        {
            Mission chosenMission = null;
            int totalMissionsAvailable = availableMissions.Count;

            int randomMissionIndex = Random.Range(0, totalMissionsAvailable); // exclusive upper bound
            chosenMission = availableMissions[randomMissionIndex];

            discardedMissions.Add(chosenMission);
            TelemetryManager.S.CountDiscard(chosenMission.GetMissionData().title);
            availableMissions.RemoveAt(randomMissionIndex);

            // Display randomly chosen mission with offsets on mission canvas
            GameObject missionObj = chosenMission.gameObject;
            missionObj.transform.SetParent(dealerCanvas.transform);
            missionObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsets[i], -100);
            missionObj.SetActive(true);

            worldCards[i].GetComponent<MissionDisplay>().UpdateMission(chosenMission);
        }

        // TEMPORARY FOR PROTO!!!!!!!!!!
        //TempAddMissionsBack();
    }

    /// <summary>
    /// Display 3 random available missions to the player, 1 econ, 1 ability, 1 narrative.
    /// </summary>
    public void DISPLAY_AVAILABLE_MISSIONS_CATEGORIZED()
    {

        ShuffleCheckCategorized();


        // Generate random index for EACH type of mission list, grab that mission from available mission list,
        // then remove it from there. Add to discard.
        List<Mission>[] missionLists = new List<Mission>[] {availableEcon, availableSkill, availableNarrative};

        for (int i = 0; i < 3; i++)
        {
            List<Mission> missionList;
            missionList = missionLists[i]; 

            // If this list is one that is perma empty, use econ cards in its place.
            if((missionList == availableSkill && ranOutOfSkillsPermanently) || (missionList == availableNarrative && ranOutOfNarrativePermanently))
            {
                missionList = availableEcon;
            }

            Mission chosenMission = null;
            int totalMissionsAvailable = missionList.Count;

            int randomMissionIndex = Random.Range(0, totalMissionsAvailable); // exclusive upper bound
            chosenMission = missionList[randomMissionIndex];

            discardedMissions.Add(chosenMission);
            TelemetryManager.S.CountDiscard(chosenMission.GetMissionData().title);
            missionList.RemoveAt(randomMissionIndex);


            // Display randomly chosen mission with offsets on mission canvas
            GameObject missionObj = chosenMission.gameObject;
            missionObj.transform.SetParent(dealerCanvas.transform);
            missionObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsets[i], -100);
            missionObj.SetActive(true);

            worldCards[i].GetComponent<MissionDisplay>().UpdateMission(chosenMission);
        }
    }

    /// <summary>
    /// Check if not enough cards to display (3) from available missions and if so,
    /// perform a shuffle with discarded missions.
    /// </summary>
    public void ShuffleCheck()
    {
        if (availableMissions.Count < 3)
        {
            Debug.LogWarning("SHUFFLE INTIATED");
            // loop backwards bc we removing elements from collection
            for (int i = discardedMissions.Count - 1; i >= 0; i--)
            {
                Mission mission = discardedMissions[i];
                availableMissions.Add(mission);
                discardedMissions.RemoveAt(i);
            }
        }
    } 




    /// <summary>
    /// Check if not enough cards to display from EACH available missions and if so,
    /// perform a shuffle with discarded missions for that type.
    /// </summary>
    public void ShuffleCheckCategorized()
    {
        // Check if each type of list requires cards shuffled into them.
        bool needEcon = availableEcon.Count < 1;
        bool needSkill = availableSkill.Count < 1;
        bool needNarrative = availableNarrative.Count < 1;

        if(!needEcon && !needSkill && !needNarrative){return;} // return early if no list needs a shuffle

        Debug.LogWarning("SHUFFLE INTIATED");
        // Only add discarded cards to the lists that need them 
        for (int i = discardedMissions.Count - 1; i >= 0; i--)        // loop backwards bc we removing elements from collection
        {
            Mission mission = discardedMissions[i];

            // add discarded card to whichever card type list it belongs to,
            // IF that card list needs a shuffle.
            CardType cardType = GetCardCategory(mission);
            switch(cardType)
            {
                case CardType.Economy when needEcon:
                    availableEcon.Add(mission);
                    discardedMissions.RemoveAt(i); // dont want to have a single remove on outside switch otherwise might remove card that was never added to a list.
                    break;

                case CardType.Skill when needSkill:
                    availableSkill.Add(mission);
                    discardedMissions.RemoveAt(i);
                    break;

                case CardType.Narrative when needNarrative:
                    availableNarrative.Add(mission);
                    discardedMissions.RemoveAt(i);
                    break;
            }
        }

        // Check if after shuffling in cards to lists that needed them, theyre still too low.
        // That is, specifically for narrative and skill cards if they ran out since those are not reshuffled (single use).
        // In this case, we will replace them with econ cards since those never run out.
        ranOutOfSkillsPermanently = availableSkill.Count < 1;
        ranOutOfNarrativePermanently = availableNarrative.Count < 1;
    } 

    /// <summary>
    /// Set all mission game objects active state to false.
    /// Called once dealing is over, so on a new deal old missions are no longer
    /// displayed (missions objects never destroyed and are reused).
    /// </summary>
    public void HIDE_ALL_MISSIONS()
    {
        foreach (Mission m in availableMissions)
        {
            m.gameObject.SetActive(false);
        }    
        
        foreach(Mission m in discardedMissions)
        {
            m.gameObject.SetActive(false);
        }    
    }

    public void HIDE_AVAILABLE_MISSIONS()
    {
        foreach (Mission mission in availableMissions)
        {
            mission.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// During dealer interaction, player has option to reroll
    /// the cards presented to them. Called by DispatchSystem.
    /// </summary>
    public void Reroll()
    {
        HIDE_ALL_MISSIONS();
        if(categoryDraw)
        {
           // ShuffleCheckCategorized(); // must do or else will drain card pool. THIS SHOULD ALR BE DONE AT START OF DISPLAY
            DISPLAY_AVAILABLE_MISSIONS_CATEGORIZED();
        } else
        {
           // ShuffleCheck();
            DISPLAY_AVAILABLE_MISSIONS();
        }
    }
    
    /// <summary>
    /// Removes given card from play pool for rest of game.
    /// Used for one time use missions.
    /// </summary>
    public void RemoveCardFromPool(Mission mission)
    {
        // remove from specific category card list if doing category draw, otherwise
        // remove from general pool alone for normal draw.
        if(categoryDraw)
        {
            CardType cardType = GetCardCategory(mission);

            switch(cardType)
            {
                case CardType.Skill:
                    availableSkill.Remove(mission);
                    Debug.LogWarning("CARD SUCCESSFULLY REMOVED FROM POOL");
                    break;
                case CardType.Economy:
                    availableEcon.Remove(mission);
                    Debug.LogWarning("CARD SUCCESSFULLY REMOVED FROM POOL");
                    break;
                case CardType.Narrative:
                    availableNarrative.Remove(mission);
                    Debug.LogWarning("CARD SUCCESSFULLY REMOVED FROM POOL");
                    break;
            }
        } else
        {
            availableMissions.Remove(mission);
        }
        // Make sure to remove from both! Safe to call
        // remove even if not present in list.
        discardedMissions.Remove(mission); // both category draw and regular use same discard list
    }


    /// <summary>
    /// (DEBUG FUNCTION) Until this system gets built further out, we not really using missions in progress
    /// or missions completed for anything. Only displaying random missions each turn.
    /// </summary>
    private void TempAddMissionsBack()
    {
        availableMissions.AddRange(discardedMissions);
        discardedMissions.Clear();
    }


    [SerializeField] private bool categoryDraw = false;
    /// <summary>
    /// Turn MissionData scriptable objects into Mission instances.
    /// Insert respective number of cards into pool.
    /// </summary>
    private void ProcessMissionData()
    {
        for (int i = 0; i < missionCards.Length; i++)
        {
            int cardCount = missionCards[i].cardCount;
            for (int j = 0; j < cardCount; j++)
            {
                GameObject newMissionObj = Instantiate(blankMissionCard);
                Mission newMission = newMissionObj.AddComponent<Mission>();

                newMission.InitMission(missionCards[i].missionData, cultistPointSystem);

                // If doing guaranteed category draw, seperate cards into their respective lists.
                if(categoryDraw)
                {
                    CategorizeCard(newMission);
                } else
                {
                    availableMissions.Add(newMission);
                }
                
                
            }
        }
        //Debug.LogWarning(availableMissions.Count);
    }

    /// <summary>
    /// Given a mission, determine its category based on its reward and add it
    /// to the respective list.
    /// </summary>
    private void CategorizeCard(Mission mission)
    {
        Reward reward = mission.GetMissionData().reward;
        switch(reward)
        {
            // nav preview and hot are exceptions bc they are not considered regular skill unlocks
            case RewardNavPreview: // consider nav preview a skill, BUT its the only one that is not one time use unlock
                                    // this also means skill reward list will never run out but repeat this? so dont need to fill with econ.
            case RewardHandOfFate: // considering hand of fate a skill
            case UnlockReward skillReward: // unlock rewards are always skills
                availableSkill.Add(mission);
                break;
            case RewardCultist rewardCultist:
                availableEcon.Add(mission);
                break;
            case NarrativeReward narrativeReward:
                availableNarrative.Add(mission);
                break;
            default:
                Debug.LogError("This card is neither skill, cultist, or narrative derived " + reward.name);
                break;
        }
    }

    /// <summary>
    /// Given a mission, determine its category based on its reward and add it
    /// to the respective list.
    /// </summary>
    public CardType GetCardCategory(Mission mission)
    {
        Reward reward = mission.GetMissionData().reward;
        switch(reward)
        {
            case RewardNavPreview:
            case RewardHandOfFate:
            case UnlockReward skillReward:
                return CardType.Skill;
            case RewardCultist rewardCultist:
                return CardType.Economy;
            case NarrativeReward narrativeReward:
                return CardType.Narrative;
            default:
                Debug.LogError("This card is neither skill, cultist, or narrative derived");
                return CardType.Debug;
        }
    }


}
