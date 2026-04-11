using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioCueEventChannelSO _playSFXOn = default;
    [SerializeField] private AudioConfigurationSO _audioConfig = default;
    [SerializeField] private AudioConfigurationSO _audioConfig_3D = default;
    [SerializeField] private SoundDataDictionary _mapper;


    public AudioCueKey PlaySFX(AudioCueSO audioCueSO)
    {
        return _playSFXOn.RaisePlayEvent(audioCueSO, _audioConfig);
    }

    public AudioCueKey PlaySFX(TypeSound typeSound, AudioConfigurationSO audioConfig)
    {
        return _playSFXOn.RaisePlayEvent(_mapper[typeSound], audioConfig);
    }

    public AudioCueKey PlaySFX(TypeSound typeSound)
    {
        Debug.LogError("typeSound" + typeSound.ToString());
        return _playSFXOn.RaisePlayEvent(_mapper[typeSound], _audioConfig);
    }

    public AudioCueKey PlaySFX(TypeSound typeSound, Vector3 position)
    {
        return _playSFXOn.RaisePlayEvent(_mapper[typeSound], _audioConfig_3D, position);
    }

    public void StopSFX(AudioCueKey key)
    {
        _playSFXOn.RaiseStopEvent(key);
    }
}

public enum TypeSound
{
    SFX_Click,
    SFX_Open,
    SFX_Cart,
    SFX_Enfuse,
    SFX_Cup,
    SFX_Sort,
    SFX_Bullet,
    SFX_PhaoGiay
}

[System.Serializable] public class SoundDataDictionary : UnitySerializedDictionary<TypeSound, AudioCueSO> { }
