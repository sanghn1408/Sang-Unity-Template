using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : Singleton<MusicPlayer>
{
    [SerializeField] private VoidEventChannelSO _onSceneReady = default;
    [SerializeField] private AudioCueEventChannelSO _playMusicOn = default;
    //[SerializeField] private AudioCueSO _musicTrack = default;
    //[SerializeField] private AudioCueSO _musicTrack2 = default;
    //[SerializeField] private AudioConfigurationSO _audioConfig = default;



    private AudioCueKey _keyBG1;
    private AudioCueKey _keyBG2;

    private void OnEnable()
    {
        _onSceneReady.OnEventRaised += PlayMusic;
        // PlayMusic();
    }

    private void OnDisable()
    {
        _onSceneReady.OnEventRaised -= PlayMusic;

        _playMusicOn.RaiseStopEvent(_keyBG1);
    }

    public void PlayMusic()
    {


        //if (GameModeManager.Instance.GameMode == TypeMode.FireworkIdle)
        //{

        //    _playMusicOn.RaisePauseEvent(_keyBG1);
        //    _keyBG2 = _playMusicOn.RaisePlayEventMusic(_musicTrack2, _audioConfig);
        //    if (!DataManager.I.SaveData.IsMusic)
        //    {
        //        _playMusicOn.RaisePauseEvent(_keyBG2);
        //    }
        //}
        //else
        //{
        //    _playMusicOn.RaisePauseEvent(_keyBG2);
        //    _keyBG1 = _playMusicOn.RaisePlayEventMusic(_musicTrack, _audioConfig);
        //    if (!DataManager.I.SaveData.IsMusic)
        //    {
        //        _playMusicOn.RaisePauseEvent(_keyBG1);
        //    }

        //}

    }

    public void  PauseMusic()
    {
        _playMusicOn.RaisePauseEvent(_keyBG1);
    }

    //public void ResumeMusic()
    //{
    //    if (GameModeManager.Instance.GameMode == TypeMode.FireworkIdle)
    //    {

    //        _playMusicOn.RaisePauseEvent(_keyBG1);
    //       _playMusicOn.RaiseResumeEvent(_keyBG2);
            
    //    }
    //    else
    //    {
    //        _playMusicOn.RaisePauseEvent(_keyBG2);
    //         _playMusicOn.RaiseResumeEvent(_keyBG1);
            

    //    }
    //}
}
