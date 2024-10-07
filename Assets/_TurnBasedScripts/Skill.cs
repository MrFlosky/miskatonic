using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Base class of skills.
/// Skills will be responsible for activating themselves upon being their button being clicked.  
/// While PlayerTurn is responsible for displaying skill options by invoking function in 
/// SkillsCollection.
/// </summary>
public class Skill : MonoBehaviour
{
    [Header("The point deduction associated with this skill")]
    public int ptDeduction = 1;

    [Header("The selectable image associated with this skill")]
    public Button skillButton;

    [Header("Event associated with this skill")]
    public UnityEvent skillEvent;

    /// SkillType associated with this skill. Default to NeverSet for debugging.
    protected SkillType st = SkillType.NeverSet; ///*** SET BACK TO PROTECTED LATER



    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void ActivateSkill()
    {
        SkillSystem.DEDUCT_PT(ptDeduction);
        skillEvent.Invoke();
        //Stuff that happens when skill is activated!
    }


}
