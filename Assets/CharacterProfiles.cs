using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterProfiles : MonoBehaviour
{
    [SerializeField] private GameObject menuObject;
    private bool isMenuOpen = false;

    // Makes this class a listener to our Input Actions settings
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }


    // actual event called when respective input is pressed
    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        isMenuOpen = !isMenuOpen; // flip bool on every press
        menuObject.SetActive(isMenuOpen);
    }

    // Subscribe to event that turns this menu on when this
    // gameobject is enabled (startup initializer)
    private void OnEnable()
    {
        inputActions.UI.CharacterProfilesMenu.performed += OnToggleMenu;
        inputActions.Enable(); // enable action map
    }

    // Unsub to event that turns this menu on when this gameobject
    // is disabled. ONLY HAVE HERE FOR CLEANUP WHEN SCENE UNLOADS
    private void OnDisable()
    {
        inputActions.UI.CharacterProfilesMenu.performed -= OnToggleMenu;
        inputActions.Disable(); // disable action map
    }

}