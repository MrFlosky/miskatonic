using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private CanvasGroup lineAndOptionsCG;
    [SerializeField] private CanvasGroup optionsPresenterCG;

    [SerializeField] private Button endTurnButton;
    // button for starting mission dealer interaction
    [SerializeField] private Button dealerButton;


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

        S.lineAndOptionsCG.interactable = true;
        S.lineAndOptionsCG.blocksRaycasts = true;

        S.optionsPresenterCG.interactable = true;
        S.optionsPresenterCG.blocksRaycasts = true;
        S.optionsPresenterCG.alpha = 1;
    }

    /// <summary>
    /// Stop blocking raycasts and turn off interaction for dialogue PRESENTER.
    /// </summary>
    public static void DisableDialogueSettings()
    {
        S.evilCanvasGroup.interactable = false;
        S.evilCanvasGroup.blocksRaycasts = false;

        S.lineAndOptionsCG.interactable = false;
        S.lineAndOptionsCG.blocksRaycasts = false;

        S.optionsPresenterCG.interactable = false;
        S.optionsPresenterCG.blocksRaycasts = false;
        S.optionsPresenterCG.alpha = 0;
    }

    public static void DisableEndTurnButton()
    {
        S.endTurnButton.interactable = false;
    }

    public static void EnableEndTurnButton()
    {
        S.endTurnButton.interactable = true;
    }

    /// <summary>
    /// Return if button is active or not, for use with skills
    /// so theyre not allowing end turn button active when dealer
    /// hasnt been DEALT with haha see what i did tehre ahhahaah
    /// </summary>
    public static bool DealerButtonIsActive()
    {
        if (S.dealerButton.IsActive())
        {
            return true;
        }
        return false;
    }
}
