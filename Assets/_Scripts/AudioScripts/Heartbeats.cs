using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This is attached to AI fear meter's bckg&fill parent,
/// as it already handles the enable/disabling when needed.
/// </summary>
public class Heartbeats : MonoBehaviour
{
    public static Heartbeats S;
    [Header("Used to scale how much heartbeat pitch increases with fear.")]
    [SerializeField] int heartbeatMaxFear = 400;
    public AudioSource heartbeat;
    public AudioSource mainTheme;
    float heartbeatPitch;
    int maxFear;

    // this was cause for issues with nullobjref bc convomanager awake first?
    void Awake()
    {
        if (S == null)
        {
            S = this; // Assign the instance to S
        }
        else if (S != this)
        {
            Destroy(gameObject); // Ensure there's only one instance
        }
        S.enabled = false;
    }

    private float timeElapsed = 0f;
    private float heartRateChangeSpeed = 0.004f; // smaller the val the faster

    void Update()
    {
        timeElapsed += Time.deltaTime;
        heartbeat.pitch = Mathf.Lerp(heartbeat.pitch, heartbeatPitch, timeElapsed * heartRateChangeSpeed);

        if(heartbeat.pitch == heartbeatPitch)
        {
            S.enabled = false;
        }
    }

    /// <summary>
    /// This used to update relative to the max individual fear of each character.
    /// Keeping this as an option incase needed later on.
    /// Since that is removed now, scaling relative to discrete value set here.
    /// </summary>
    public void StartHeartbeat(int fear, int maxFear)
    {
        AudioManager.SmoothVolumeTransition(mainTheme.volume, mainTheme.volume / 2.5f);  // lower main theme volume so heartbeat better heard

        //S.maxFear = maxFear; old
        S.maxFear = heartbeatMaxFear;
        S.heartbeatPitch = S.FearToHeartBeat(fear);
        S.heartbeat.pitch = S.heartbeatPitch;
        S.heartbeat.Play();
    }

    public void UpdateHeartBeat(int newFear)
    {
        S.heartbeatPitch = S.FearToHeartBeat(newFear);
        S.timeElapsed = 0;
        S.enabled = true; // turn this bad boy on so we can update the heartbeat speed
    }


    float FearToHeartBeat(int fear)
    {
       
        float percentage = fear /((float) maxFear * 2f);
        float pitch = (2f * percentage) + 1f;
        if(pitch <= 1f)
        {
            pitch = 1f;
        }


        if(pitch > 2f)
        {
            pitch = 2f;
        }
        return pitch;
    }

    /// <summary>
    /// AudioManager will update this float to apply any volume adjustments in settings.
    /// </summary>
    public static float mainThemeCurr = 0.5f;
    /// <summary>
    /// Stop the heartbeat sound. 
    /// This is called in convo manager bc cannot use disabled in this situation:
    /// OnDisabled only detected when this script is enabled, and that is only when
    /// we need to increase heart rate in update.
    /// </summary>
    public void StopHeartbeat()
    {
        //Tempoary fix for nullref. Convomanager setting this as it is awake first, and turns UI off on awake.
        if(S == null)
        {
            S = this;
            mainThemeCurr = mainTheme.volume; 
            return;
        }

        
        AudioManager.SmoothVolumeTransition(mainTheme.volume, mainThemeCurr);  // lower main theme volume so heartbeat better heard
        

        S.heartbeat.Stop();
    }

    // Called when parent object is disabled? Can seemly only detect if this script also enabled i.e. the behavior
    // void OnDisable()
    // {
    //     S.heartbeat.Stop();
    //     S.enabled = false;
    // }


}
