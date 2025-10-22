using UnityEngine;

/// <summary>
/// Holds all the data used to describe a save, primarily to be used in rendering UI
/// </summary>
[System.Serializable]
public class SaveDisplayData
{
    string _name;
    string _date;
    string _turnNum;

    public SaveDisplayData(string name, string date, string turnNum)
    {
        _name = name;
        _date = date;
        _turnNum = turnNum;
    }
}