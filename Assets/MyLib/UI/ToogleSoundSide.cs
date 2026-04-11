using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToogleSoundSide : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO _boolEventChannelSO;
    [SerializeField] private TypeToogle _type= default;
    [SerializeField] RectTransform _scrollRect=default;
    [SerializeField] private float _deltaScroll=1f;
    [SerializeField] private float _timeScroll=0.5f;

    private Vector3 _initPos;
    private bool _isOn;

    private void Start()
    {
        _initPos = _scrollRect.anchoredPosition3D;

        switch (_type)
        {
            case TypeToogle.ToogleHapic:
                _isOn = DataManager.I.SaveData.IsHapic;
                break;
            case TypeToogle.ToogleSound:
                _isOn = DataManager.I.SaveData.IsSound;
                break;
            case TypeToogle.TooggleMusic:
                _isOn = DataManager.I.SaveData.IsMusic;
                break;
            case TypeToogle.ToogleMasterSound:
                _isOn = DataManager.I.SaveData.IsMasterSound;
                break;
        }

        UpdateUI(0f);
    }

    private void UpdateUI(float time)
    {
        if (!_isOn)
        {
            _scrollRect.DOAnchorPos3DX(_initPos.x - _deltaScroll, time);
        }
        else
        {
            _scrollRect.DOAnchorPos3DX(_initPos.x, time);
        }
    }

    public void ClickMe()
    {
        _isOn = !_isOn;
        UpdateUI(_timeScroll);

        switch (_type)
        {
            case TypeToogle.ToogleHapic:
                DataManager.I.SaveData.IsHapic = _isOn;
                break;
            case TypeToogle.ToogleSound:
                DataManager.I.SaveData.IsSound = _isOn;
                break;
            case TypeToogle.TooggleMusic:
                DataManager.I.SaveData.IsMusic = _isOn;
                break;
            case TypeToogle.ToogleMasterSound:
                DataManager.I.SaveData.IsMasterSound = _isOn;
                break;
        }
        _boolEventChannelSO?.RaiseEvent(_isOn);
    }
}

public enum TypeToogle
{
    TooggleMusic,
    ToogleSound,
    ToogleHapic,
    ToogleMasterSound
}