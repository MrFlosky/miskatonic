using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn;
using Yarn.Unity;

/// <summary>
/// This class will be responsible for accessing the respective dialogues for each character.
/// It automatically detects how many dialogues a given character has based on a strict file structure.
/// </summary>
public class DialogueBank : MonoBehaviour, ITurnObserver
{
    /// <summary>
    /// The main way to manipulate the state of dialog.
    /// </summary>
    DialogueRunner DR;

    // Number of total dialogues this character has.
    private int totalDialogues;
    // Numer of dialogues this character has LEFT
    private int dialoguesRemaining;

    // MUST match the character's folder that holds their dialogues.
    [SerializeField] private string characterFirstName;
    [SerializeField] private string characterLastName;
    // Need full name to adhere to accessing each yarn script
    private string characterFullName;

    // Track when a dialogue has already been initiated in a given turn
    public bool alreadyTalked = false;

    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);

        DR = FindObjectOfType<DialogueRunner>();
        characterFullName = characterFirstName + characterLastName;

        LoadYarnDialogues();
        dialoguesRemaining = totalDialogues;

        

    }

    /// <summary>
    /// Loads respective character's dialogues from resource folder and stores them
    /// in a dictionary.
    /// </summary>
    void LoadYarnDialogues()
    {
        string characterPath = $"YarnScripts/{characterFirstName}";
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>(characterPath);

        Debug.Log($"Found {textAssets.Length} text files for {characterFirstName}");
    }

    public void PlayRandomDialogue()
    {
        // need do smth when out of dialogues
        int dialogueKey = UnityEngine.Random.Range(0, dialoguesRemaining - 1);
        string yarnAcessStr = characterFullName + dialogueKey;
        DR.StartDialogue(yarnAcessStr);

    }

    // When play turn is initiated, re-allow interaction with this npc
    void ITurnObserver.PlayerTurnInitiated()
    {
        alreadyTalked = false;
    }
    void ITurnObserver.EnemyTurnInitiated(){}




}
