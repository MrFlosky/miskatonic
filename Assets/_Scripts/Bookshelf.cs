using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bookshelf : MonoBehaviour
{
    /// <summary>
    /// This script keeps track of all the collected books and handles the logic for clicking on books or page turners
    /// 
    /// currently no way to add new books
    /// </summary>
    public static Bookshelf S { get; private set; }

    //Keeps track of all the books in game
    public List<Books> Booklist = new List<Books>();

    Camera mainCamera;

    private PlayerInputActions inputActions; 



    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (S != null && S != this)
        {
            Destroy(this);
        }

        else
        {
            S = this;
        }

        inputActions = new PlayerInputActions();
        inputActions.Player.Click.performed += OnClick;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClick;
        inputActions.Player.Disable();
    }

    void Update()
    {
        
    }
    // This function detects if an object Book or if a PageTurner object
    //It will call functions to change book or turn page accordingly 
    public void OnClick(InputAction.CallbackContext context)
    {
              

        Vector2 mousePos;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse
            ? Mouse.current.position.ReadValue()
            : VirtualMouseUI.Instance.GetVirtualMousePosition();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
           if (hit.collider.GetComponent<BookTurner>())
            {
            hit.collider.GetComponent<BookTurner>().TurnThePage();
            }   
        }

        

    }

}


