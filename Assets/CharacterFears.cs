using System;
using System.Collections;
using System.Collections.Generic;
using CharacterData;
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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Debug.LogError("Two Character Fears in scene!");

        hFearsActivated = new bool[hFearTexts.Length];
        eFearsActivated = new bool[eFearTexts.Length];
        iFearsActivated = new bool[iFearTexts.Length];
        mFearsActivated = new bool[mFearTexts.Length];
    }

    public void DiscoverFear(Character c, int fearNum)
    {
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


}
