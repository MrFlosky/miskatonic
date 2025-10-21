using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using Yarn.Unity;


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

    private List<Mission> availableMissions = new List<Mission>();
    private List<Mission> discardedMissions = new List<Mission>();
    private List<Mission> completedMissions = new List<Mission>();

    [SerializeField] private Canvas dealerCanvas;
    [SerializeField] private GameObject blankMissionCard;

    // Needed for mission rewards!
    // Im having missions access through static variables
    // here in order to keep the class structure contained.
    [SerializeField] private GameObject skillObject;
    static public GameObject skillObjectStatic;
    static public NarrativeRewardTracker trackerStatic;

    // Needed for initializing all missions
    private CultistPoints cultistPointSystem;

    void Start()
    {
        S = this;
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
        // Generate random index, grab that mission from available mission list,
        // then remove it from there. Add to in progress missions.

        Debug.LogWarning(availableMissions.Count + "Cards remaining in the pool");

        for (int i = 0; i < 3; i++)
        {
            Mission chosenMission = null;
            int totalMissionsAvailable = availableMissions.Count;

            int randomMissionIndex = Random.Range(0, totalMissionsAvailable); // exclusive upper bound
            chosenMission = availableMissions[randomMissionIndex];

            discardedMissions.Add(chosenMission);
            availableMissions.RemoveAt(randomMissionIndex);

            // Display randomly chosen mission with offsets on mission canvas
            GameObject missionObj = chosenMission.gameObject;
            missionObj.transform.SetParent(dealerCanvas.transform);
            missionObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsets[i], -100);
            missionObj.SetActive(true);
        }

        // TEMPORARY FOR PROTO!!!!!!!!!!
        //TempAddMissionsBack();
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
    /// Removes given card from play pool for rest of game.
    /// Used for one time use missions.
    /// </summary>
    public void RemoveCardFromPool(Mission mission)
    {
        // Make sure to remove from both! Safe to call
        // remove even if not present in list.
        availableMissions.Remove(mission);
        discardedMissions.Remove(mission);

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
                availableMissions.Add(newMission);
                
            }
        }
        Debug.LogWarning(availableMissions.Count);
    }




}
