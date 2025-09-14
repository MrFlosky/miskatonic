using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitFade : MonoBehaviour, ITurnObserver
{
    ///This script fades out a characters portrait after being clicked and fades in when the turn refreshes

    [SerializeField]
    Image Portrait;
    Color portraitColor;
    float MaxAlpha =1.0f;
    float MinAlpha = .1f;

    [SerializeField]
    float interpolationSpeed;

    

    private void Start()
    {
        TurnBasedSystem.SubscribeAsObserver(this);

        portraitColor = Portrait.color; 


        if (Portrait == null)
        {
            Debug.LogError(gameObject.name + " does not have a portrait assigned! Assign the characters portrait image.");
        }
       
        
    }


    //Uses Lerp to lower alpha
   public IEnumerator FadeOut()
    {
        float t = 0;
        while(Portrait.color.a > MinAlpha)
        {
            float alpha = portraitColor.a;
            alpha = Mathf.Lerp(MaxAlpha, MinAlpha, t);
            t += interpolationSpeed * Time.deltaTime;


            portraitColor.a = alpha;
            Portrait.color = portraitColor;
            print(Portrait.color.a);




            yield return new WaitForEndOfFrame();
        }


        
    }

    //Uses Lerp to raise alpha
    public IEnumerator FadeIn()
    {
        float t = 0;
        while (Portrait.color.a < MaxAlpha)
        {
            float alpha = portraitColor.a;
            alpha = Mathf.Lerp(MinAlpha, MaxAlpha, t);
            t += interpolationSpeed * Time.deltaTime;


            portraitColor.a = alpha;
            Portrait.color = portraitColor;
            print(Portrait.color.a);



            yield return new WaitForEndOfFrame();
        }


    }
    
    void ITurnObserver.PlayerTurnInitiated()
    {
        StartCoroutine(FadeIn());
    }

    
    void ITurnObserver.EnemyTurnInitiated() {}

}
