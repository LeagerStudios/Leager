using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public static class DataSaver
{
    public static string version = Application.version;

    public static void SaveStats(string[] statsSave, string saveDirectory)
    {
        BinaryFormatter ensambler = new BinaryFormatter();
        string directory = saveDirectory;
        FileStream file = new FileStream(directory, FileMode.Create);

        SaveData dataSave = new SaveData(statsSave);
        ensambler.Serialize(file, dataSave);
        file.Close();
    }

    public static void CreateFolder(string directory)
    {
        DirectoryInfo directoryInfo = Directory.CreateDirectory(directory);
    }

    public static bool CheckIfFileExists(string file)
    {
        bool ret = false;

        if (File.Exists(file))
        {
            ret = true;
        }
        else if (Directory.Exists(file))
        {
            ret = true;
        }

        return ret;
    }

    public static int FilesInside(string folder)
    {
        return Directory.GetFiles(folder).Length;
    }

    public static void DeleteFile(string fileDirectory)
    {
        if (Directory.Exists(fileDirectory)) Directory.Delete(fileDirectory, true);
        else File.Delete(fileDirectory);
    }

    public static SaveData LoadStats(string directory)
    {
        if (File.Exists(directory))
        {
            BinaryFormatter disensambler = new BinaryFormatter();
            FileStream file = new FileStream(directory, FileMode.Open);

            SaveData loaded = (SaveData)disensambler.Deserialize(file);
            file.Close();
            return loaded;
        }
        else
        {
            Debug.LogError("There's no file in " + directory);
            return null;
        }
    }

    public static void CreateTxt(string directory, string[] txt)
    {
        if (!File.Exists(directory))
        {
            File.WriteAllLines(directory, txt);
        }
        else Debug.LogAssertion(directory + " already exists!");
    }

    public static void ModifyTxt(string directory, string[] txt)
    {
        if (!File.Exists(directory)) Debug.LogAssertion(directory + "doesn't exist!");
        else
        {
            File.WriteAllLines(directory, txt);
        }
    }

    public static void AddLineToTxt(string directory, string txt)
    {
        if (!File.Exists(directory)) Debug.LogError(directory + "doesn't exist!");
        else
        {
            List<string> exitTxtList = new List<string>(ReadTxt(directory))
            {
                txt
            };
            ModifyTxt(directory, exitTxtList.ToArray());
        }
    }

    public static void DeleteLastLineInTxt(string directory)
    {
        if (!File.Exists(directory)) Debug.LogError(directory + "doesn't exist!");
        else
        {
            List<string> exitTxtList = new List<string>(ReadTxt(directory));
            exitTxtList.RemoveAt(exitTxtList.Count - 1);
            string[] exitTxt = exitTxtList.ToArray();
            ModifyTxt(directory, exitTxt);
        }
    }

    public static string[] ReadTxt(string directory)
    {
        return File.ReadAllLines(directory);
    }

    public static int LinesInTxt(string directory)
    {
        if (!File.Exists(directory)) Debug.LogError(directory + "doesn't exist!");
        else
        {
            List<string> TxtList = new List<string>(ReadTxt(directory));
            return TxtList.Count;
        }
        return -1;
    }

    public static void CopyPasteTxt(string copy, string paste)
    {
        string[] copied = ReadTxt(copy);
        ModifyTxt(paste, copied);
        Debug.Log(copy + " copied to " + paste);
    }

    public static Sprite LoadPng(string directory)
    {
        byte[] pngDat = File.ReadAllBytes(directory);
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(pngDat);
        Rect rect = new Rect(0, 0, tex.width, tex.height);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
    }

    public static string GetFileInsideDirectory(string directory, int idx)
    {
        if (FilesInside(directory) < 1) return "";
        return Directory.GetDirectories(directory)[idx];
    }

    public static void SerializeAt<T>(T toSerialize, string saveDirectory)
    {
        BinaryFormatter ensambler = new BinaryFormatter();
        string directory = saveDirectory;
        FileStream file = new FileStream(directory, FileMode.Create);

        ensambler.Serialize(file, toSerialize);
        file.Close();
    }

    public static T DeSerializeAt<T>(string directory)
    {
        if (File.Exists(directory))
        {
            BinaryFormatter disensambler = new BinaryFormatter();
            FileStream file = new FileStream(directory, FileMode.Open);

            T loaded = (T)disensambler.Deserialize(file);
            file.Close();
            return loaded;
        }
        else
        {
            Debug.LogError("There's no file in " + directory);
            return default;
        }
    }
}
