using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using CharacterData;
using UnityEditor.PackageManager;

/// <summary>
/// PLEASE ADD COMMENTS TO WHAT THIS SCRIPT DOES!!!- jay
/// This is added to both dialogue runners in game.
/// </summary>
public class CharacterGameDialogue : MonoBehaviour
{
    private static Sprite activeCharacterSprite;
    public Image CharacterSpriteImage;
    static public Image characterSpriteImageStatic;
    public Animator DialogueAnimator;

    // Dictionary that contains all character sprites, one per character.
    // Key is character, value is potrait sprite.
    // Emotion is ambigous for now.
    static private Dictionary<Character, Sprite> allPortraits = new Dictionary<Character, Sprite>();

    /// <summary>
    /// Dictionary that contains all character emotion sprites
    /// </summary>
    static private Dictionary<Character, Dictionary<FearState, Sprite>> allEmotionPortraits = new Dictionary<Character, Dictionary<FearState, Sprite>>();

    void Awake()
    {
        // set static image if not already set
        if(characterSpriteImageStatic == null)
        {
            characterSpriteImageStatic = CharacterSpriteImage;
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
                
                Debug.Log($"{character} - {fearState}: {portrait.name}");
            }
        }

        DialogueAnimator.SetBool("active", false); // idk if this doing anything, ask Alex - jay
    }


    // Called by DialogueBank and NarrativeReward to 
    // set given character's portrait
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
        characterSpriteImageStatic.sprite = activeCharacterSprite;
        Debug.LogWarning(activeCharacterSprite.name + " SET !!");
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
