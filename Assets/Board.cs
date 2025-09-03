using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public bool isFlipped = false;
    public float flipDuration = 1f;
    public float flipSteps = 30f;

    private void Start()
    {
        FlipBoard();
    }
    public void FlipBoard()
    {
        StartCoroutine(nameof(FlipRoutine));
        Debug.LogWarning("Flip called!");
    }

    public IEnumerator FlipRoutine()
    { 
        Debug.LogWarning("Flip stepped!");
        transform.Rotate(new Vector3(180 / flipSteps, 0, 0));
        yield return new WaitForSeconds(flipDuration / flipSteps);
    }

}
