using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

/// <summary>
/// Handles save data by writing/reading data to/from individual binary files, as well as managing a unique binary file that holds information about all the saves managed
/// 
/// NOTE: This class uses BinaryFormatter, rendering it insecure against loading potential malicious saves
/// The serialization will therefore need to be refactored if/when we want to release to general public
/// </summary>
public class SaveManager : MonoBehaviour
{


    /*
    TODO:
        High Prio
        - Save/Load UIs
        - Reading map base and changes made to the map (barriers placed/walls destroyed)
        - Reading ability unlocks
        - Reading dispatch deck state
        - Reading fears discovered
        - Reading dialogue info? (idk if its based on turns or previous dialogue)
        - Loading save data into scene

        Med Prio
        - Warn user when a save will be overwritten
        - Autosave

        Low Prio
        - Sort saves by Date
        - Capped amt of saves?
        - Reformat to JSON or proprietary binary serialization
    */



    public static SaveManager S;
    // A list of SaveDisplayData, to be used by UI to display the saves that are currently stored
    private List<SaveDisplayData> saves;


    /// <summary>
    /// Saves current scene data into a save with name saveName (must be called in the main scene!)
    /// </summary>
    /// <param name="saveName"></param>
    public void Save(string saveName)
    {
        // Create file in a persistent OS agnostic location
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + saveName + "player.mis";
        FileStream stream = new FileStream(path, FileMode.Create);


        // Fetch necessary player data
        PlayerData data = new PlayerData(
            TurnBasedSystem.GET_TURN_COUNT(),
            CultistPoints.GetPoints(),
            FearMeter.GET_METER_TOTAL(),
            AgentManager.Instance.agents[0].transform.position,
            AgentManager.Instance.agents[1].transform.position,
            AgentManager.Instance.agents[2].transform.position,
            AgentManager.Instance.agents[3].transform.position
        );

        // Send to binary file
        formatter.Serialize(stream, data);
        stream.Close();

        // Add to our list of available saves
        SaveDisplayData d = new SaveDisplayData(saveName, System.DateTime.Now.ToString(), TurnBasedSystem.GET_TURN_COUNT().ToString());
        AddSave(d);
    }



    /// <summary>
    /// Loads a save with the given saveName
    /// </summary>
    /// <param name="saveName"></param>
    public void Load(string saveName)
    {
        // Check if file with given saveName exists
        string path = Application.persistentDataPath + "/" + saveName + "player.mis";
        if (!File.Exists(path))
        {
            Debug.LogError("No save file found at " + path + "!!");
            return;
        }

        // Deserialize our save data as a PlayerData object
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);
        PlayerData data = formatter.Deserialize(stream) as PlayerData;
        stream.Close();

        // Load this data into a new scene and then open it [WIP]
        // This is the hard part lol

    }

    // Adds a save to the list in memory as well as to our saved binary file
    private void AddSave(SaveDisplayData dat)
    {
        // saves is the list of saves that we have in memory
        saves.Add(dat);

        // saves.mis stores only the data about what saves exist for loading UI and such
        BinaryFormatter f = new BinaryFormatter();
        string path = Application.persistentDataPath + "/saves.mis";

        // FileMode.Create overwrites any existing files, meaning the 'saves' list must always contain what is in the binary file, hence why it is loaded in Awake()
        FileStream stream = new FileStream(path, FileMode.Create);
        f.Serialize(stream, saves);

        stream.Close();
    }

    /// <summary>
    /// Returns an array of all SaveDisplayData currently stored in the SaveManager
    /// </summary>
    /// <returns></returns>
    public SaveDisplayData[] GetSaveInformation()
    {
        return saves.ToArray();
    }
















    void Awake()
    {
        if (S == null) S = this;
        else Debug.LogError("There cannot be more than one SaveManager in scene!!!");

        // Load information abt saves into memory
        string path = Application.persistentDataPath + "/saves.mis";
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            List<SaveDisplayData> ls = bf.Deserialize(stream) as List<SaveDisplayData>;
            saves = ls;
        }
        else
        {
            Debug.Log("No save information found!");
        }
        

    }


}
