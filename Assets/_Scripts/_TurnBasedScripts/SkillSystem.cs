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

    /// Camera used for detecting clicks. Defaults to main in scene.
    public Camera mainCam;
    
    void Start()
    {
        //DISPLAY_PTS(); ///*** DEBUG

        if (mainCam == null)
        {
            mainCam = Camera.main; // Use main camera if none specified
        }
    }


    public Camera GET_CAM()
    {
        return mainCam;
    }

    public int GET_PTS()
    {
        return WorshipPoints.S.totalPoints;
    }


}
