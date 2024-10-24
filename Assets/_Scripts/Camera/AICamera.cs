using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DialogueEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AICamera : MonoBehaviour
{
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private NPCConversation _conversation;
    [SerializeField] private GameObject _canvas;
    /// <summary>
    /// AI Camera is responsible for setting the AI's fear
    /// slider with the AI's current fear upon starting a convo.
    /// </summary>
    private Fear thisAIsFear;

    private bool isWaitingForBlend = false;

    private void Awake()
    {
        thisAIsFear = transform.parent.GetComponent<Fear>();
        if (Camera.main != null) brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void Update()
    {
        // Start checking for blend completion only if the blend is in progress
        if (!isWaitingForBlend && brain.IsBlending)
        {
            isWaitingForBlend = true; // Start tracking the blend
        }

        // After blending is done, enable the canvas and start the dialogue
        if (isWaitingForBlend && !brain.IsBlending)
        {
            AIFearMeter.StartUpSlider(thisAIsFear.GetFear()); 
            ConversationManager.Instance.StartConversation(_conversation);

            // Reset the flag to ensure it only triggers once per blend
            isWaitingForBlend = false;

        }
        
    }
    
}
