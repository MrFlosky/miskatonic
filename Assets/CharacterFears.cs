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
        if (Instance == null)
        {
            Instance = this;
        }
        else Debug.LogError("Two Character Fears in scene!");
        
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
        buttonGlow.active=true;
        totalFearsDiscovered++;
        switch (c)
        {
            case Character.HaroldAuden:
                hFearsActivated[fearNum] = true;
                hFearTexts[fearNum].SetActive(true);
                break;
            case Character.EvelynVentress:
                eFearsActivated[fearNum] = true;
                eFearTexts[fearNum].SetActive(true);
                break;
            case Character.IainSchweblin:
                iFearsActivated[fearNum] = true;
                iFearTexts[fearNum].SetActive(true);
                break;
            case Character.MackenzieJoyce:
                mFearsActivated[fearNum] = true;
                mFearTexts[fearNum].SetActive(true);
                break;
            default:
                Debug.LogError("Invalid Character set in DialogueBank!!");
                break;
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
