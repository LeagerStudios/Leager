using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponetSaver : MonoBehaviour {

    public List<string[]> SavedData = new List<string[]>();
    public List<string> SavedDataName = new List<string>();

	void Start () {
		
	}
	
	void Update () {
		
	}

    public void SaveData(string[] data, string dataName)
    {
        SavedData.Add(data);
        SavedDataName.Add(dataName);
    }

    public string[] LoadData(string dataName)
    {
        return SavedData[SavedDataName.IndexOf(dataName)];
    }

    public void DeleteData(string dataName)
    {
        SavedData.Remove(SavedData[SavedDataName.IndexOf(dataName)]);
        SavedDataName.Remove(dataName);
    }
}
