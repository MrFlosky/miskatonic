using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this script triggers an animation on end. helped with Claude

public class TriggerAnimatorOnEnd : MonoBehaviour
{
    public Animator targetAnimator;
    public string triggerName;

    public Animator secondaryAnimator;
    public string secondaryTriggerName;

    //called by animation event
    public void TriggerAnimation()
    {
        Debug.Log("TriggerAnimation called!");

        if (targetAnimator != null)
        {
            Debug.Log("Triggering: " + triggerName + " on " + targetAnimator.gameObject.name);
            targetAnimator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogError("Target Animator is null!");
        }
    }

    //triggers both animators at once
    public void TriggerBothAnimations()
    {
        Debug.Log("TriggerBothAnimations called!");
        TriggerAnimation();

        if (secondaryAnimator != null)
        {
            Debug.Log("Triggering secondary: " + secondaryTriggerName + " on " + secondaryAnimator.gameObject.name);
            secondaryAnimator.SetTrigger(secondaryTriggerName); 
        }
    }

    private void TriggerAnimationAndReset()
    {
        Debug.Log("TriggerAnimationReset Called!");
        TriggerAnimation();

        Animator selfAnimator = GetComponent<Animator>();
        if (selfAnimator != null)
        {
            Debug.Log("Resetting");
            selfAnimator.Play("Idle");
        }
    }
}