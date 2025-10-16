using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// All this class does is hold the collection of skills and display them.
/// </summary>
public class SkillsCollection : MonoBehaviour
{
    static public SkillsCollection S;
    /// Collection of Skill objects.
    List<Skill> skillList;
    

    // Start is called before the first frame update
    void Start()
    {
        S = this;
        S.skillList = new List<Skill>(); // HAVE TO DO THIS!!!!!!!!!!!!!!!!! IDK WHY BUT IT TOOK ME LIKE 30+ MINS TO DEBUG WHY THIS WASNT WORKING. NOW WORKING 
        //The new keyword is necessary to instantiate the list, allocating memory for it and initializing it so you can start adding elements

         foreach(Skill skill in transform.GetComponentsInChildren<Skill>())
        {
            // OBJECT NULLREF HERE NOT LETTING ME ADD SKILLS TO LIST. FIXED
            // Checks to make sure there is a button added to this skill. Omit base class.
            if(skill.skillButton != null)
            {
                S.skillList.Add(skill);
                
            }
        }
    }

    public void DISPLAY_SKILLS()
    {
        foreach(Skill skill in S.skillList)
        {
            skill.skillButton.gameObject.SetActive(true);
        }
    }

    public void HIDE_SKILLS()
    {
        foreach(Skill skill in S.skillList)
        {
            skill.skillButton.gameObject.SetActive(false);
        }
    }

    public void ENABLE_BUTTONS()
    {
        foreach(Skill skill in S.skillList)
        {
            skill.skillButton.interactable = true;
        }
    }

    public void DISABLE_BUTTONS()
    {
        foreach(Skill skill in S.skillList)
        {
            skill.skillButton.interactable = false;
        }
    }

    public void DEACTIVATE_SKILLS()
    {
        foreach (Skill skill in S.skillList)
        {
            skill.EndSkill();
        }
    }
    
    /// <summary>
    /// Calls end skill without UI manager transition to board base
    /// </summary>
    public void DEACTIVATE_SKILLS_NAKED()
    {
        foreach(Skill skill in S.skillList)
        {
            skill.EndSkillNaked();
        }
    }

}
