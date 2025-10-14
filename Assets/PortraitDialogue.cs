using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitDialogue : MonoBehaviour
{
    Button button;
    
    ClickAI CAI;
    public GameObject character;
    DialogueBank DB;
    [SerializeField] private AIClickOptions aiClickOptions;
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(BeginDialogue);
        CAI = ClickAI.S;
        DB = character.GetComponent<DialogueBank>();
    }

    private void Update()
    {
        if(CAI== null)
        {
            CAI= ClickAI.S;
        }
    }

    void BeginDialogue()
    {
        if (DB.alreadyTalked)
        {
            DB.AlreadyTalked();
            return;
        }

        if (DB.GetRemainingDialogueCount() == 0)
        {
            DB.OutOfDialogues();
            return;
        }

        DB.alreadyTalked = true;
        PortraitFade portraitFade = character.GetComponent<PortraitFade>();
        portraitFade.StartCoroutine(portraitFade.FadeOut());

        AudioManager.PlaySFX(SFXType.CharacterSelection);
        print(CAI.gameObject.name);
        CAI._clickedAI = true;
        var virtualCam = DB.gameObject.GetComponentInChildren<CinemachineVirtualCamera>(true);
        if (virtualCam)
        {
            virtualCam.gameObject.SetActive(true);
            virtualCam.enabled = true;
        }
        CAI.activeCamera = virtualCam;
        CAI.playerCam.enabled = false;


        aiClickOptions.ShowOptions(DB);
    }


}
