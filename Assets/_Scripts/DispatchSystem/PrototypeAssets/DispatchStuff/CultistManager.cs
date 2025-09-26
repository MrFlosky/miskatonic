using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CultistManager : MonoBehaviour
{
    public static CultistManager Instance;

    public GameObject[] AvailableOptionsStartList;
    private float optionOffset = 200f;
    private GameObject option1;
    private GameObject option2;
    private GameObject option3;
    private List<GameObject> AvailableOptionPrefabs = new List<GameObject>();

    public GameObject mainCanvas;

    // items to hide when presenting options
    public GameObject profilesButton;
    public GameObject endTurnButton;
    public GameObject fearMeter;

    public GameObject hideOptionsButton;
    public GameObject showOptionsButton;
    public GameObject rerollButton;

    private TextMeshProUGUI rerollText;

    public int rerollsAvailable = 2;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        rerollText = rerollButton.GetComponentInChildren<TextMeshProUGUI>();
        rerollText.text = "Rerolls: " + rerollsAvailable;
    }


    public void PresentOptions()
    {
        optionOffset = Screen.width / 3;

        // Call opening animation here
        profilesButton.SetActive(false);
        endTurnButton.SetActive(false);
        fearMeter.SetActive(false);

        hideOptionsButton.SetActive(true);
        rerollButton.SetActive(true);

        SpawnNewOptions();

    }


    private void SpawnNewOptions()
    {

        // If there are less than 3 missions to choose from left, reset the list to what it was on game start
        if (AvailableOptionPrefabs.Count < 3)
        {
            ResetOptionsList();
        }

        // Destroy anything still in the holder objects
        if (option1 != null) Destroy(option1);
        if (option2 != null) Destroy(option2);
        if (option3 != null) Destroy(option3);

        // Create 3 random options from the list of available options
        int i = Random.Range(0, AvailableOptionPrefabs.Count);
        option1 = Instantiate(AvailableOptionPrefabs[i], mainCanvas.transform);
        option1.GetComponent<RectTransform>().anchoredPosition = new Vector2(-optionOffset, 0);
        AvailableOptionPrefabs.RemoveAt(i);

        i = Random.Range(0, AvailableOptionPrefabs.Count);
        option2 = Instantiate(AvailableOptionPrefabs[i], mainCanvas.transform);
        AvailableOptionPrefabs.RemoveAt(i);

        i = Random.Range(0, AvailableOptionPrefabs.Count);
        option3 = Instantiate(AvailableOptionPrefabs[i], mainCanvas.transform);
        option3.GetComponent<RectTransform>().anchoredPosition = new Vector2(optionOffset, 0);
        AvailableOptionPrefabs.RemoveAt(i);
    }


    public void HideOptions()
    {
        option1.SetActive(false);
        option2.SetActive(false);
        option3.SetActive(false);

        profilesButton.SetActive(true);
        fearMeter.SetActive(true);

        hideOptionsButton.SetActive(false);
        rerollButton.SetActive(false);
        showOptionsButton.SetActive(true);
        // Call hiding animation here
    }

    public void ShowOptions()
    {
        option1.SetActive(true);
        option2.SetActive(true);
        option3.SetActive(true);

        profilesButton.SetActive(false);
        fearMeter.SetActive(false);

        hideOptionsButton.SetActive(true);
        rerollButton.SetActive(true);
        showOptionsButton.SetActive(false);
        // Call popup animation here
    }

    public void OptionChosen()
    {
        Destroy(option1);
        Destroy(option2);
        Destroy(option3);

        profilesButton.SetActive(true);
        endTurnButton.SetActive(true);
        fearMeter.SetActive(true);

        rerollButton.SetActive(false);
        hideOptionsButton.SetActive(false);
    }

    public void Reroll()
    {
        if (rerollsAvailable > 0)
        {
            rerollsAvailable--;
            SpawnNewOptions();
            rerollText.text = "Rerolls: " + rerollsAvailable;
        }
    }

    private void ResetOptionsList()
    {
        AvailableOptionPrefabs.Clear();
        for (int i = 0; i < AvailableOptionsStartList.Length; i++)
        {
            AvailableOptionPrefabs.Add(AvailableOptionsStartList[i]);
        }    
    }



}
