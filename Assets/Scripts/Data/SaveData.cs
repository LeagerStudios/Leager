using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{

    public string[] SavedData;

    public SaveData(string[] saves)
    {
        SavedData = saves;
    }
}
