using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject[] dialogues;
    private int _currentIndex = 0;
    
    public void Increment()
    {
        if (dialogues.Length == _currentIndex+1) return;

        _currentIndex++;
    }

    public GameObject GetCurrentDialogue()
    {
        return dialogues[_currentIndex];
    }
}
