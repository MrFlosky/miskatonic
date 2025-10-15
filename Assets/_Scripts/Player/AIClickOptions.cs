// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class AIClickOptions : MonoBehaviour
// {
//     [Header("UI References")]
//     [SerializeField] private GameObject optionPanel;
//     [SerializeField] private Button talkButton;
//     [SerializeField] private Button otherOptionButton;

//     private DialogueBank _currentBank;

//     void Start()
//     {
//         // Hook up button listeners
//         if (talkButton != null)
//             talkButton.onClick.AddListener(OnTalkClicked);

//         if (otherOptionButton != null)
//             otherOptionButton.onClick.AddListener(OnOtherOptionClicked);

//         // Hide panel by default
//         if (optionPanel != null)
//             optionPanel.SetActive(false);
//     }

//     /// <summary>
//     /// Called by ClickAI when an AI is selected.
//     /// Pass in that AI�s DialogueBank so the buttons know who to talk to.
//     /// </summary>
//     public void ShowOptions(DialogueBank bank)
//     {
//         _currentBank = bank;

//         if (optionPanel != null)
//             optionPanel.SetActive(true);
//     }

//     public void HideOptions()
//     {
//         if (optionPanel != null)
//             optionPanel.SetActive(false);
//     }

//     private void OnTalkClicked()
//     {
//         if (_currentBank != null)
//         {
//             _currentBank.PlayRandomDialogue();   //  starts dialogue
//         }
//         else
//         {
//             Debug.LogWarning("No DialogueBank found for this AI.");
//         }
//         HideOptions();
//     }

//     private void OnOtherOptionClicked()
//     {
//         if (_currentBank != null)
//         {
//             // TODO: Replace this with real logic
//             _currentBank.AlreadyTalked(); // Example: use existing DialogueBank function
//             // Or: _currentBank.OutOfDialogues();
//         }
//         else
//         {
//             Debug.LogWarning("No DialogueBank found for this AI.");
//         }
//         HideOptions();
//     }
// }
