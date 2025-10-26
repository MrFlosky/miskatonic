using System.Collections;
using System.Collections.Generic;
using CharacterData;
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
    private DialogueRunner dialgoueRunner;

    /// <summary>
    /// Character's given fear, used for yarn commands
    /// </summary>
    private Fear characterFear;

    // Number of total dialogues this character has.
    private int totalDialogues;
    // Numer of dialogues this character has LEFT
    private int dialoguesRemaining;

    // MUST match the character's folder that holds their dialogues.
    [SerializeField] private CharacterData.Character character;
    private string characterFirstName;
    private string characterLastName;
    
    // Need full name to adhere to accessing each yarn script
    private string characterFullName;

    // Track when a dialogue has already been initiated in a given turn
    public bool alreadyTalked = false;

    // Character's respective sprite. HANDLED BY CHAR GAME DIALOGUE
    //[SerializeField] private Sprite characterSprite;

    //List of Fear to Reveal. NOW HANDLED BY CHARACTERFEARS
    //public GameObject[] fears;

    

    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
        dialgoueRunner = FindObjectOfType<DialogueRunner>();
        characterFear = GetComponent<Fear>();

        // sub to dialogue on/off events for essentials line presenter settings
        dialgoueRunner.onDialogueStart.AddListener(UIManagerLite.EnableDialogueSettings);
        dialgoueRunner.onDialogueComplete.AddListener(UIManagerLite.DisableDialogueSettings);

        characterFirstName = CharacterNames.GetFirstName(character);
        characterLastName = CharacterNames.GetLastName(character);
        characterFullName = characterFirstName + characterLastName;

        LoadYarnDialogues();
        dialoguesRemaining = totalDialogues;

    }

    // List of dialogue indices, corresponding to yarn scripts, that the player has yet to
    // play. Entries removed after they are used, for non-repeating dialogues
    List<int> unPlayedDialogues = new List<int>();

    /// <summary>
    /// Load respective character's yarn script and count all
    /// the dialogues within it by counting "title" within text file.
    /// </summary>
    void LoadYarnDialogues()
    {
        string characterPath = $"YarnScripts/{characterFirstName}/{characterFirstName}{characterLastName}Dialogues";
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>(characterPath);

        if (textAssets.Length > 0 && textAssets[0] != null)
        {
            string documentText = textAssets[0].text;
            int titleCount = CountDialogueNodes(documentText);
            totalDialogues = titleCount;
            Debug.Log($"'{textAssets[0].name}' has {titleCount} title occurrences");
            // setup list for non-repeating dialogues
            unPlayedDialogues.Capacity = totalDialogues;
            for (int i = 0; i < totalDialogues; i++)
            {
                unPlayedDialogues.Add(i);
            }

        }
        else
        {
            Debug.LogError($"No text asset found in path: {characterPath}");
        }
    }

    /// <summary>
    /// Play random dialogue for given character,
    /// then remove that dialogue index as a possibility.
    /// </summary>
    public void PlayRandomDialogue()
    {
        ClickAI.S.StartConversation();
        // add function to be called when dialogue ends. cannot be done in start,
        // otherwise all dialoguebank gameobjects listening for event at same time
        dialgoueRunner.onDialogueComplete.AddListener(EndDialogue);

        int listIndex = UnityEngine.Random.Range(0, unPlayedDialogues.Count);
        // access the dialogue at the random index, then play and remove
        // whatever dialogue number that returns from the list
        int dialogueKey = unPlayedDialogues[listIndex];
        string yarnAcessStr = characterFullName + dialogueKey;

        CharacterGameDialogue.SetActivePortrait(character);
        
        dialgoueRunner.StartDialogue(yarnAcessStr);

        dialoguesRemaining -= 1;
        unPlayedDialogues.RemoveAt(listIndex);
    }

    /// <summary>
    /// Calls events that must happen when dialogue ends
    /// </summary>
    private void EndDialogue()
    {
     
        dialgoueRunner.onDialogueComplete.RemoveListener(EndDialogue);

        // Events that need to happen when dialogue has ended. This
        // is a bit messy since these StartDialogue events also happen in AICamera
        ClickAI.S.EndConversation();
        Heartbeats.S.StopHeartbeat();
        AIFearMeter.HideAIMeter();
    }

    /// <summary>
    /// Called when AI has already been spoken to in given turn
    /// </summary>
    public void AlreadyTalked()
    {
        CharacterGameDialogue.SetActivePortrait(character);
        dialgoueRunner.StartDialogue("ClickedAlready");
    }

    /// <summary>
    /// Called when AI is out of new dialogues to present.
    /// </summary>
    public void OutOfDialogues()
    {
        CharacterGameDialogue.SetActivePortrait(character);
        dialgoueRunner.StartDialogue("OutOfDialogue");
    }

    
    public int GetRemainingDialogueCount()
    {
        return dialoguesRemaining;
    }


    // When play turn is initiated, re-allow interaction with this npc
    void ITurnObserver.PlayerTurnInitiated()
    {
        alreadyTalked = false;
    }
    void ITurnObserver.EnemyTurnInitiated() { }

    /// <summary>
    /// Plays characters respective key dialogue, called once they've found their key
    /// </summary>
    public void FindKey()
    {
        CharacterGameDialogue.SetActivePortrait(character);
        dialgoueRunner.StartDialogue(characterFullName + "Key");
    }


    /// <summary>
    /// Helper function to count number of dialogues in a yarn file,
    /// by counting titles.
    /// </summary>
    /// <param name="text">
    /// Entire yarn script in string format
    /// </param>
    /// <returns>
    /// Number of dialogues in yarn file
    /// </returns>
    private int CountDialogueNodes(string text)
    {
        string searchTerm = "title";
        int count = 0;
        int index = 0;

        // Count occurrences
        while ((index = text.IndexOf(searchTerm, index)) != -1)
        {
            count++;
            index += searchTerm.Length;
        }

        return count;
    }



    // Changed by balance value interface, controls
    // how much fear is added/reduced with yarn script 
    // commands. Based on assumption that these are
    // consistent across all characters and dialogues.
    public static int fearAdd;
    public static int fearReduce;
    /// <summary>
    ///  These are the functions that are used within the yarn scripts
    /// </summary>
    #region Yarn Commands
    [YarnCommand("add_fear")]
    public void YarnAddFear()
    {
        characterFear.AddFear(fearAdd);
        CultistPoints.AddPoint(); // grant cultist point on successful diagloue
    }

    [YarnCommand("reduce_fear")]
    public void YarnReduceFear()
    {
        characterFear.AddFear(fearReduce);
    }
    [YarnCommand("discover_fear")]
    public void discoverFear(int fearNum)
    {
        CharacterFears.Instance.DiscoverFear(character, fearNum);
    }
    #endregion



}


    