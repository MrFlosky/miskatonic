using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// This class is responsible for point resource management.
/// </summary>
public class SkillSystem : MonoBehaviour
{
    // All skills attached to this game object
    private Skill[] skills;

    /// Camera used for detecting clicks. Defaults to main in scene.
    public Camera mainCam;
    
    void Start()
    {
        skills = GetComponents<Skill>();
        DISPLAY_PTS(); ///*** DEBUG

        if (mainCam == null)
        {
            mainCam = Camera.main; // Use main camera if none specified
        }
    }

    public void DEDUCT_PT(int deduction, Skill skill)
    {
        if(skill.usagesRemaining != 0)
        {
            skill.usagesRemaining -= deduction;

            //DISPLAY_PTS(); // Update text each deduction TODO
        }
    }

    public void ADD_PT(int addition, Skill skill)
    {
        skill.usagesRemaining += addition; // may need cap check here

        //DISPLAY_PTS(); TODO
    }
    
    public void SET_PT(int num, Skill skill)
    {
        skill.usagesRemaining = num;
    }

    public void DISPLAY_PTS()
    {
        return; // TODO SEE SKILL CLASS
        foreach (Skill skill in skills)
        {
            if (skill.IsUnlocked())
            {
                TMP_Text txt = skill.usagesRemainTxt;
                txt.text = "X " + skill.usagesRemaining;
                if (txt.IsActive() == false)
                {
                    txt.gameObject.SetActive(true);
                }
            }
        }

    }

    public void HIDE_PTS()
    {
        return; // TODO
        foreach (Skill skill in skills)
        {
            TMP_Text txt = skill.usagesRemainTxt;
            txt.gameObject.SetActive(false);
        }
    }

    public Camera GET_CAM()
    {
        return mainCam;
    }

    // public int GET_PTS()
    // {
    //     return availablePts;
    // }


}
