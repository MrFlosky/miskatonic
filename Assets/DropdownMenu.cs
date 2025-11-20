using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// script that animates and sets the abilities tab as active and inactive with aniamations
public class DropdownMenu : MonoBehaviour
{
    public GameObject dropdownMenu; //the red banner containing powers 
    private Animator animator; //runs on parent
    private bool isOpen = false;

    void Start()
    {
        animator = dropdownMenu.GetComponent<Animator>(); //parent
        dropdownMenu.SetActive(false); // starts closed (red banner)
    }

    public void ToggleDrop()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    void Open()
    {
        dropdownMenu.SetActive(true); //starts before animating
        animator.SetBool("isOpen", true); // opens
        isOpen = true;
    }

    void Close()
    {
        animator.SetBool("isOpen", false);
        isOpen = false;
        StartCoroutine(DeactivateAfterAnimation()); //sets object inactive when animation finished
    }

    //public void OnCloseAnimation() // closes menu with event
    //{
    //  dropdownMenu.SetActive(false);
    //}
    System.Collections.IEnumerator DeactivateAfterAnimation()
    {
        //wait for length of animation
        yield return new WaitForSeconds(0.15f); //same length as animation recording
    }
}   
