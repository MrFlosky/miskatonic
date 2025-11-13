using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Spotlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Light spotlight;
    private void Start()
    {
        if (spotlight != null)
        {
            spotlight.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spotlight != null)
        {
        spotlight.enabled = true;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (spotlight != null)
        {
        spotlight.enabled = false;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (spotlight != null)
        {
            spotlight.enabled = false;
        }
    }


}
