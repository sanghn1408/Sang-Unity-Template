using System.Collections;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu(fileName = "SaveSystem", menuName = "Game/SaveSystem")]
public class SaveSystem : ScriptableObject
{
    [SerializeField] private string _saveFilename = "save.mygame";
    [SerializeField] private string _backupSaveFilename = "save.mygame.bak";

    public SaveData SaveData;

    public void InitData()
    {
        Debug.Log("Init data successful");
        SaveData = new SaveData();
    }

    public bool IsExists()
    {
        return FileManager.IsExists(_saveFilename);
    }

    public bool LoadSaveDataFromDisk()
    {
        if (FileManager.LoadFromFile(_saveFilename, out var json))
        {
            SaveData = JsonConvert.DeserializeObject<SaveData>(json);
            return true;
        }

        return false;
    }


    public void SaveDataToDisk()
    {
        if (FileManager.MoveFile(_saveFilename, _backupSaveFilename))
        {
            if (FileManager.WriteToFile(_saveFilename, JsonConvert.SerializeObject(SaveData)))
            {
                //Debug.Log("Save successful");
            }
        }
    }

    public void WriteEmptySaveFile()
    {
        FileManager.WriteToFile(_saveFilename, "");
    }

    public void DeleteFile()
    {
        FileManager.DeleteFile(_saveFilename);
        Debug.Log("Clear data successful");
    }
}
