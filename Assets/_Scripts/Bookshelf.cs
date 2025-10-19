using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bookshelf : MonoBehaviour
{
    public static Bookshelf S { get; private set; }

    //Keeps track of all the books in game
    public List<Books> Booklist = new List<Books>();

    Camera mainCamera;



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
    }

    void Update()
    {
        
    }

    public void OnClick(InputAction.CallbackContext context)
    {
              

        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse
            ? Mouse.current.position.ReadValue()
            : VirtualMouseUI.Instance.GetVirtualMousePosition();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            print(hit.collider.gameObject.name);
            if (hit.collider.CompareTag("Book"))
            {

                hit.collider.GetComponent<Books>().ChangeBook();
            }

            else if (hit.collider.GetComponent<BookTurner>())
            {
            hit.collider.GetComponent<BookTurner>().TurnThePage();
            }   
        }

        

    }

}


