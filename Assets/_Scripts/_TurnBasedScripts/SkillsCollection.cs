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
    private List<Skill> skillList;

    void Awake()
    {
        if(S != null && S != this)
        {
            Destroy(gameObject);
            return;
        }
        S = this;
    }

    // clear static ref on scene unload
    void OnDestroy()
    {
        if(S == this)
        {
            S = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (LoadManager.S.loadData)
        {
            PlayerData pd = LoadManager.S.playerDataToLoad;
            if (pd.AOEFearUnlocked) GetComponent<SkillAoeFear>().UnlockSkill();
            if (pd.BarrierUnlocked) GetComponent<SkillBarrier>().UnlockSkill();
            if (pd.TotemUnlocked) GetComponent<SkillFearTotem>().UnlockSkill();
            if (pd.LureUnlocked) GetComponent<SkillLure>().UnlockSkill();
            if (pd.RoomSwapUnlocked) GetComponent<SkillSwapRooms>().UnlockSkill();
            if (pd.PsychosisUnlocked) GetComponent<SkillPsychosis>().UnlockSkill();
            if (pd.RenovationUnlocked) GetComponent<SkillRenovation>().UnlockSkill();
            if (pd.BarrierDestroyUnlocked) GetComponent<SkillBarrierDestroy>().UnlockSkill();
        }

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
            if (skill.IsUnlocked())
            {
                skill.skillButton.gameObject.SetActive(true);
                skill.cultistCostTxt.gameObject.SetActive(true);
            }
        }
    }

    public void HIDE_SKILLS()
    {
        foreach (Skill skill in S.skillList)
        {
            skill.skillButton.gameObject.SetActive(false);
            skill.cultistCostTxt.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Turn all skill buttons to interactable if they have can be afforded or have usages.
    /// </summary>
    public void ENABLE_BUTTONS()
    {
        foreach(Skill skill in S.skillList)
        {
            if (skill.IsUnlocked() && skill.CanAffordUsage() && !skill.HasExpirationLock())
            {
                skill.skillButton.interactable = true;
            } else
            {
                skill.skillButton.interactable = false;
            }
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
        foreach (Skill skill in S.skillList)
        {
            skill.EndSkillNaked();
        }
    }
    
    public List<Skill> GetSkills()
    {
        return skillList;
    }

    // Fetchers for save system
    public bool IsTotemUnlocked()
    {
        return GetComponent<SkillFearTotem>().IsUnlocked();
    }

    public bool IsLureUnlocked()
    {
        return GetComponent<SkillLure>().IsUnlocked();
    }

    public bool IsAOEFearUnlocked()
    {
        return GetComponent<SkillAoeFear>().IsUnlocked();
    }

    public bool IsBarrierUnlocked()
    {
        return GetComponent<SkillBarrier>().IsUnlocked();
    }

    public bool IsRoomSwapUnlocked()
    {
        return GetComponent<SkillSwapRooms>().IsUnlocked();
    }

    public bool IsPsychosisUnlocked()
    {
        return GetComponent<SkillPsychosis>().IsUnlocked();
    }

    public bool IsRenovationUnlocked()
    {
        return GetComponent<SkillRenovation>().IsUnlocked();
    }

    public bool IsBarrierDestroyUnlocked()
    {
        return GetComponent<SkillBarrierDestroy>().IsUnlocked();
    }



}
