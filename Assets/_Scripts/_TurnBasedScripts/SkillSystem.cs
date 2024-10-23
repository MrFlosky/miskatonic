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

    /// Camera used for detecting clicks. Defaults to main in scene.
    public Camera mainCam;

    /// The number of available points the player has. Public for proto
    [Header("Number of available points the player has")]
    public int availablePts = 3;
    
    void Start()
    {
        S = this;
        DISPLAY_PTS(); ///*** DEBUG

        if (mainCam == null)
        {
            mainCam = Camera.main; // Use main camera if none specified
        }
    }

    static public void DEDUCT_PT(int deduction)
    {
        if(S.availablePts != 0)
        {
            S.availablePts -= deduction;
            DISPLAY_PTS(); // Update text each deduction
        }
    }

    static public void ADD_PT(int addition)
    {
        S.availablePts+=addition; // may need cap check here
        DISPLAY_PTS();
    }
    
    static public void SET_PT(int num)
    {
        S.availablePts = num;
    }

    static public void DISPLAY_PTS()
    {
        S.txt.text = "Available Points: " + S.availablePts;
        if(S.txt.IsActive() == false)
        {
            S.txt.gameObject.SetActive(true);
        }
    }

    static public void HIDE_PTS()
    {
        S.txt.gameObject.SetActive(false);
    }

    static public Camera GET_CAM()
    {
        return S.mainCam;
    }

    static public int GET_PTS()
    {
        return S.availablePts;
    }


}
