using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// script makes buttons show a menu and hide a menu at the same time - This was helped created with Claude.AI
public class Buttons : MonoBehaviour
{

    public GameObject MenuObject;
    [SerializeField] ButtonGlow buttonGlow;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(ToggleMenu);
    }

    void ToggleMenu()
    {
        MenuObject.SetActive(!MenuObject.activeSelf);
        if(buttonGlow!=null) buttonGlow.EndGlow();

    }
    public void ToggleMenuActive()
    {
        MenuObject.SetActive(!MenuObject.activeSelf);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }
}