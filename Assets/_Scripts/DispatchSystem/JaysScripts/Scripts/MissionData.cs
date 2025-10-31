using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Missions/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Title")]
    public string title;

    [Header("Description")]
    [TextArea(3, 5)]
    public string description;

    [Header("Cultist Cost")]
    public int cultistCost;

    [Header("Removed from pool for rest of game after play?")]
    public bool oneTimeUse;

    [Header("Turn Activation Time")]
    public int turnActivateTime;

    [Header("Reward for purchasing this mission")]
    public Reward reward;

    [Header("If a skill reward, how many usages are given?")]
    public int usages;

    [Header("If a cultist reward, how many are granted?")]
    public int cultistsGranted;

    [Header("If granting path previews, for how many turns?")]
    public int previewsGranted;

    [Header("If a narrative reward, who is it for?")]
    public CharacterData.Character character;
}
