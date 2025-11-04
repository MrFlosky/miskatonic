using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveMenu : MonoBehaviour
{


    public TextMeshProUGUI inputFieldText;

    public int slotToSaveIn;
    public void SendSave()
    {
        Debug.LogWarning("Trying to save with name: '" + inputFieldText.text + "'!!");
        SaveManager.S.Save(inputFieldText.text, slotToSaveIn);
    }

    public void SetToSlot1()
    {
        slotToSaveIn = 1;
    }

    public void SetToSlot2()
    {
        slotToSaveIn = 2;
    }

    public void SetToSlot3()
    {
        slotToSaveIn = 3;
    }

}
