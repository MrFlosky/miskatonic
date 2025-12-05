using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool isPaused;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        pauseMenu.SetActive(false);
    }

    // Subscribe to event that turns this menu on when this
    // gameobject is enabled (startup initializer)
    private void OnEnable()
    {
        inputActions.UI.PauseMenu.performed += OnPauseToggle;
        inputActions.Enable();
    }


    // Unsub to event that turns this menu on when this gameobject
    // is disabled. ONLY HAVE HERE FOR CLEANUP WHEN SCENE UNLOADS
    private void OnDisable()
    {
        inputActions.UI.PauseMenu.performed -= OnPauseToggle;
        inputActions.Disable();
    }

    // actual function called on input event
    private void OnPauseToggle(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Debug.Log("The game is now paused");
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        inputActions.Player.Disable();
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        inputActions.Player.Enable();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        inputActions.Player.Enable();
        SceneManager.LoadScene("NEW_MAIN_MENU");
    }
}