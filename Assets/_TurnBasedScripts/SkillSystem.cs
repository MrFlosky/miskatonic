using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// All possible skill types.
/// Neverset is used as a default for function args and bug catching.
/// </summary>
public enum SkillType{Skill1, Skill2, Skill3, NeverSet};

public class SkillSystem : MonoBehaviour
{
    static private SkillSystem S;

    /// The text that holds current points
    public TextMeshProUGUI txt;

    /// The number of availible points the player has. Public for proto
    [Header("Number of availible points the player has")]
    public int availiblePts = 3;
    
    void Start()
    {
        S = this;
        DISPLAY_PTS(); ///*** DEBUG
    }

    static public void DEDUCT_PT(int deduction)
    {
        if(S.availiblePts != 0)
        {
            S.availiblePts -= deduction;
            DISPLAY_PTS(); // Update text each deduction
        }
    }

    static public void ADD_PT(int addition)
    {
        S.availiblePts+=addition;
    }
    
    static public void SET_PT(int num)
    {
        S.availiblePts = num;
    }

    static public void DISPLAY_PTS()
    {
        S.txt.text = "Availible Points: " + S.availiblePts;
        if(S.txt.IsActive() == false)
        {
            S.txt.gameObject.SetActive(true);
        }
    }

    static public void HIDE_PTS()
    {
        S.txt.gameObject.SetActive(false);
    }



}
