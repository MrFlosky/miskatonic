using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using CharacterData;
using UnityEditor.PackageManager;

/// <summary>
/// PLEASE ADD COMMENTS TO WHAT THIS SCRIPT DOES!!!- jay
/// </summary>
public class CharacterGameDialogue : MonoBehaviour
{
    private static Sprite activeCharacterSprite;
    public Image CharacterSpriteImage;
    public Animator DialogueAnimator;

    // Dictionary that contains all character sprites, one per character.
    // Key is character, value is potrait sprite.
    // Emotion is ambigous for now.
    static private Dictionary<Character, Sprite> allPortraits = new Dictionary<Character, Sprite>();

    // Start is called before the first frame update
    void Start()
    {
        allPortraits = CharacterNames.GetAllPortraits();
        Debug.LogWarning(allPortraits.Count);
        DialogueAnimator.SetBool("active", false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Called by DialogueBank and NarrativeReward to 
    // set given character's portrait
    static public void SetActivePortrait(Character chara)
    {
        activeCharacterSprite = allPortraits[chara];
    }

    public void OnDialogueStart()
    {
        DialogueAnimator.SetBool("active", true);

        CharacterSpriteImage.sprite = activeCharacterSprite;
    }
    public void OnDialogueEnd()
    {
        DialogueAnimator.SetBool("active", false);
    }
}
