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
    [SerializeField] private GameObject _canvas;
    [SerializeField] private GameObject CharacterBust;
    [SerializeField] private float flipDuration = 2f;
    
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    
    private bool isRotating = false;
    private bool isRotatingToTarget = true;
    private float rotationTimer = 0f;
    
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
        
        // Store initial rotation on awake
        if (CharacterBust != null)
        {
            initialRotation = CharacterBust.transform.rotation;
        }
    }

    private void Update()
    {
        // Start checking for blend completion only if the blend is in progress
        if (!isWaitingForBlend && brain.IsBlending)
        {
            isWaitingForBlend = true; // Start tracking the blend
        }

        // After blending is done, enable the ai's fear meter and start the dialogue
        if (isWaitingForBlend && !brain.IsBlending)
        {
            AIFearMeter.StartUpSlider(thisAIsFear.GetFear(), thisAIsFear.GetFearThreshold());
            AIFearMeter.DisplayAIMeter();
            
            dialogueBank.PlayRandomDialogue();

            // Reset the flag to ensure it only triggers once per blend
            isWaitingForBlend = false;
        }

        if (isRotating)
        {
            HandleRotation();
        }
    }

    /// <summary>
    /// Call this to start rotating the character bust towards the camera
    /// </summary>
    public void RotBustToCam()
    {
        if (CharacterBust == null) return;
        
        // Calculate target rotation
        Vector3 relativePos = this.transform.position - CharacterBust.transform.position;
        targetRotation = Quaternion.LookRotation(relativePos, Vector3.up);
        
        isRotating = true;
        isRotatingToTarget = true;
        rotationTimer = 0f;
    }

    /// <summary>
    /// Call this to reset the character bust back to its initial rotation
    /// </summary>
    public void ResetBustRotation()
    {

        if (CharacterBust == null) return;
        
        isRotating = true;
        isRotatingToTarget = false;
        rotationTimer = 0f;
    }

    private void HandleRotation()
    {
        Debug.LogWarning("HANDLING ROT");
        rotationTimer += Time.deltaTime;
        float t = Mathf.Clamp01(rotationTimer / flipDuration);
        
        if (isRotatingToTarget)
        {
            CharacterBust.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
        }
        else
        {
            CharacterBust.transform.rotation = Quaternion.Slerp(targetRotation, initialRotation, t);
        }
        
        if (t >= 1f)
        {
            isRotating = false;
        }
    }
}