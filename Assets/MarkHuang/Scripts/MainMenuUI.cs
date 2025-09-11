using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuUI : MonoBehaviour
{
    
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions(); // Initialize Input Actions
    }

    private void OnEnable()
    {
        inputActions.Enable(); // Enable Input Actions
        inputActions.Player.AnyKey.performed += OnAnyKey; // Subscribe to any key press
    }

    private void OnDisable()
    {
        inputActions.Player.AnyKey.performed -= OnAnyKey; // Unsubscribe on disable
        inputActions.Disable();
    }

    private void OnAnyKey(InputAction.CallbackContext context)
    {
        SceneController.Instance.LoadScene("Idol_Test");
    }
}
