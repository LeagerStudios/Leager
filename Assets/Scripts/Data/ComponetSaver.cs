using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponetSaver : MonoBehaviour
{

    public static ComponetSaver self;
    public List<string[]> SavedData = new List<string[]>();
    public List<string> SavedDataName = new List<string>();

    void Start()
    {
        if (self != null) Destroy(gameObject);
        else
        {
            self = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Save(string[] data, string dataName)
    {
        if (SavedDataName.Contains(dataName))
        {
            SavedData[SavedDataName.IndexOf(dataName)] = data;
        }
        else
        {
            SavedData.Add(data);
            SavedDataName.Add(dataName);
        }
    }

    public bool Load(string dataName, out string[] data)
    {
        if (SavedDataName.Contains(dataName))
        {
            data = SavedData[SavedDataName.IndexOf(dataName)];
            return true;
        }
        else
        {
            data = null;
            return false;
        }

    }

    public bool DeleteData(string dataName)
    {
        if (SavedDataName.Contains(dataName))
        {
            SavedData.Remove(SavedData[SavedDataName.IndexOf(dataName)]);
            SavedDataName.Remove(dataName);
            return true;
        }
        else
        {
            return false;
        }
    }
}
