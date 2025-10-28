using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadMenu : MonoBehaviour
{
    // Will need to improve this at some point, currently just loads the 3 most recent saves

    public TextMeshProUGUI[] saveNames;
    public TextMeshProUGUI[] turnCounts;
    public TextMeshProUGUI[] dates;


    private void LoadSavesIntoLoadMenu()
    {
        SaveDisplayData[] saveDisplayDatas = SaveManager.S.GetSaveInformation();
        for (int i = 0; i < 3; i++)
        {
            if (i+1 > saveDisplayDatas.Length)
            {
                saveNames[i].text = "No Data";
            } else
            {
                saveNames[i].text = saveDisplayDatas[i].name;
                turnCounts[i].text = saveDisplayDatas[i].turnNum;
                dates[i].text = saveDisplayDatas[i].date;
            }
                
        }
    }

    private void Start()
    {
        LoadSavesIntoLoadMenu();
    }

    public void LoadSave1()
    {
        SaveManager.S.Load(saveNames[0].text);
    }

    public void LoadSave2()
    {
        SaveManager.S.Load(saveNames[1].text);
    }

    public void LoadSave3()
    {
        SaveManager.S.Load(saveNames[2].text);
    }

}
