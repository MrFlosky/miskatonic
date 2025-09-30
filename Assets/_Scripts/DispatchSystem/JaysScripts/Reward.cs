using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

/// <summary>
/// This class is abstract, responsible for stating all attributes/functions all Rewards will have.
/// </summary>
public abstract class Reward : MonoBehaviour
{
    [SerializeField] protected string rewardName;
    // Popup associated with being granted reward
    [SerializeField] protected GameObject rewardPopupUI;
    protected TextMeshProUGUI rewardText;

    // time till message dissapears
    [SerializeField] protected float timeToDissapear = 5.0f;
    protected float timeElapsed = 0.0f;

    /// <summary>
    /// Initialize reward UI references.
    /// </summary>
    public abstract void InitRewardUI();
    /// <summary>
    /// Grant player reward for purchasing this mission.
    /// </summary>
    public abstract void GrantReward(); // derived children classes must implement abstract
    public abstract void DisplayRewardMessage();


}
