using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshPro missionTitle;
    [SerializeField] private TextMeshPro missionDescription;
    [SerializeField] private TextMeshPro cultistCost;
    [SerializeField] private TextMeshPro timeCost;
    [SerializeField] private TextMeshPro reward;

    private Mission missionObj;

    public void UpdateMission(Mission data)
    {
        missionObj = data;
        InitUI();
    }

    /// <summary>
    /// Grab all relevant UI components attached to this mission card.
    /// </summary>
    private void InitUI()
    {
        missionTitle.text = missionObj.missionTitle.text;
        missionDescription.text = missionObj.missionDescription.text;
        cultistCost.text = missionObj.cultistCost.text;
        //timeCost.text = missionObj.timeCost.text;
        //reward.text = missionObj.rewardName.text;
    }

    /// <summary>
    /// Upon clicking this card, activate it if user has enough cultist points.
    /// </summary>
    public bool SelectCard()
    {
        return missionObj.MissionSelect();
    }

}
