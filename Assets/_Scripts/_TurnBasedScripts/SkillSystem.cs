using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
#endif
using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// All possible skill types.
/// Neverset is used as a default for function args and bug catching.
/// </summary>
public enum SkillType{Skill1, Skill2, Skill3, NeverSet};

/// <summary>
/// obsolete, use resourcemanager
/// TODO: move this functionality elsewhere and delete this
/// </summary>
public class SkillSystem : MonoBehaviour
{

    /// The text that holds current points
    public TextMeshProUGUI txt;

    /// Camera used for detecting clicks. Defaults to main in scene.
    public Camera mainCam;

    public int availablePts
    {
        get
        {
            return ResourceManager.Instance.worshipPoints;
        }
        set
        {
            ResourceManager.Instance.SetWorship(value);
        }
    }

    void Start()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main; // Use main camera if none specified
        }
    }


    public void HIDE_PTS()
    {
        ResourceManager.Instance.HideResourceText();
    }



    public Camera GET_CAM()
    {
        return mainCam;
    }


}
