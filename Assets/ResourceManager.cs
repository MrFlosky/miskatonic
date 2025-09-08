using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the player's resources, currently just idols and worship points
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public int availableIdols = 0;
    public int worshipPoints = 0;

    public TextMeshProUGUI idolText;
    public TextMeshProUGUI worshipText;



    void Awake()
    {
        Instance = this;
        idolText.text = "Idols: " + availableIdols;
        worshipText.text = "Worship Points: " + worshipPoints;
    }

    public void AddIdols(int num)
    {
        availableIdols += num;
        idolText.text = "Idols: " + availableIdols;
    }

    public void RemoveIdols(int num)
    {
        availableIdols -= num;
        idolText.text = "Idols: " + availableIdols;
        if (availableIdols == 0) MinigameManager.Instance.EndMinigame();
    }

    public void AddWorship(int num)
    {
        worshipPoints += num;
        worshipText.text = "Worship Points: " + worshipPoints;
    }

    public void SetWorship(int num)
    {
        worshipPoints = num;
        worshipText.text = "Worship Points: " + worshipPoints;
    }

    public void RemoveWorship(int num)
    {
        if (worshipPoints >= 0) worshipPoints -= num;
        worshipText.text = "Worship Points: " + worshipPoints;
    }

    public void HideResourceText()
    {
        idolText.enabled = false;
        worshipText.enabled = false;
    }

    public void ShowResourceText()
    {
        idolText.enabled = true;
        worshipText.enabled = true;
    }
    
}
