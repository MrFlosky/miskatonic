using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// this script makes the spotlight affect work while hovering on the buttons
public class Spotlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Light spotlight;
    public GameObject vfx;
    private void Start()
    {
        if (spotlight != null)
        {
            spotlight.enabled = false;
        }
        if (vfx != null)
            vfx.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spotlight != null)
        {
        spotlight.enabled = true;
        }
        if (vfx != null)
            vfx.SetActive(true);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (spotlight != null)
        {
        spotlight.enabled = false;
        }
        if(vfx != null)
            vfx.SetActive(false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (spotlight != null)
        {
            spotlight.enabled = false;
        }
        if (vfx != null)
            vfx.SetActive(false);
    }


}
