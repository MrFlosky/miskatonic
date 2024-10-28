using UnityEngine;
using UnityEngine.Serialization;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject[] dialogues;
    [SerializeField] private GameObject[] thoughts;
    private int _currentIndex = 0;
    public bool usedDialogue = false;

    private void Update()
    {
        if (!usedDialogue || !TurnBasedSystem.IsAITurn()) return;
        
        usedDialogue = false;
        Increment();
    }

    public void Increment()
    {
        if (dialogues.Length == _currentIndex+1) return;
        if (usedDialogue) return;
        
        _currentIndex++;
    }

    public GameObject GetCurrentDialogue()
    {
        return !usedDialogue ? dialogues[_currentIndex] : thoughts[Random.Range(0, thoughts.Length)];
    }
}
