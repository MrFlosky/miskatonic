using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterProfiles : MonoBehaviour
    // based on AI response
{
    [SerializeField] private GameObject menuObject;
    private bool isMenuOpen = false;

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            isMenuOpen = !isMenuOpen;
            menuObject.SetActive(isMenuOpen);
        }
    }
}