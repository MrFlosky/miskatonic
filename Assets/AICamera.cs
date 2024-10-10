using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DialogueEditor;
using UnityEngine;

public class AICamera : MonoBehaviour
{
    private CinemachineBrain brain;
    private void Awake()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (brain.IsBlending)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false); // Disable each child GameObject
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true); // enable each child GameObject
                if (ConversationManager.Instance.IsConversationActive == false)
                {
                    ConversationManager.Instance.StartConversation();
                }
            }
        }
    }
}
