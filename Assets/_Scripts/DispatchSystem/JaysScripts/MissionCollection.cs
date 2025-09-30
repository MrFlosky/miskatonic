using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for managing the collection of missions, which includes:
/// - Displaying/hiding missions
/// - Shuffling missions
/// - Processing mission data
/// </summary>
public class MissionCollection : MonoBehaviour
{
    static public MissionCollection S;

    [Header("All missions that may be presented to the player")]
    [SerializeField] private MissionData[] missionOptions;
    private List<Mission> availableMissions = new List<Mission>();
    private List<Mission> inProgressMissions = new List<Mission>();
    private List<Mission> completedMissions = new List<Mission>();

    [SerializeField] private Canvas missionsCanvas;
    [SerializeField] private GameObject blankMissionCard;

    // Needed for mission rewards!
    [SerializeField] private GameObject skillObject;
    static public GameObject skillObjectStatic;

    // Needed for initializing all missions
    private CultistPoints cultistPointSystem;

    void Start()
    {
        S = this;
        skillObjectStatic = skillObject;
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
        for (int i = 0; i < 3; i++)
        {
            Mission chosenMission = null;
            int totalMissionsAvailable = availableMissions.Count;

            int randomMissionIndex = Random.Range(0, totalMissionsAvailable); // exclusive upper bound
            chosenMission = availableMissions[randomMissionIndex];
            inProgressMissions.Add(chosenMission);
            availableMissions.RemoveAt(randomMissionIndex);

            // Display randomly chosen mission with offsets on mission canvas
            GameObject missionObj = chosenMission.gameObject;
            missionObj.transform.SetParent(missionsCanvas.transform);
            missionObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsets[i], -100);
        }

        // TEMPORARY FOR PROTO!!!!!!!!!!
        TempAddMissionsBack();
    }

    public void HIDE_AVAILABLE_MISSIONS()
    {
        foreach (Mission mission in availableMissions)
        {
            mission.gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// Until this system gets built further out, we not really using missions in progress
    /// or missions completed for anything. Only displaying random missions each turn.
    /// </summary>
    private void TempAddMissionsBack()
    {
        availableMissions.AddRange(inProgressMissions);
        inProgressMissions.Clear();
    }

    /// <summary>
    /// Turn MissionData scriptable objects into Mission instances.
    /// </summary>
    private void ProcessMissionData()
    {
        foreach (MissionData md in missionOptions)
        {
            GameObject newMissionObj = Instantiate(blankMissionCard);
            Mission newMission = newMissionObj.AddComponent<Mission>();

            newMission.InitMission(md, cultistPointSystem);
            availableMissions.Add(newMission);
        }
    }

    


}
