using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using System;

public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager S;
    public MissionData[] missionsToCollectDataOn;
    private Dictionary<string, int> discardCounts;
    private Dictionary<string, int> chosenCounts;
    private Dictionary<string, int> skillUsageCounts;

    void Start()
    {
        if (S != null)
        {
            Destroy(this);
        } else
        {
            S = this;
        }
        GameAnalytics.Initialize();
        for (int i = 0; i < missionsToCollectDataOn.Length; i++)
        {
            string m = missionsToCollectDataOn[i].title;
            discardCounts.Add(m, 0);
            chosenCounts.Add(m, 0);
        }
    }

    public void CountDiscard(string title)
    {
        if (discardCounts.ContainsKey(title))
        {
            discardCounts[title]++;
        }
        
    }

    public void CountPick(string title)
    {
        if (chosenCounts.ContainsKey(title)) chosenCounts[title]++;
    }


}
