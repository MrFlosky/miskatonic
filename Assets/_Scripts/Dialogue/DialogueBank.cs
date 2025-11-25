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
    [SerializeField] public CharacterData.Character character;
    private string characterFirstName;
    private string characterLastName;
    
    // Need full name to adhere to accessing each yarn script
    private string characterFullName;

    // Track when a dialogue has already been initiated in a given turn
    public bool alreadyTalked = false;

    /// <summary>
    /// For ease to access and trigger portrait fade in ClickAI. 
    /// Not used in this class for any other purpose.
    /// </summary>
    [SerializeField] private PortraitFade portraitFade;
    public PortraitFade GetPortraitFade()
    {
        return portraitFade;
    }

    // Character's respective sprite. HANDLED BY CHAR GAME DIALOGUE
    //[SerializeField] private Sprite characterSprite;

    //List of Fear to Reveal. NOW HANDLED BY CHARACTERFEARS
    //public GameObject[] fears;
    

    void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);
        var dialogueSystem = GameObject.Find("Dialogue System"); // gets gameobject ("Dialogue System") MESSAGE FOR ANYONE ON CODE TEAM - You have permission to change this from finding a gameobject -Alex 
        if (dialogueSystem == null) Debug.LogError("No dialogue system found! Make sure there is a dialogue runner called 'Dialogue System' in the scene - MUST MATCH EXACT SPELLING!!!!!! Did you change the name of 'Dialogue System' in the scene?");
        dialgoueRunner = GameObject.Find("Dialogue System").GetComponent<DialogueRunner>(); // gets gameobject ("Dialogue System")
        characterFear = GetComponent<Fear>();

        // sub to dialogue on/off events for essentials line presenter settings
        dialgoueRunner.onDialogueStart.AddListener(UIManagerLite.EnableDialogueSettings);
        dialgoueRunner.onDialogueComplete.AddListener(UIManagerLite.DisableDialogueSettings);

        characterFirstName = CharacterNames.GetFirstName(character);
        characterLastName = CharacterNames.GetLastName(character);
        characterFullName = characterFirstName + characterLastName;

        // If we are loading from a save, get the corresponding dialoguesRemaining and unplayed dialogues
        if (LoadManager.S.loadData)
        {
            PlayerData pd = LoadManager.S.playerDataToLoad;
            switch (character)
            {
                case Character.HaroldAuden:
                    dialoguesRemaining = pd.haroldDialogueRemainingCount;
                    unPlayedDialogues = pd.haroldRemainingDialogues;
                    break;
                case Character.IainSchweblin:
                    dialoguesRemaining = pd.iainDialogueRemainingCount;
                    unPlayedDialogues = pd.iainRemainingDialogues;
                    break;
                case Character.MackenzieJoyce:
                    dialoguesRemaining = pd.mackenzieDialogueRemainingCount;
                    unPlayedDialogues = pd.mackenzieRemainingDialogues;
                    break;
                case Character.EvelynVentress:
                    dialoguesRemaining = pd.evelynDialogueRemainingCount;
                    unPlayedDialogues = pd.evelynRemainingDialogues;
                    break;
                default:
                    break;
            }
        }
        else
        {
            // Otherwise instantiate these variables normally
            LoadYarnDialogues();
            dialoguesRemaining = totalDialogues;
        }
        

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

    public enum DialogueType
    {
        Room,
        State,
        Encounter,
        None
    }

    private (DialogueType type, string text) dialog = (DialogueType.None, "Null");

    /// <summary>
    /// Queue up special dialog when a requirement is met. 
    /// (Techincally with the current game design a queue is not needed as we don't want to actually store up dialogs but functionality is here if you just move some things around)
    /// </summary>
    public void QueueSpecialDialog(string trigger, DialogueType dialogueType)
    {
        characterFirstName = CharacterNames.GetFirstName(character);
        characterLastName = CharacterNames.GetLastName(character);
        characterFullName = characterFirstName + characterLastName;
        string yarnAcessStr = characterFullName + trigger;
        Debug.Log($"Dialog {yarnAcessStr} queued for {characterFirstName}");

        switch(dialog.type)
        {
            case DialogueType.Room:
                if(dialogueType != DialogueType.Room) return;
                break;
            case DialogueType.State:
                if(dialogueType == DialogueType.Encounter) return;
                break;
        }
        dialog = (dialogueType, yarnAcessStr);

    }

    public void DequeueSpecialDialog(DialogueType dialogueType)
    {
        if(dialogueType != dialog.type) return;
        dialog = (DialogueType.None, "Null");
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

        CharacterGameDialogue.SetActivePortrait(character, characterFear.GetFearState());
        
        dialgoueRunner.StartDialogue(yarnAcessStr);

        dialoguesRemaining -= 1;
        unPlayedDialogues.RemoveAt(listIndex);
    }

    /// <summary>
    /// Play a base dialog given an index
    /// then remove that dialogue index as a possibility.
    /// </summary>
    public void PlayDialogAtIndex(int index)
    {
        if(index > unPlayedDialogues.Count)
        {
            Debug.LogWarning("dialog index provided is outside of index range of available dialogs", this);
            return;
        }
        ClickAI.S.StartConversation();
        // add function to be called when dialogue ends. cannot be done in start,
        // otherwise all dialoguebank gameobjects listening for event at same time
        dialgoueRunner.onDialogueComplete.AddListener(EndDialogue);

        int dialogueKey = unPlayedDialogues[index];
        string yarnAcessStr = characterFullName + dialogueKey;

        CharacterGameDialogue.SetActivePortrait(character);
        
        dialgoueRunner.StartDialogue(yarnAcessStr);

        dialoguesRemaining -= 1;
        unPlayedDialogues.RemoveAt(index);        
    }

    public void PlayQueuedDialog(string yarnAcessStr)
    {
        ClickAI.S.StartConversation();
        // add function to be called when dialogue ends. cannot be done in start,
        // otherwise all dialoguebank gameobjects listening for event at same time
        dialgoueRunner.onDialogueComplete.AddListener(EndDialogue);

        CharacterGameDialogue.SetActivePortrait(character, characterFear.GetFearState());
        
        dialgoueRunner.StartDialogue(yarnAcessStr);    
    }

    bool firstDialogPlayed = false;

    /// <summary>
    /// Plays and handles dialog
    /// </summary>
    public void PlayDialog()
    {

        // Terrible dummy way to handle the first dialog being the zero index
        if(!firstDialogPlayed)
        {
            firstDialogPlayed = true;
            if(TurnBasedSystem.GET_TURN_COUNT() > 0)
            {
                if(dialog.type != DialogueType.None)
                {
                    PlayQueuedDialog(dialog.text);
                }
                else
                {
                    PlayRandomDialogue();
                }
                return;
            }
            PlayDialogAtIndex(0);

            return;
        }
        if(dialog.type != DialogueType.None)
        {
            PlayQueuedDialog(dialog.text);
        }
        else
        {
            PlayRandomDialogue();
        }
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
        CharacterGameDialogue.SetActivePortrait(character, characterFear.GetFearState());
        dialgoueRunner.StartDialogue("ClickedAlready");
    }

    /// <summary>
    /// Called when AI is out of new dialogues to present.
    /// </summary>
    public void OutOfDialogues()
    {
        CharacterGameDialogue.SetActivePortrait(character, characterFear.GetFearState());
        dialgoueRunner.StartDialogue("OutOfDialogue");
    }


    public int GetRemainingDialogueCount()
    {
        return dialoguesRemaining;
    }
    
    public List<int> GetRemainingDialogues()
    {
        return unPlayedDialogues;
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
        CharacterGameDialogue.SetActivePortrait(character, FearState.Determined);
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

    public Character GetCharacter()
    {
        return character;
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

    [YarnCommand("neutral")]
    public void YarnNeutralFS()
    {
        characterFear.SetFearState(FearState.Neutral);
        CharacterGameDialogue.SetActivePortrait(character, FearState.Neutral);
    }

    [YarnCommand("determined")]
    public void YarnDeterminedFS()
    {
        characterFear.SetFearState(FearState.Determined);
        CharacterGameDialogue.SetActivePortrait(character, FearState.Determined);

    }

    [YarnCommand("scared")]
    public void YarnScaredFS()
    {
        characterFear.SetFearState(FearState.Scared);
        CharacterGameDialogue.SetActivePortrait(character, FearState.Scared);

    }

    [YarnCommand("terrified")]
    public void YarnTerrifiedFS()
    {
        characterFear.SetFearState(FearState.Terrified);
        CharacterGameDialogue.SetActivePortrait(character, FearState.Terrified);

    }
    
    [YarnCommand("tweaking")]
    public void YarnTweakingFS()
    {
        characterFear.SetFearState(FearState.Tweaking);
        CharacterGameDialogue.SetActivePortrait(character, FearState.Scared); // no unique portrait for tweaking

    }
    #endregion



}


    