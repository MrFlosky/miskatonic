using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PortraitFade : MonoBehaviour, ITurnObserver
{
    /// <summary>
    /// This script contains functions for fading portraits in and out. Also disables/enables the text of the portrait button and the button itself
    /// </summary>


    public Button PortraitButton; // for setting the buttons inactive on click and vice versa
    [SerializeField]
    Image Portrait;
    [SerializeField] Color portraitColor;
    [SerializeField] Color defaultColor;
    float MaxAlpha =1.0f;
    
    [SerializeField]
    float interpolationSpeed;
    [SerializeField] GameObject buttonText;
    public bool clicked;
    float maxClickedAlpha = .1f;

    
    Image portraitImage;

    




    private void Start()
    {
        
        portraitImage = gameObject.GetComponent<Image>();
       
        TurnBasedSystem.SubscribeAsObserver(this);
       
        portraitColor = Portrait.color;


        if (Portrait == null)
        {
            Debug.LogError(gameObject.name + " does not have a portrait assigned! Assign the characters portrait image.");
        }


    }

   

    //used when returning from storyboard to Boardbase
    public IEnumerator BoardBaseReset()
    {
        float t = 0;
        if (clicked)
        {
            while (Portrait.color.a > maxClickedAlpha)
            {
                float alpha = portraitColor.a;
                alpha = Mathf.Lerp(MaxAlpha, 0, t);
                t += interpolationSpeed * Time.deltaTime;


                portraitColor.a = alpha;
                Portrait.color = portraitColor;
                yield return new WaitForEndOfFrame();

            }
        }

        else
        {
           
            PortraitButton.enabled = true;
        }
        buttonText.GetComponent<TextMeshProUGUI>().enabled = true;
          
    }

    //fully fades out a portrait
    public IEnumerator FadeOut()
    {
        float startingAlpha = Portrait.color.a;
        buttonText.GetComponent<TextMeshProUGUI>().enabled = false;
        PortraitButton.enabled = false;
        
        float t = 0;
        portraitImage.color = PortraitButton.colors.pressedColor;
        while (Portrait.color.a > 0)
        {
            float alpha = portraitColor.a;
            alpha = Mathf.Lerp(startingAlpha, 0, t);
            t += interpolationSpeed * Time.deltaTime;


            portraitColor.a = alpha;
            Portrait.color = portraitColor;
//            print(Portrait.color.a);
            PortraitButton.interactable = false;
           // ButtonText.gameObject.SetActive(false);




            yield return new WaitForEndOfFrame();
        }

        

    }

    //used at the start of the player turn to reset all portraits 
    public IEnumerator TurnResetFadeIn()
    {
        float t = 0;
        
        while (Portrait.color.a < MaxAlpha)
        {

            float alpha = portraitColor.a;
            alpha = Mathf.Lerp(0, MaxAlpha, t);
            t += interpolationSpeed * Time.deltaTime;


            portraitColor.a = alpha;
            Portrait.color = portraitColor;




            yield return new WaitForEndOfFrame();
        }
        buttonText.GetComponent<TextMeshProUGUI>().enabled = true;
        PortraitButton.enabled = true;

    }

    //fades in portraits, if a dialogue has already been initiated for this turn portraits are still partially faded and buttons are disabled
    public IEnumerator FadeIn()
    {

        float t = 0;

        if (!clicked || UIManagerLite.GetCurrentState() != UIManagerLite.GameState.BoardBase)
        {
            while (Portrait.color.a < MaxAlpha)
            {

                float alpha = portraitColor.a;
                alpha = Mathf.Lerp(Portrait.color.a, MaxAlpha, t);
                t += interpolationSpeed * Time.deltaTime;


                portraitColor.a = alpha;
                Portrait.color = portraitColor;
                yield return new WaitForEndOfFrame();
            }
            PortraitButton.enabled = true;
        }

        else
        {


            while (Portrait.color.a < maxClickedAlpha)
            {

                float alpha = portraitColor.a;
                alpha = Mathf.Lerp(Portrait.color.a, maxClickedAlpha, t);
                t += interpolationSpeed * Time.deltaTime;


                portraitColor.a = alpha;
                Portrait.color = portraitColor;
                yield return new WaitForEndOfFrame();
            }
        }

        buttonText.GetComponent<TextMeshProUGUI>().enabled = true;

    }
    
    void ITurnObserver.PlayerTurnInitiated()
    {
        StartCoroutine(TurnResetFadeIn());
        PortraitButton.enabled = true;
        clicked = false;
    }

    
    void ITurnObserver.EnemyTurnInitiated()
    {
        StartCoroutine(FadeOut());
        PortraitButton.enabled = false;

    }

}
 