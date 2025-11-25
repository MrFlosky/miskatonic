using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGlow : MonoBehaviour
{
    Image selfImage;
    Color nextColor;
    Color currentColor;
    [SerializeField] float MaxAlpha;
    bool ascending;
    [SerializeField] float interpolationSpeed;
    float t;

    public bool active;

    void Start()
    {
        
       selfImage = gameObject.GetComponent<Image>();
       MaxAlpha = selfImage.color.a;
       nextColor= selfImage.color;
       nextColor.a= 0;
        t=0;
       selfImage.color = nextColor;  
       active=true;
       ascending= true;
       
       
    }

    void Update()
    {
        if (active)
        {
            Glow(); 
        }
    }

    public void Glow()
    {
        if(ascending)
        {
            t+= interpolationSpeed*Time.deltaTime;
            nextColor.a = Mathf.Lerp(0, MaxAlpha,t);
            selfImage.color= nextColor;
        }

        else
        {
          
            t+= interpolationSpeed*Time.deltaTime;
            nextColor.a = Mathf.Lerp(MaxAlpha, 0,t);
            selfImage.color= nextColor;
        }


        if(selfImage.color.a>= MaxAlpha)
        {
            t=0;
            ascending=false;
        }

        else if(selfImage.color.a<= 0)
        {
            t=0;
            ascending= true;
        }
    }

  

}

