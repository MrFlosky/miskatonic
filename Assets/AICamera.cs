using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DialogueEditor;
using Unity.VisualScripting;
using UnityEngine;

public class AICamera : MonoBehaviour
{
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private NPCConversation _conversation;
    [SerializeField] private GameObject _canvas;

    private bool isWaitingForBlend = false;

    private void Awake()
    {
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
            ConversationManager.Instance.StartConversation(_conversation);

            // Reset the flag to ensure it only triggers once per blend
            isWaitingForBlend = false;
        }
        
    }
    
}
