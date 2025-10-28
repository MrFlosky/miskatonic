using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static object that holds the currently desired player data, this object persists on scene change.
/// </summary>
public class LoadManager : MonoBehaviour
{
    public static LoadManager S;
    public PlayerData playerDataToLoad;


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (S == null) S = this;
        else if (S != this) Destroy(this.gameObject);
    }

    
}
