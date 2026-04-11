using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class DataManager : Singleton<DataManager>
{
    [SerializeField] private SaveSystem _saveSystem;

    public SaveData SaveData { get { return _saveSystem.SaveData; } }

    private int intrate = -1;
    public int IntRate
    {
        get => intrate;
        set
        {
            intrate = value;
            PlayerPrefs.SetInt("intrate", intrate);
        }
    }
    private void Start()
    {
        intrate = PlayerPrefs.GetInt("NumberPlay", IntRate);
    }
    protected override void OnRegisterInstance()
    {
        base.OnRegisterInstance();
        Init();
    }
    public void Init()
    {
        bool hasSaveData = _saveSystem.IsExists();
        if (hasSaveData == false)
        {
            _saveSystem.InitData();
            _saveSystem.WriteEmptySaveFile();
            Save();
        }
        else _saveSystem.LoadSaveDataFromDisk();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            Save();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus == false)
            Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    [ContextMenu("SaveData")]
    private void Save()
    {
        _saveSystem.SaveDataToDisk();
    }

    [ContextMenu("DeleteData")]
    private void DeleteData()
    {
        _saveSystem.DeleteFile();
    }
    
   
}
