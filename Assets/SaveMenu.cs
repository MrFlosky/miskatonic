using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveMenu : MonoBehaviour
{


    public TextMeshProUGUI inputFieldText;
    public void SendSave()
    {
        Debug.LogWarning("Trying to save with name: '" +  inputFieldText.text + "'!!");
        SaveManager.S.Save(inputFieldText.text);
    }
}
