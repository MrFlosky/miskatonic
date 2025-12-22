using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using CharacterData;


/// <summary>
/// PLEASE ADD COMMENTS TO WHAT THIS SCRIPT DOES!!!- jay
/// This class is responsible for setting a character sprites across the game, including
/// in dialogues and on the portrait buttons. Its attached to both dialogue runners, normal dialogue and
/// narrative clues.
/// </summary>
public class CharacterGameDialogue : MonoBehaviour
{
    static public CharacterGameDialogue S;

    /// <summary>
    /// Currently set dialogue sprite.
    /// </summary>
    private Sprite activeCharacterSprite;
    [SerializeField] private Image actualCharacterImage; // inpsector ref

    /// <summary>
    /// Portrait buttons used to initiate dialogues.
    /// </summary>
    [SerializeField] private Image[] portraitButtonImages;

    /// <summary>
    /// Dict storing refs to all portrait button images with character as key.
    /// </summary>
    private Dictionary<Character, Image> buttonPortraitDict;


    /// <summary>
    /// Animator associated with dialogues? not sure
    /// </summary>
    public Animator DialogueAnimator;

    // Dictionary that contains all character sprites, one per character.
    // Key is character, value is potrait sprite.
    // Emotion is ambigous for now.
    private Dictionary<Character, Sprite> allPortraits;

    /// <summary>
    /// Dictionary that contains all character emotion sprites
    /// </summary>
    private Dictionary<Character, Dictionary<FearState, Sprite>> allEmotionPortraits;

    void Awake()
    {
        if(S != null && S != this)
        {
            Destroy(gameObject);
            return;
        }

        S = this;

        // this will only work if character enum is aligned with array element order assigned in inspector
        // for characters
        buttonPortraitDict = new Dictionary<Character, Image>();
        for(int i = 0; i < portraitButtonImages.Length; i ++)
        {
            buttonPortraitDict[(Character)i] = portraitButtonImages[i];
        }
    }

    // clear static ref on scene unload
    void OnDestroy()
    {
        if(S == this)
        {
            S = null;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        allPortraits = CharacterNames.GetAllPortraits();
        allEmotionPortraits = CharacterNames.GetAllEmotionPortraits();


        // Debug that we got all emotion portraits
        // foreach(var pair in allEmotionPortraits)
        // {
        //     Character character = pair.Key;
        //     foreach(var innerPair in pair.Value) 
        //     {
        //         FearState fearState = innerPair.Key;
        //         Sprite portrait = innerPair.Value;
                
        //         Debug.Log($"{character} - {fearState}: {portrait.name}");
        //     }
        // }

        DialogueAnimator.SetBool("active", false); // idk if this doing anything, ask Alex - jay
    }


    // Called by DialogueBank and NarrativeReward to 
    // set given character's portrait. Not in use!
    static public void SetActivePortrait(Character chara)
    {
        S.activeCharacterSprite = S.allPortraits[chara];

        S.actualCharacterImage.sprite = S.activeCharacterSprite;
        
    }

    /// <summary>
    /// Overload to set character's portrait with a corresponding emotion
    /// </summary>
    static public void SetActivePortrait(Character chara, FearState state)
    {
        S.activeCharacterSprite = S.allEmotionPortraits[chara][state];
        S.actualCharacterImage.sprite = S.activeCharacterSprite; // set for dialogue portraits
        S.buttonPortraitDict[chara].sprite = S.activeCharacterSprite; // set for portrait buttons
        
        //Debug.LogWarning(activeCharacterSprite.name + " SET !!");
    }
    
    /// <summary>
    /// These apparently are being used- jay
    /// </summary>
    public void OnDialogueStart()
    {
        DialogueAnimator.SetBool("active", true);
    }
    public void OnDialogueEnd()
    {
        DialogueAnimator.SetBool("active", false);
    }

    
}
