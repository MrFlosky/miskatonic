using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bookshelf : MonoBehaviour
{
    public static Bookshelf S {  get; private set; }

    //Keeps track of all the books in game
    public List<Books> Booklist= new List<Books>();

    

    private void Awake()
    {
        if(S != null && S != this)
        {
            Destroy(this);
        }

        else
        {
            S = this;
        }
    }



}


