using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for managing activating dialogue by clicking on character Portraits.
/// Some spaghetti associations here, could use refactor along with ClickAI later.
/// </summary>
public class PortraitDialogue : MonoBehaviour
{
    private Button button;


    AICamera aiCam;
    // AI object with dialogue bank and ai cam
    [SerializeField] GameObject character;
    [SerializeField] GameObject playerControls;
    [SerializeField] public Books selfBook;

    private DialogueBank DB;
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(DetermineAction);
        DB = character.GetComponent<DialogueBank>();

        aiCam = character.transform.GetChild(0).GetComponent<AICamera>();
        selfBook = gameObject.GetComponent<Books>();
        print(selfBook.gameObject.name);
    }




    public void DetermineAction()
    {
        if (UIManagerLite.GetCurrentState() == UIManagerLite.GameState.BoardBase)
        {
            BeginDialogue();
        }

        else if(UIManagerLite.GetCurrentState() == UIManagerLite.GameState.Storyboard)
        {
            selfBook.ChangeBook();
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

        if (playerControls.GetComponent<ClickAI>()._inConversation == false)
        {
            {
                DB.alreadyTalked = true;
                PortraitFade portraitFade = character.GetComponent<PortraitFade>();
                portraitFade.StartCoroutine(portraitFade.FadeOut());

                ClickAI.S.SetAICam(aiCam);
                aiCam.RotBustToCam();


                AudioManager.PlaySFX(SFXType.CharacterSelection);


                ClickAI.S._clickedAI = true;
                var virtualCam = DB.gameObject.GetComponentInChildren<CinemachineVirtualCamera>(true);
                if (virtualCam)
                {
                    virtualCam.gameObject.SetActive(true);
                    virtualCam.enabled = true;
                }
                ClickAI.S.activeCamera = virtualCam;
                ClickAI.S.playerCam.enabled = false;

                UIManagerLite.TransitionToState(UIManagerLite.GameState.Dialogue);
            }
        }
    }

    
    

}
