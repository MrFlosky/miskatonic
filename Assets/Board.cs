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
    public GameObject[] NPCModels;
    private bool starting = true;


    /// <summary>
    /// Flips the gameboard and disables any other models
    /// </summary>
    public void FlipBoard()
    {
        starting = !MinigameManager.Instance.gameRunning;
        StartCoroutine(nameof(FlipRoutine));
        
    }

    public IEnumerator FlipRoutine()
    {
        // Handle actions that should happen before the board flips
        if (!starting) MinigameManager.Instance.gameRunning = false;
        else
        {
            foreach (GameObject model in NPCModels)
            {
                model.SetActive(false);
            }
        }

        // Rotate the board
        for (int i = 0; i < flipSteps; i++)
        {
            transform.Rotate(new Vector3(180 / flipSteps, 0, 0));
            yield return new WaitForSeconds(flipDuration / flipSteps);
        }

        // Handle actions that should happen after the board flips
        if (starting) MinigameManager.Instance.gameRunning = true;
        else
        {
            foreach (GameObject model in NPCModels)
            {
                model.SetActive(true);
            }
        }

    }

}
