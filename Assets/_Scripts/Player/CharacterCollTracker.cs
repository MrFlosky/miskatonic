using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is responsible for tracking how many characters are in the collider of the game object this
/// script is attached to.
/// </summary>
public class CharacterCollTracker : MonoBehaviour
{   
    /// <summary>
    /// Number of characters in this collider.
    /// </summary>
    private int characterCount;

    void Awake()
    {
        characterCount = 0;
    }

    public void AddCharacter()
    {
        characterCount++;
    }

    public void RemoveCharacter()
    {
        characterCount --;
    }

    /// <summary>
    /// Only allow character to ghostify an object if its the first inside it.
    /// </summary>
    public bool CanGhostify()
    {
        return characterCount == 0;
    }

    /// <summary>
    /// If a ghostify collider still has characters left in it, dont allow a fade back in to
    /// opaque.
    /// </summary>
    public bool HasCharacters()
    {
        return characterCount > 0;
    }

}
