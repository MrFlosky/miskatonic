using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterGameDialogue : MonoBehaviour
{
    public static Sprite CharacterSprite;
    public Image CharacterSpriteImage;
    public Animator DialogueAnimator;

    // Start is called before the first frame update
    void Start()
    {
        DialogueAnimator.SetBool("active", false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnDialogueStart()
    {
        DialogueAnimator.SetBool("active", true);

        CharacterSpriteImage.sprite = CharacterSprite;
    }
    public void OnDialogueEnd()
    {
        DialogueAnimator.SetBool("active", false);   
    }
}
