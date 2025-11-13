using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PortraitFade : MonoBehaviour, ITurnObserver
{
    ///This script fades out a characters portrait after being clicked and fades in when the turn refreshes


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

    //this is used to fade in/out all portraits at once regardless of which is clicked on
    public static List<PortraitFade> AllPortraits = new List<PortraitFade>();




    private void Start()
    {
        
        portraitImage = gameObject.GetComponent<Image>();
        AllPortraits.Add(this);
        TurnBasedSystem.SubscribeAsObserver(this);
       
        portraitColor = Portrait.color;


        if (Portrait == null)
        {
            Debug.LogError(gameObject.name + " does not have a portrait assigned! Assign the characters portrait image.");
        }


    }

    void Update()
    {
    }


    //Uses Lerp to lower alpha
    public IEnumerator FadeOut()
    {
  
       
        foreach(var B in AllPortraits)
        {
            B.buttonText.SetActive(true);
            B.buttonText.GetComponent<TextMeshProUGUI>().enabled = false;
            B.buttonText.SetActive(false);
            B.PortraitButton.enabled = false;
        }
        float t = 0;
        portraitImage.color = PortraitButton.colors.pressedColor;
        while(Portrait.color.a > 0)
        {
            float alpha = portraitColor.a;
            alpha = Mathf.Lerp(MaxAlpha, 0, t);
            t += interpolationSpeed * Time.deltaTime;


            portraitColor.a = alpha;
            Portrait.color = portraitColor;
//            print(Portrait.color.a);
            PortraitButton.interactable = false;
           // ButtonText.gameObject.SetActive(false);




            yield return new WaitForEndOfFrame();
        }

        
        
    }

    //Uses Lerp to raise alpha
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


    }

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
            PortraitButton.enabled=true;
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
    }
    
    void ITurnObserver.PlayerTurnInitiated()
    {
        StartCoroutine(TurnResetFadeIn());
        PortraitButton.enabled = true;
    }

    
    void ITurnObserver.EnemyTurnInitiated()
    {
        portraitColor.a = 0;
        Portrait.color = portraitColor;
        PortraitButton.enabled = false;

    }

}
 