using System;
using System.Collections;
using System.Collections.Generic;
using CharacterData;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles the discovery of fears and updating of UI
/// </summary>
public class CharacterFears : MonoBehaviour
{
    public static CharacterFears Instance;

    public int totalFearsDiscovered = 0;
    private bool[] hFearsActivated;
    private bool[] eFearsActivated;
    private bool[] iFearsActivated;
    private bool[] mFearsActivated;
    public GameObject[] hFearTexts;
    public GameObject[] eFearTexts;
    public GameObject[] iFearTexts;
    public GameObject[] mFearTexts;

    public ButtonGlow buttonGlow;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Two Character Fears in scene!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        
        if (LoadManager.S.loadData)
        {
            PlayerData pd = LoadManager.S.playerDataToLoad;
            hFearsActivated = pd.haroldFears;
            iFearsActivated = pd.iainFears;
            mFearsActivated = pd.mackenzieFears;
            eFearsActivated = pd.evelynFears;
            UpdateTexts();
            return;
        }

        hFearsActivated = new bool[hFearTexts.Length];
        eFearsActivated = new bool[eFearTexts.Length];
        iFearsActivated = new bool[iFearTexts.Length];
        mFearsActivated = new bool[mFearTexts.Length];

    }

    // clear static ref on scene unload
    void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Updates texts to match the list of bools
    /// </summary>
    private void UpdateTexts()
    {
        for (int i = 0; i < hFearsActivated.Length; i++)
        {
            if (hFearsActivated[i])
            {
                hFearTexts[i].SetActive(true);
            }
        }

        for (int i = 0; i < iFearsActivated.Length; i++)
        {
            if (iFearsActivated[i])
            {
                iFearTexts[i].SetActive(true);
            }
        }

        for (int i = 0; i < mFearsActivated.Length; i++)
        {
            if (mFearsActivated[i])
            {
                mFearTexts[i].SetActive(true);
            }
        }

        for (int i = 0; i < eFearsActivated.Length; i++)
        {
            if (eFearsActivated[i])
            {
                eFearTexts[i].SetActive(true);
            }
        }
    }


    /// <summary>
    /// "Discovers" a fear for a given character c, updating the stored list of bools in CharacterFears used for saving, as well as the text in the character profiles UI
    /// </summary>
    /// <param name="c"></param>
    /// <param name="fearNum"></param>
    public void DiscoverFear(Character c, int fearNum)
    {
        // Obtain given character's fear discovered bool array and revealed text array.
        bool[] fearsArray = null;
        GameObject[] fearTextsArray = null;   
        switch (c)
        {
            case Character.HaroldAuden:
                fearsArray = hFearsActivated;
                fearTextsArray = hFearTexts;
                break;
            case Character.EvelynVentress:
                fearsArray = eFearsActivated;
                fearTextsArray = eFearTexts;
                break;
            case Character.IainSchweblin:
                fearsArray = iFearsActivated;
                fearTextsArray = iFearTexts;
                break;
            case Character.MackenzieJoyce:
                fearsArray = mFearsActivated;
                fearTextsArray = mFearTexts;
                break;
            default:
                Debug.LogError("Invalid Character");
                return;
        }
        
        // Check if fearNum exists for character
        if (fearNum < 0 || fearNum >= fearsArray.Length)
        {
            Debug.LogError($"Fear index {fearNum} out of range for character {c}. Valid range: 0-{fearsArray.Length - 1}");
            return;
        }
        
        // Only unlock if the fear hasn't been discovered yet
        if (!fearsArray[fearNum])
        {
            buttonGlow.active = true;
            totalFearsDiscovered++;
            fearsArray[fearNum] = true;
            fearTextsArray[fearNum].SetActive(true);
        }
    }

    public bool[] GetFearsDiscovered(Character c)
    {
        switch (c)
        {
            case Character.EvelynVentress:
                return eFearsActivated;
            case Character.HaroldAuden:
                return hFearsActivated;
            case Character.IainSchweblin:
                return iFearsActivated;
            case Character.MackenzieJoyce:
                return mFearsActivated;
            default:
                Debug.LogError("Character not recognized in GetFearsDiscovered!");
                return null;
        }
    }

}
