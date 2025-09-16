using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for bandaid UI management.
/// Im calling this lite because not everything relevant to UI management is here yet, but
/// might as well start creating a solution.
/// </summary>
public class UIManagerLite : MonoBehaviour
{
    public static UIManagerLite S;

    /// <summary>
    /// Component attached to new dialogue system's line presenter.
    /// For wahtever reason, it does not automatically turn off block raycasts and interactable
    /// when a dialogue is complete. Doing that manually here.
    /// </summary>
    [SerializeField] private CanvasGroup evilCanvasGroup;


    void Start()
    {
        S = this;
    }
    /// <summary>
    /// Block raycasts and turn ON interaction for dialogue PRESENTER.
    /// </summary>
    public static void EnableDialogueSettings()
    {
        S.evilCanvasGroup.interactable = true;
        S.evilCanvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Stop blocking raycasts and turn off interaction for dialogue PRESENTER.
    /// </summary>
    public static void DisableDialogueSettings()
    {
        S.evilCanvasGroup.interactable = false;
        S.evilCanvasGroup.blocksRaycasts = false;
    }
}
