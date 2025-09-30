using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Game/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Title")]
    public string title;

    [Header("Description")]
    [TextArea(3, 5)]
    public string description;

    [Header("Cultist Cost")]
    public int cultistCost;

    [Header("Turn Activation Time")]
    public int turnActivateTime;

    [Header("Reward for purchasing this mission")]
    public Reward reward;

    [Header("If a skill reward, how many usages are given?")]
    public int usages;

    [Header("If a cultist reward, how many are granted?")]
    public int cultistsGranted;
}
