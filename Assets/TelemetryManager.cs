using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TheKiwiCoder;

public class TelemetryManager : MonoBehaviour, ITurnObserver
{
    public static TelemetryManager S;
    public MissionData[] missionsToCollectDataOn;
    private Dictionary<string, int> discardCounts = new Dictionary<string, int>();
    private Dictionary<string, int> chosenCounts = new Dictionary<string, int>();
    private Dictionary<string, int> skillUsageCounts = new Dictionary<string, int>();

    // used to prevent double counting of games with the same ID
    private int game_id;
    private bool won;
    private int turn;
    private bool lostOnTime;
    private bool lostOnKeys;
    private string logFilePath;

    void Start()
    {
        if (S != null)
        {
            Destroy(this);
        } else
        {
            S = this;
        }
        TurnBasedSystem.SubscribeAsObserver(this);
        // set the game id to our maze gen seed, as this value is random on every new game, but is preserved across saves
        game_id = MazeGenerator.Instance.seed;
        turn = 1;
        logFilePath = Application.persistentDataPath + "/gameplaydata.txt";

        for (int i = 0; i < missionsToCollectDataOn.Length; i++)
        {
            string m = missionsToCollectDataOn[i].title;
            discardCounts.Add(m, 0);
            chosenCounts.Add(m, 0);
        }

        List<Skill> skills = SkillsCollection.S.GetSkills();
        foreach (Skill s in skills)
        {
            skillUsageCounts.Add(s.GetType().Name, 0);
        }
    }

    public void CountDiscard(string title)
    {
        if (discardCounts.ContainsKey(title)) discardCounts[title]++;
    }

    public void CountPick(string title)
    {
        if (chosenCounts.ContainsKey(title)) chosenCounts[title]++;
    }

    public void CountSkillUsage(string name, int amt)
    {
        if (skillUsageCounts.ContainsKey(name)) skillUsageCounts[name] += amt;    
    }

    public void Won()
    {
        won = true;
        SendReport();
    }

    public void Lost(bool OnTime)
    {
        lostOnTime = OnTime;
        lostOnKeys = !OnTime;
        SendReport();
    }

    /// <summary>
    /// Sends all collected data (currently to a log file, can be reworked to work with a database provider like Unity Analytics, Firebase, or GameAnalytics)
    /// </summary>
    public void SendReport()
    {
        Debug.LogWarning("Sent telemetry report to: '" + logFilePath + "'!");
        File.AppendAllText(logFilePath, "Game ID," + game_id.ToString() + "\n");

        File.AppendAllText(logFilePath, "Discarded:\n");
        foreach (KeyValuePair<string,int> d in discardCounts)
        {
            File.AppendAllText(logFilePath, d.Key + "," + d.Value.ToString() + "\n");
        }


        File.AppendAllText(logFilePath, "Picked:\n");
        foreach (KeyValuePair<string,int> d in chosenCounts)
        {
            File.AppendAllText(logFilePath, d.Key + "," + d.Value.ToString() + "\n");
        }

        File.AppendAllText(logFilePath, "Turn," + turn.ToString() + "\n");

        if (won)
        {
            File.AppendAllText(logFilePath, "Won\n");
        } else if (lostOnKeys)
        {
            File.AppendAllText(logFilePath, "Lost On Keys\n");
        } else if (lostOnTime)
        {
            File.AppendAllText(logFilePath, "Lost On Time\n");
        } else
        {
            File.AppendAllText(logFilePath, "Incomplete Session\n");
        }

        File.AppendAllText(logFilePath, "END DATA\n\n");
    }



    void ITurnObserver.EnemyTurnInitiated()
    {
        SendReport();
    }

    void ITurnObserver.PlayerTurnInitiated()
    {
        turn = TurnBasedSystem.GET_TURN_COUNT(TurnType.Player);
    }

}
