using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Turns : MonoBehaviour
{
    #region Event variables that change/call functions upon player/AI turn switching
    /// Text displaying the name of whose turn it is.
    // Apparently serialized fields must be assigned for all children, even when private. Stupid feature.
    [SerializeField] protected TMP_Text turnText;
    [SerializeField] protected GameObject endTurnButton;
    [SerializeField] private GameObject skillsManager;
    protected SkillsCollection skillsUI;
    protected SkillSystem skillSystem;
    #endregion

    void Awake()
    {
        skillsUI = skillsManager.GetComponent<SkillsCollection>();
        skillSystem = skillsManager.GetComponent<SkillSystem>();
    }
}
