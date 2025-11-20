using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;

public class TelemetryManager : MonoBehaviour
{
    void Start()
    {
        GameAnalytics.Initialize();
    }
}
