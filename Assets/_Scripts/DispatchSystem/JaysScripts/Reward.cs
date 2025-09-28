using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is abstract, responsible for stating all attributes/functions all Rewards will have.
/// </summary>
public abstract class Reward : MonoBehaviour
{
    // Popup associated with being granted reward
    [SerializeField] protected GameObject rewardPopupUI;

    /// <summary>
    /// Grant player reward for purchasing this mission.
    /// </summary>
    public abstract void GrantReward(); // derived children classes must implement abstract


}
