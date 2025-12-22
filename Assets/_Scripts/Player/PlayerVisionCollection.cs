using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for holding all character's PlayerVision objects,
/// so that they may be used in PV as a collection.
/// </summary>
public class PlayerVisionCollection : MonoBehaviour
{  
    static public PlayerVisionCollection S;
    /// <summary>
    /// Array of all player visions attached to characters.
    /// </summary>
    private PlayerVision[] pvArr;

    void Awake()
    {
        if(S != null && S != this)
        {
            Destroy(gameObject);
            return;
        }
        S = this;

        pvArr = GetComponentsInChildren<PlayerVision>();
        Debug.LogWarning(pvArr.Length);
    }

    // clear static ref on scene unload
    void OnDestroy()
    {
        if(S == this)
        {
            S = null;
        }
    }

    /// <summary>
    /// Get array of all player visions.
    /// </summary>
    public PlayerVision[] GetPlayerVisions()
    {
        return S.pvArr;
    }

}
