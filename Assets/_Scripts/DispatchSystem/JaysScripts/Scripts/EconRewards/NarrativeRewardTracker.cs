using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using CharacterData;
using System;

/// <summary>
/// This class is responsible for managing dialogues granted as rewards (ALL NarrativeReward objects).
/// Its similar to DialogueBank, but without the events that happen
/// with character dialogue.
/// </summary>
public class NarrativeRewardTracker : MonoBehaviour
{

    // Character first names as key, character remaining dialogues as list of indices
    // corresponding to respective yarn script.
    private Dictionary<Character, List<int>> characterClueDialogues = new Dictionary<Character, List<int>>();
    [SerializeField] DialogueRunner dialogueRunner;

    public int listIndex = 0;


    void Start()
    {
        LoadYarnDialogues();
    }    

    /// <summary>
    /// Play random dialogue for given character,
    /// then remove that dialogue index as a possibility.
    /// </summary>
    public void PlayDialogue(Character chara)
    {
        // Check if any clues remaining first
        if (characterClueDialogues[chara].Count == 0)
        {
            OutOfDialogues(chara);
            return;
        }        

        // access the dialogue at the random index, then play and remove
        // whatever dialogue number that returns from the list
        int dialogueKey = characterClueDialogues[chara][listIndex];
        string yarnAcessStr = CharacterNames.GetFullNameUnspaced(chara) + "Clue" + dialogueKey;

        CharacterGameDialogue.SetActivePortrait(chara);
        dialogueRunner.StartDialogue(yarnAcessStr);
        characterClueDialogues[chara].RemoveAt(listIndex);
    }

    /// <summary>
    /// Called when AI is out of new dialogues to present.
    /// </summary>
    public void OutOfDialogues(Character chara)
    {
        CharacterGameDialogue.SetActivePortrait(chara);
        dialogueRunner.StartDialogue("OutOfDialogue");
    }


    /// <summary>
    /// Load each respective character's yarn script and count all
    /// the dialogues within it by counting "title" within text file.
    /// 
    /// Then store in a dictionary where each key is a characters name,
    /// and the value is a list of unused dialogue indices corresponding
    /// to a yarn file format.
    /// </summary>
    void LoadYarnDialogues()
    {
        for (int i = 0; i < 4; i++)
        {
            Character character = (Character)i;
            string characterFirstName = CharacterNames.GetFirstName(character);
            string characterLastName = CharacterNames.GetLastName(character);

            string characterPath = $"YarnScripts/{characterFirstName}/{characterFirstName}{characterLastName}Clues";
            TextAsset[] textAssets = Resources.LoadAll<TextAsset>(characterPath);

                if (textAssets.Length > 0 && textAssets[0] != null)
                {
                    string documentText = textAssets[0].text;
                    int titleCount = CountDialogueNodes(documentText);
                    Debug.Log($"'{textAssets[0].name}' has {titleCount} title occurrences");

                    List<int> unPlayedDialogues = new List<int>();
                    unPlayedDialogues.Capacity = titleCount; // title count total num dialogues

                    for (int j = 0; j < titleCount; j++)
                    {
                        unPlayedDialogues.Add(j);
                    }
                    
                    characterClueDialogues.Add(character, unPlayedDialogues);
            }
                else
                {
                    Debug.LogError($"No text asset found in path: {characterPath}");
                }
        }
    }
    
    /// <summary>
    /// Helper function to count number of dialogues in a yarn file,
    /// by counting titles.
    /// </summary>
    /// <param name="text">
    /// Entire yarn script in string format
    /// </param>
    /// <returns>
    /// Number of dialogues in yarn file
    /// </returns>
    private int CountDialogueNodes(string text)
    {
        string searchTerm = "title";
        int count = 0;
        int index = 0;

        // Count occurrences
        while ((index = text.IndexOf(searchTerm, index)) != -1)
        {
            count++;
            index += searchTerm.Length;
        }

        return count;
    }
}
