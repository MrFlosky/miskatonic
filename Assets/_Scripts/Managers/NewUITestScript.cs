using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewUITestScript : MonoBehaviour
{
    [Header("Buttons")]
    public Button mainButton;
    public Button dialogueButton;
    public Button infoButton;
    public Button fearsButton;

    [Header("Menus")]
    public GameObject dialogueMenu;
    public GameObject infoMenu;
    public GameObject fearsMenu;

    [Header("Close Panel")]
    public GameObject closePanel; // background overlay

    void Start()
    {
        // Initial state
        dialogueButton.gameObject.SetActive(false);
        infoButton.gameObject.SetActive(false);
        fearsButton.gameObject.SetActive(true); // fears button is visible?

        dialogueMenu.SetActive(false);
        infoMenu.SetActive(false);
        fearsMenu.SetActive(false);
        closePanel.SetActive(false);

        // Hook up listeners
        mainButton.onClick.AddListener(OpenChoiceButtons);
        dialogueButton.onClick.AddListener(OpenDialogueMenu);
        infoButton.onClick.AddListener(OpenInfoMenu);
        fearsButton.onClick.AddListener(OpenFearsMenu);

        // close panel clears everything
        closePanel.GetComponent<Button>().onClick.AddListener(CloseAllMenus);
    }

    public void OpenChoiceButtons()
    {
        dialogueButton.gameObject.SetActive(true);
        infoButton.gameObject.SetActive(true);
    }

    public void OpenDialogueMenu()
    {
        CloseAllMenus();
        dialogueMenu.SetActive(true);
        closePanel.SetActive(true);
        

    }

    public void OpenInfoMenu()
    {
        CloseAllMenus();
        infoMenu.SetActive(true);
        closePanel.SetActive(true);
        

    }

    public void OpenFearsMenu()
    {
        CloseAllMenus();
        fearsMenu.SetActive(true);
        closePanel.SetActive(true);
        

    }

    public void CloseAllMenus()
    {
        dialogueMenu.SetActive(false);
        infoMenu.SetActive(false);
        fearsMenu.SetActive(false);
        closePanel.SetActive(false);
        

    }
}