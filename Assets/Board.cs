using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles manipulation of the entire game board
/// </summary>
public class Board : MonoBehaviour
{
    public bool isFlipped = false;
    public float flipDuration = 1f;
    public float flipSteps = 30f;
    public MeshRenderer[] NPCModels;


    /// <summary>
    /// Flips the gameboard and disables any other models
    /// </summary>
    public void FlipBoard()
    {
        StartCoroutine(nameof(FlipRoutine));
        foreach (MeshRenderer model in NPCModels)
        {
            model.enabled = !model.enabled;
        }
    }

    public IEnumerator FlipRoutine()
    {
        for (int i = 0; i < flipSteps; i++)
        {
            transform.Rotate(new Vector3(180 / flipSteps, 0, 0));
            yield return new WaitForSeconds(flipDuration / flipSteps);
        }
        MinigameManager.Instance.gameStarted = !MinigameManager.Instance.gameStarted;
    }

}
