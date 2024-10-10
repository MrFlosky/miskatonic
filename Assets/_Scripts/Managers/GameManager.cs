using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    private enum Turn { Player, AI }
    private Turn currentTurn;

    private void Awake() {
        Instance = this;
        currentTurn = Turn.Player;
    }

    public bool IsAITurn() {
        return currentTurn == Turn.AI;
    }
    
    public bool IsPlayerTurn() {
        return currentTurn == Turn.Player;
    }

    public void EndTurn() {
        if (currentTurn == Turn.Player) {
            currentTurn = Turn.AI;
        } else {
            currentTurn = Turn.Player;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }
}
