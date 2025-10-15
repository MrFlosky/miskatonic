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
    [SerializeField] private float flipSteps = 60;
    [SerializeField] private float flipDuration = 2;
    private Quaternion CharacterFacingRotation;
    private bool didRotate = false;
    private Quaternion initialRotation;
    private Vector3 relativePos;
    private DialogueBank dialogueBank;
    private ICinemachineCamera mainCamera;
    
    


    /// <summary>
    /// AI Camera is responsible for setting the AI's fear
    /// slider with the AI's current fear upon starting a convo.
    /// </summary>
    private Fear thisAIsFear;

    private bool isWaitingForBlend = false;

    private void Awake()
    {
        dialogueBank = transform.parent.GetComponent<DialogueBank>();
        thisAIsFear = transform.parent.GetComponent<Fear>();
        if (Camera.main != null) brain = Camera.main.GetComponent<CinemachineBrain>();
        mainCamera = brain.ActiveVirtualCamera;
    }

    private void Update()
    {       
;
        // Start checking for blend completion only if the blend is in progress
        if (!isWaitingForBlend && brain.IsBlending)
        {
            isWaitingForBlend = true; // Start tracking the blend
            
        }       

        // After blending is done, enable the ai's fear meter and start the dialogue
        if (isWaitingForBlend && !brain.IsBlending)
        {
            //rotate the character bust            
            if (!didRotate)
            {
                StartCoroutine(RotateBust());
                didRotate = true;
            } else
            {
                CharacterBust.transform.rotation = initialRotation;
            }


            AIFearMeter.StartUpSlider(thisAIsFear.GetFear(), thisAIsFear.GetFearThreshold());
            AIFearMeter.DisplayAIMeter();
            
            dialogueBank.PlayRandomDialogue();

            // Reset the flag to ensure it only triggers once per blend
            isWaitingForBlend = false;
        }
    }

    IEnumerator RotateBust()
    {
        //Finding Rotation of Bust
        relativePos = this.transform.position - CharacterBust.transform.position;
        Quaternion CharacterFacingRotation = Quaternion.LookRotation(relativePos, Vector3.up);

        initialRotation = CharacterBust.transform.rotation;

        for (int i = 0; i <= flipSteps; i++)
        {
            CharacterBust.transform.rotation = Quaternion.Slerp(initialRotation, CharacterFacingRotation, (i / flipSteps));
            yield return new WaitForSeconds(flipDuration / flipSteps);
        }
        yield return new WaitForSeconds(2);
        for (int i = 0; i <= flipSteps; i++)
        {
            CharacterBust.transform.rotation = Quaternion.Slerp(CharacterFacingRotation, initialRotation, (i / flipSteps));
            yield return new WaitForSeconds(flipDuration / flipSteps);
        }
    }
}
