using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GamePlayController : Singleton<GamePlayController>
{
    [SerializeField] private Volume _volume;

    [Header("Profile")]
    [SerializeField] private VolumeProfile _volumeProfileFocus;
    [SerializeField] private VolumeProfile _volumeProfileNormal;

    [Header("Listening")]
    [SerializeField] private VoidEventChannelSO _onGameInit;
    [SerializeField] private VoidEventChannelSO _onGameActive;
    [SerializeField] private VoidEventChannelSO _onGameWin;


    private void OnEnable()
    {
        _onGameInit.OnEventRaised += OnGameInit;
        _onGameActive.OnEventRaised += OnGameActive;
        _onGameWin.OnEventRaised += OnGameWin;
    }

    private void OnDisable()
    {
        _onGameInit.OnEventRaised -= OnGameInit;
        _onGameActive.OnEventRaised -= OnGameActive;
        _onGameWin.OnEventRaised -= OnGameWin;
    }

    private void OnGameInit()
    {
        
    }

    private void OnGameActive()
    {
       
    }

    private void OnGameWin()
    {

    }


    public void ChangeToNornal()
    {
        _volume.profile = _volumeProfileNormal;
    }

    public void ChangeToFocus()
    {
        _volume.profile = _volumeProfileFocus;
    }

}
