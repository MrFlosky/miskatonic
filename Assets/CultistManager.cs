using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistManager : MonoBehaviour
{
    public static CultistManager Instance;

    public List<GameObject> AvailableOptionPrefabs;
    private float optionOffset = 200f;
    private GameObject option1;
    private GameObject option2;
    private GameObject option3;
    private List<GameObject> AvailableOptionsStartList;

    public GameObject mainCanvas;

    // items to hide when presenting options
    public GameObject profilesButton;
    public GameObject endTurnButton;
    public GameObject fearMeter;

    public GameObject hideOptionsButton;
    public GameObject showOptionsButton;

    public int rerollsAvailable = 2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        AvailableOptionsStartList = AvailableOptionPrefabs;
    }



    public void PresentOptions()
    {
        optionOffset = Screen.width / 3;

        // Call opening animation here
        profilesButton.SetActive(false);
        endTurnButton.SetActive(false);
        fearMeter.SetActive(false);

        hideOptionsButton.SetActive(true);

        SpawnNewOptions();
        
    }


    private void SpawnNewOptions()
    {
        // If there are less than 3 missions to choose from left reset the list to what it was on game start
        if (AvailableOptionPrefabs.Count < 3)
        {
            AvailableOptionPrefabs = AvailableOptionsStartList;
        }


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

        hideOptionsButton.SetActive(false);
    }

    public void Reroll()
    {
        if (rerollsAvailable > 0)
        {
            rerollsAvailable--;
            SpawnNewOptions();
        }
    }



}
