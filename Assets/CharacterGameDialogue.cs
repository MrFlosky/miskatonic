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
    /// <summary>
    /// Currently set dialogue sprite.
    /// </summary>
    private static Sprite activeCharacterSprite;
    [SerializeField] private Image CharacterSpriteImage; // inpsector ref
    static public Image characterSpriteImageStatic; // the above, for use in static methods

    /// <summary>
    /// Portrait buttons used to initiate dialogues.
    /// </summary>
    [SerializeField] private Image[] portraitButtonImages;

    /// <summary>
    /// Dict storing refs to all portrait button images with character as key.
    /// </summary>
    private static Dictionary<Character, Image> buttonPortraitDict;


    /// <summary>
    /// Animator associated with dialogues? not sure
    /// </summary>
    public Animator DialogueAnimator;

    // Dictionary that contains all character sprites, one per character.
    // Key is character, value is potrait sprite.
    // Emotion is ambigous for now.
    static private Dictionary<Character, Sprite> allPortraits;

    /// <summary>
    /// Dictionary that contains all character emotion sprites
    /// </summary>
    static private Dictionary<Character, Dictionary<FearState, Sprite>> allEmotionPortraits;

    void Awake()
    {
        // set static image if not already set. have to do this bc this script attached to two runners
        if(characterSpriteImageStatic == null)
        {
            characterSpriteImageStatic = CharacterSpriteImage;
        }

        if(buttonPortraitDict == null && portraitButtonImages.Length != 0)
        {
            // this will only work if character enum is aligned with array element order assigned in inspector
            // for characters
            buttonPortraitDict = new Dictionary<Character, Image>();
            for(int i = 0; i < portraitButtonImages.Length; i ++)
            {
                buttonPortraitDict[(Character)i] = portraitButtonImages[i];
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        allPortraits = CharacterNames.GetAllPortraits();
        allEmotionPortraits = CharacterNames.GetAllEmotionPortraits();

        foreach(var pair in allEmotionPortraits)
        {
            Character character = pair.Key;
            foreach(var innerPair in pair.Value) 
            {
                FearState fearState = innerPair.Key;
                Sprite portrait = innerPair.Value;
                
//                Debug.Log($"{character} - {fearState}: {portrait.name}");
            }
        }

        DialogueAnimator.SetBool("active", false); // idk if this doing anything, ask Alex - jay
    }


    // Called by DialogueBank and NarrativeReward to 
    // set given character's portrait. Not in use!
    static public void SetActivePortrait(Character chara)
    {
        activeCharacterSprite = allPortraits[chara];
         characterSpriteImageStatic.sprite = activeCharacterSprite;
    }

    /// <summary>
    /// Overload to set character's portrait with a corresponding emotion
    /// </summary>
    static public void SetActivePortrait(Character chara, FearState state)
    {
        activeCharacterSprite = allEmotionPortraits[chara][state];
        characterSpriteImageStatic.sprite = activeCharacterSprite; // set for dialogue portraits
        buttonPortraitDict[chara].sprite = activeCharacterSprite; // set for portrait buttons
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
