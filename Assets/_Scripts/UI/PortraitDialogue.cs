using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for managing activating dialogue by clicking on character Portraits.
/// 
/// </summary>
public class PortraitDialogue : MonoBehaviour
{
    private Button button;
    public Color unavaliableColor;

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
        unavaliableColor = button.colors.pressedColor;
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
            //DB.AlreadyTalked();
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
                PortraitFade portraitFade = gameObject.GetComponent<PortraitFade>();
                
                portraitFade.clicked = true;

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
