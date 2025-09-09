using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
#endif
using UnityEngine;
using UnityEngine.UI;



public class SkillSystem : MonoBehaviour
{
    /// The text that holds current points
    public TextMeshProUGUI txt;

    /// Camera used for detecting clicks. Defaults to main in scene.
    public Camera mainCam;

    /// The number of available points the player has. Public for proto
    [Header("Number of available points the player has")]
    public int availablePts = 3;
    
    void Start()
    {
        DISPLAY_PTS(); ///*** DEBUG

        if (mainCam == null)
        {
            mainCam = Camera.main; // Use main camera if none specified
        }
    }

    public void DEDUCT_PT(int deduction)
    {
        if(availablePts != 0)
        {
            availablePts -= deduction;
            DISPLAY_PTS(); // Update text each deduction
        }
    }

    public void ADD_PT(int addition)
    {
        availablePts+=addition; // may need cap check here
        DISPLAY_PTS();
    }
    
    public void SET_PT(int num)
    {
        availablePts = num;
    }

    public void DISPLAY_PTS()
    {
        txt.text = "Available Points: " + availablePts;
        if(txt.IsActive() == false)
        {
            txt.gameObject.SetActive(true);
        }
    }

    public void HIDE_PTS()
    {
        txt.gameObject.SetActive(false);
    }

    public Camera GET_CAM()
    {
        return mainCam;
    }

    public int GET_PTS()
    {
        return availablePts;
    }


}
